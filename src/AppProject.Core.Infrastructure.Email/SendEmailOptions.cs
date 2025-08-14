using System;

namespace AppProject.Core.Infrastructure.Email;

public class SendEmailOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string FromEmailAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;
}
