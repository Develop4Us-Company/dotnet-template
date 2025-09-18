using System;

namespace AppProject.Core.Contracts;

public class UserInfo
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsSystemAdmin { get; set; }
}
