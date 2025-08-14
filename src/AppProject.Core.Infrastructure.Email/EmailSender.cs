using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AppProject.Core.Infrastructure.Email;

public class EmailSender(
    ISendGridClient sendGridClient,
    IOptions<SendEmailOptions> sendEmailOptions,
    ILogger<EmailSender> logger)
    : IEmailSender
{
    public async Task<bool> SendEmailAsync(
        string subject,
        string body,
        IEnumerable<string>? to = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? fromEmailAddress = null,
        string? fromName = null,
        IEnumerable<EmailAttachment>? emailAttachments = null,
        CancellationToken cancellationToken = default)
    {
        fromEmailAddress ??= sendEmailOptions.Value?.FromEmailAddress;
        fromName ??= sendEmailOptions.Value?.FromName;

        if (string.IsNullOrEmpty(fromEmailAddress))
        {
            throw new InvalidOperationException("From email address must be provided.");
        }

        if (string.IsNullOrEmpty(sendEmailOptions.Value?.ApiKey))
        {
            throw new InvalidOperationException("SendEmail API key must be configured.");
        }

        var message = new SendGridMessage
        {
            From = new EmailAddress(fromEmailAddress, fromName),
            Subject = subject,
            HtmlContent = body,
        };

        var addedRecipients = false;

        if (to?.Any() == true)
        {
            addedRecipients = true;

            foreach (var recipient in to)
            {
                message.AddTo(new EmailAddress(recipient));
            }
        }

        if (cc?.Any() == true)
        {
            addedRecipients = true;

            foreach (var recipient in cc)
            {
                message.AddCc(new EmailAddress(recipient));
            }
        }

        if (bcc?.Any() == true)
        {
            addedRecipients = true;

            foreach (var recipient in bcc)
            {
                message.AddBcc(new EmailAddress(recipient));
            }
        }

        if (!addedRecipients)
        {
            logger.LogWarning("No recipients were added to the email. At least one recipient (To, CC, BCC) is required.");
            return false;
        }

        if (emailAttachments?.Any() == true)
        {
            foreach (var attachment in emailAttachments)
            {
                var sendGridAttachment = new Attachment
                {
                    Content = attachment.Content,
                    Filename = attachment.FileName,
                    Type = attachment.Type,
                    Disposition = attachment.Disposition,
                    ContentId = attachment.ContentId
                };

                message.AddAttachment(sendGridAttachment);
            }
        }

        var response = await sendGridClient.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Email was not sent successfully. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }
}
