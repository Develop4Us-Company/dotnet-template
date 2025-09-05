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

public class StateService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IStateService
{
    public async Task<EntityResponse<State>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var state = await databaseRepository.GetFirstOrDefaultAsync<TbState, State>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (state == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<State>
        {
            Entity = state
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<State> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateStateAsync(request.Entity, cancellationToken);

        var tbState = request.Entity.Adapt<TbState>();
        await databaseRepository.InsertAndSaveAsync(tbState, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbState.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<State> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateStateAsync(request.Entity, cancellationToken);

        var tbState = await databaseRepository.GetFirstOrDefaultAsync<TbState>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbState == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbState);

        await databaseRepository.UpdateAndSaveAsync(tbState, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbState.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbState = await databaseRepository.GetFirstOrDefaultAsync<TbState>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbState == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbState, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateStateAsync(State state, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<TbState>(
            query => query.Where(x =>
                x.CountryId == state.CountryId
                && x.Name == state.Name
                && x.Id != state.Id), cancellationToken))
        {
            throw new AppException(ExceptionCode.General_State_DuplicateName);
        }
    }
}
