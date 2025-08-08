using System;

namespace AppProject.Core.Contracts;

public class UserInfo
{
    required public Guid UserId { get; set; }

    required public string UserName { get; set; }

    required public string Email { get; set; }

    required public bool IsSystemAdmin { get; set; }
}
