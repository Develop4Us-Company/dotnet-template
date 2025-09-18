using System;

namespace AppProject.Core.Infrastructure.Email;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(
        string subject,
        string body,
        IEnumerable<string>? to = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? fromEmailAddress = null,
        string? fromName = null,
        IEnumerable<EmailAttachment>? emailAttachments = null,
        CancellationToken cancellationToken = default);
}
