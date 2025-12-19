using System;
using System.Linq.Expressions;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Mapster;

namespace AppProject.Core.Services.General;

public class CountryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, ICountryService
{
    public async Task<EntityResponse<Country>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var country = await databaseRepository.GetFirstOrDefaultAsync<TbCountry, Country>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (country == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<Country>
        {
            Entity = country
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCountryAsync(request.Entity, cancellationToken);

        var tbCountry = request.Entity.Adapt<TbCountry>();
        await databaseRepository.InsertAndSaveAsync(tbCountry, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCountry.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCountryAsync(request.Entity, cancellationToken);

        var tbCountry = await databaseRepository.GetFirstOrDefaultAsync<TbCountry>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbCountry == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbCountry);

        await databaseRepository.UpdateAndSaveAsync(tbCountry, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCountry.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbCountry = await databaseRepository.GetFirstOrDefaultAsync<TbCountry>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbCountry == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbCountry, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateCountryAsync(Country country, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<TbCountry>(
            query => query.Where(x => x.Name == country.Name && x.Id != country.Id),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.General_Country_DuplicateName);
        }
    }
}
