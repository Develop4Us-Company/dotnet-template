using System;

namespace AppProject.Core.Contracts;

public interface IUserContext
{
    public Task<UserInfo> GetSystemAdminUserAsync(CancellationToken cancellationToken = default);

    public Task<UserInfo> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
