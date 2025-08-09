using System;
using System.Security.Claims;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace AppProject.Core.API.Auth;

public class UserContext(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext applicationDbContext,
    ISystemAdminUserContext systemAdminUserContext)
    : IUserContext
{
    private UserInfo? currentUser;

    public async Task<UserInfo> GetCurrentUserAsync()
    {
        if (this.currentUser != null)
        {
            return this.currentUser;
        }

        var systemAdminUser = await systemAdminUserContext.GetSystemAdminUserAsync();

        var claimsPrincipal = httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal?.Identity?.IsAuthenticated == true)
        {
            var name = claimsPrincipal.FindFirstValue("name");
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? claimsPrincipal.FindFirstValue("email");

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException();
            }

            if (email == systemAdminUser.Email)
            {
                this.currentUser = systemAdminUser;
                return this.currentUser;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = email;
            }

            var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
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
            // Not authenticated, return system admin user as fallback
            this.currentUser = systemAdminUser;
            return this.currentUser;
        }
    }
}
