using System;

namespace AppProject.Core.Contracts;

public interface ISystemAdminUserContext
{
    public Task<UserInfo> GetSystemAdminUserAsync();
}
