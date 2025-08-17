using System;
using AppProject.Models.Auth;

namespace AppProject.Core.Services.Auth;

public interface IPermissionService : IScopedService
{
    Task ValidateCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default);

    Task<bool> HasCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<PermissionType>> GetCurrentUserPermissionsAsync(PermissionContext? context = null, CancellationToken cancellationToken = default);
}
