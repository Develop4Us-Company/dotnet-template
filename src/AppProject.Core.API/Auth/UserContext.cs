using System;
using System.Security.Claims;
using AppProject.Core.API.SettingOptions;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AppProject.Core.API.Auth;

public class UserContext(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext applicationDbContext,
    IOptions<SystemAdminUserOptions> systemAdminUserOptions)
    : IUserContext
{
    private UserInfo? currentUser;
    private UserInfo? systemAdminUser;

    public async Task<UserInfo> GetCurrentUserAsync()
    {
        if (this.currentUser != null)
        {
            return this.currentUser;
        }

        var claimsPrincipal = httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal?.Identity?.IsAuthenticated == true)
        {
            var name = claimsPrincipal.FindFirstValue("name");
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? claimsPrincipal.FindFirstValue("email");

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = email;
            }

            if (email == systemAdminUserOptions.Value.Email)
            {
                this.currentUser = await this.GetSystemAdminUserAsync();
                return this.currentUser;
            }

            var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                var systemAdminUser = await this.GetSystemAdminUserAsync();

                user = new TbUser
                {
                    Name = name,
                    Email = email,
                    IsSystemAdmin = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = systemAdminUser.UserId,
                    CreatedByUserName = systemAdminUser.UserName
                };

                applicationDbContext.Users.Add(user);
                await applicationDbContext.SaveChangesAsync();
            }

            this.currentUser = new UserInfo
            {
                UserId = user.Id,
                UserName = user.Name,
                Email = user.Email,
                IsSystemAdmin = user.IsSystemAdmin
            };

            return this.currentUser;
        }
        else
        {
            return await this.GetSystemAdminUserAsync();
        }
    }

    public async Task<UserInfo> GetSystemAdminUserAsync()
    {
        if (this.systemAdminUser != null)
        {
            return this.systemAdminUser;
        }

        if (systemAdminUserOptions.Value is null
            || string.IsNullOrEmpty(systemAdminUserOptions.Value.Name)
            || string.IsNullOrEmpty(systemAdminUserOptions.Value.Email))
        {
            throw new ArgumentException("SystemAdminUser configuration is not set properly.");
        }

        var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IsSystemAdmin);

        if (user == null)
        {
            var adminUserId = Guid.NewGuid();

            user = new TbUser
            {
                Id = adminUserId,
                Name = systemAdminUserOptions.Value.Name!,
                Email = systemAdminUserOptions.Value.Email!,
                IsSystemAdmin = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminUserId,
                CreatedByUserName = systemAdminUserOptions.Value.Name!
            };

            applicationDbContext.Users.Add(user);
            await applicationDbContext.SaveChangesAsync();
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
