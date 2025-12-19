using System;
using AppProject.Core.Contracts;
using AppProject.Exceptions;
using AppProject.Models.Auth;

namespace AppProject.Core.Services.Auth;

public class PermissionService(IUserContext userContext)
    : BaseService, IPermissionService
{
    public async Task ValidateCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default)
    {
        if (!await this.HasCurrentUserPermissionAsync(permissionType, context, cancellationToken))
        {
            throw new AppException(ExceptionCode.SecurityValidation);
        }
    }

    public async Task<bool> HasCurrentUserPermissionAsync(PermissionType permissionType, PermissionContext? context = null, CancellationToken cancellationToken = default)
    {
        var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);

        if (currentUser.IsSystemAdmin)
        {
            return true;
        }

        // Implement your logic to check user permissions
        switch (permissionType)
        {
            case PermissionType.System_ManageSettings when currentUser.IsSystemAdmin:
                return true;

            // Add more cases for other permissions as needed
            default:
                return false;
        }
    }

    public async Task<IEnumerable<PermissionType>> GetCurrentUserPermissionsAsync(PermissionContext? context = null, CancellationToken cancellationToken = default)
    {
        var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);

        if (currentUser.IsSystemAdmin)
        {
            return Enum.GetValues<PermissionType>();
        }

        // Implement your logic to retrieve current user permissions here
        return Enumerable.Empty<PermissionType>();
    }
}
