using System;
using AppProject.Models.Auth;
using Refit;

namespace AppProject.Web.ApiClient.Auth;

public interface IPermissionClient
{
    [Get("/api/auth/Permission/GetCurrentUserPermissions")]
    public Task<IEnumerable<PermissionType>> GetCurrentUserPermissionsAsync([Query] PermissionContext? context = null, CancellationToken cancellationToken = default);
}
