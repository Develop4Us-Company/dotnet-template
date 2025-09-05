using System;

namespace AppProject.Core.Contracts;

public class UserInfo
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public bool IsSystemAdmin { get; set; }
}
