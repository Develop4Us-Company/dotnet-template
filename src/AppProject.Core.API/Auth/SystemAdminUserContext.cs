using System;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AppProject.Core.API.Auth;

public class SystemAdminUserContext(ApplicationDbContext applicationDbContext)
    : ISystemAdminUserContext
{
    private UserInfo? systemAdminUser;

    public async Task<UserInfo> GetSystemAdminUserAsync()
    {
        if (this.systemAdminUser is not null)
        {
            return this.systemAdminUser;
        }

        var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IsSystemAdmin);

        if (user is null)
        {
            throw new InvalidOperationException("System admin user not found.");
        }

        this.systemAdminUser = new UserInfo
        {
            UserId = user.Id,
            UserName = user.Name,
            Email = user.Email,
            IsSystemAdmin = true
        };

        return this.systemAdminUser;
    }
}
