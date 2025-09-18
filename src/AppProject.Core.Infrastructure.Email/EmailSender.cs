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
        var optionsValue = sendEmailOptions.Value;

        fromEmailAddress ??= optionsValue.FromEmailAddress;
        fromName ??= optionsValue.FromName;

        if (string.IsNullOrWhiteSpace(fromEmailAddress))
        {
            throw new InvalidOperationException("From email address must be provided.");
        }

        if (string.IsNullOrWhiteSpace(optionsValue.ApiKey))
        {
            throw new InvalidOperationException("SendEmail API key must be configured.");
        }

        var message = new SendGridMessage
        {
            From = new EmailAddress(fromEmailAddress, fromName ?? string.Empty),
            Subject = subject,
            HtmlContent = body,
        };

        var addedRecipients = false;

        if (to != null)
        {
            foreach (var recipient in to)
            {
                if (string.IsNullOrWhiteSpace(recipient))
                {
                    continue;
                }

                message.AddTo(new EmailAddress(recipient));
                addedRecipients = true;
            }
        }

        if (cc != null)
        {
            foreach (var recipient in cc)
            {
                if (string.IsNullOrWhiteSpace(recipient))
                {
                    continue;
                }

                message.AddCc(new EmailAddress(recipient));
                addedRecipients = true;
            }
        }

        if (bcc != null)
        {
            foreach (var recipient in bcc)
            {
                if (string.IsNullOrWhiteSpace(recipient))
                {
                    continue;
                }

                message.AddBcc(new EmailAddress(recipient));
                addedRecipients = true;
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
                if (string.IsNullOrWhiteSpace(attachment.Content)
                    || string.IsNullOrWhiteSpace(attachment.FileName)
                    || string.IsNullOrWhiteSpace(attachment.Type))
                {
                    continue;
                }

                var sendGridAttachment = new Attachment
                {
                    Content = attachment.Content,
                    Filename = attachment.FileName,
                    Type = attachment.Type,
                    Disposition = attachment.Disposition,
                };

                if (!string.IsNullOrWhiteSpace(attachment.ContentId))
                {
                    sendGridAttachment.ContentId = attachment.ContentId;
                }

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
