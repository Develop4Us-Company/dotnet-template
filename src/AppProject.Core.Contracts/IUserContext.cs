using System;

namespace AppProject.Core.Contracts;

public interface IUserContext
{
    public Task<UserInfo> GetCurrentUserAsync();
}
