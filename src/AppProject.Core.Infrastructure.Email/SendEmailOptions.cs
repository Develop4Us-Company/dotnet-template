using System;

namespace AppProject.Core.Infrastructure.Email;

public class SendEmailOptions
{
    public string? ApiKey { get; set; }

    public string? FromEmailAddress { get; set; }

    public string? FromName { get; set; }
}
