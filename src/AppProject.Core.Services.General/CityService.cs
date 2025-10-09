using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Mapster;

namespace AppProject.Core.Services.General;

public class CityService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, ICityService
{
    public async Task<EntityResponse<City>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var city = await databaseRepository.GetFirstOrDefaultAsync<TbCity, City>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (city == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<City>
        {
            Entity = city
        };
    }

    public async Task<EntitiesResponse<Neighborhood>> GetNeighborhoodEntitiesAsync(GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var neighborhood = await databaseRepository.GetByConditionAsync<TbNeighborhood, Neighborhood>(
            query => query.Where(x => x.CityId == request.ParentId),
            cancellationToken);

        return new EntitiesResponse<Neighborhood>
        {
            Entities = neighborhood
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<City> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCityAsync(request.Entity, cancellationToken);

        var tbCity = request.Entity.Adapt<TbCity>();
        await databaseRepository.InsertAsync(tbCity, cancellationToken);

        foreach (var neighborhood in request.Entity.ChangedNeighborhoodRequests)
        {
            var tbNeighborhood = neighborhood.Entity.Adapt<TbNeighborhood>();
            tbNeighborhood.CityId = tbCity.Id;
            await databaseRepository.InsertAsync(tbNeighborhood, cancellationToken);
        }

        await databaseRepository.SaveAsync(cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCity.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<City> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCityAsync(request.Entity, cancellationToken);

        var tbCity = await databaseRepository.GetFirstOrDefaultAsync<TbCity>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbCity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbCity);

        await databaseRepository.UpdateAsync(tbCity, cancellationToken);

        foreach (var neighborhood in request.Entity.ChangedNeighborhoodRequests)
        {
            var tbNeighborhood = await databaseRepository.GetFirstOrDefaultAsync<TbNeighborhood>(
                query => query.Where(x => x.Id == neighborhood.Entity.Id),
                cancellationToken);

            if (tbNeighborhood == null)
            {
                tbNeighborhood = neighborhood.Entity.Adapt<TbNeighborhood>();
                tbNeighborhood.CityId = tbCity.Id;
                await databaseRepository.InsertAsync(tbNeighborhood, cancellationToken);
            }
            else
            {
                neighborhood.Entity.Adapt(tbNeighborhood);
                await databaseRepository.UpdateAsync(tbNeighborhood, cancellationToken);
            }
        }

        foreach (var neighborhood in request.Entity.DeletedNeighborhoodRequests)
        {
            var tbNeighborhood = await databaseRepository.GetFirstOrDefaultAsync<TbNeighborhood>(
                query => query.Where(x => x.Id == neighborhood.Id),
                cancellationToken);

            if (tbNeighborhood != null)
            {
                await databaseRepository.DeleteAsync(tbNeighborhood, cancellationToken);
            }
        }

        await databaseRepository.SaveAsync(cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCity.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbCity = await databaseRepository.GetFirstOrDefaultAsync<TbCity>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbCity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbCity, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateCityAsync(City city, CancellationToken cancellationToken = default)
    {
        await this.ValidateNeighborhoodsBelongToCityAsync(city, cancellationToken);

        if (await databaseRepository.HasAnyAsync<TbCity>(
            query => query.Where(x =>
                x.StateId == city.StateId
                && x.Name == city.Name
                && x.Id != city.Id), cancellationToken))
        {
            throw new AppException(ExceptionCode.General_City_DuplicateName);
        }

        var neighborhoodNames = city.ChangedNeighborhoodRequests.Select(x => x.Entity.Name);
        var neighborhoodIds = city.ChangedNeighborhoodRequests.Select(x => x.Entity.Id);

        if (await databaseRepository.HasAnyAsync<TbNeighborhood>(
            query => query.Where(x =>
                x.CityId == city.Id
                && neighborhoodNames.Contains(x.Name)
                && !neighborhoodIds.Contains(x.Id)),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.General_City_Neighborhood_DuplicateName);
        }

        if (neighborhoodNames.Count() != neighborhoodNames.Distinct().Count())
        {
            throw new AppException(ExceptionCode.General_City_Neighborhood_DuplicateName);
        }
    }

    private async Task ValidateNeighborhoodsBelongToCityAsync(City city, CancellationToken cancellationToken = default)
    {
        var neighborhoodIds = city.ChangedNeighborhoodRequests
            .Where(x => x.Entity.Id.GetValueOrDefault() != Guid.Empty)
            .Select(x => x.Entity.Id.GetValueOrDefault())
            .Union(city.DeletedNeighborhoodRequests.Select(x => x.Id));

        if (neighborhoodIds.Any())
        {
            return;
        }

        if (await databaseRepository.HasAnyAsync<TbNeighborhood>(
            query => query.Where(x =>
                neighborhoodIds.Contains(x.Id)
                && x.CityId != city.Id),
            cancellationToken))
        {
            throw new InvalidOperationException();
        }
    }
}
