#if DEBUG
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.AI;
using AppProject.Core.Infrastructure.Email;
using AppProject.Core.Infrastructure.Email.Models;
using AppProject.Core.Infrastructure.Jobs;
using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
public class SampleController(
    IUserContext userContext,
    ILogger<SampleController> logger,
    IEmailTemplateRenderer emailTemplateRenderer,
    IEmailSender emailSender,
    IChatClient chatClient,
    IJobDispatcher jobDispatcher)
    : ControllerBase
{
    [HttpGet]
    public IActionResult GetSample()
    {
        return this.Ok("This is a sample response from GeneralSampleController.");
    }

    [HttpGet]
    public IActionResult GetCultureSample()
    {
        return this.Ok(StringResource.GetStringByKey("Sample_Message_Text"));
    }

    [HttpGet]
    public IActionResult GetException()
    {
        throw new AppException(ExceptionCode.Generic, "This is a sample exception for testing purposes.");
    }

    [HttpPost]
    public IActionResult PostSample([FromBody] CreateOrUpdateRequest<SampleDto> request)
    {
        return this.Ok();
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetProtectedData()
    {
        return this.Ok($"This is a protected data!");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCurrentUserEmailAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);
        var systemAdminUser = await userContext.GetSystemAdminUserAsync(cancellationToken);

        var message = $"Current user email: {currentUser.Email}. " +
                      $"System admin user email: {systemAdminUser.Email}";

        return this.Ok(message);
    }

    [HttpGet]
    public string GetLogSample()
    {
        var logMessage = "This is a sample log message.";
        logger.LogInformation(logMessage);

        return $"Log message: {logMessage}";
    }

    [HttpPost]
    public async Task<IActionResult> SendEmailAsync(CancellationToken cancellationToken = default)
    {
        var emailModel = new SampleEmailModel
        {
            Name = "John Doe",
            Date = DateTime.UtcNow,
        };

        var body = await emailTemplateRenderer.RenderAsync("SampleEmailTemplate", emailModel);
        var emailAttachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    FileName = "sample.txt",
                    Type = "text/plain",
                    Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("This is a sample attachment."))
                }
            };

        var result = await emailSender.SendEmailAsync(
            to: new List<string> { "juarez.a.s.junior@gmail.com" },
            cc: null,
            bcc: null,
            subject: "Welcome to our AppProject!",
            body: body,
            emailAttachments: emailAttachments,
            cancellationToken: CancellationToken.None);

        if (result)
        {
            return this.Ok("Email sent successfully.");
        }
        else
        {
            return this.StatusCode(500, "Failed to send email.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendAIConversationAsync([FromBody] string userMessage, CancellationToken cancellationToken = default)
    {
        var systemMessage = "You are a helpful assistant. Reply in the same language as the user.";
        var userMessages = new List<string> { userMessage };

        var response = await chatClient.SendMessageAsync(systemMessage, userMessages, "openai/gpt-4.1", cancellationToken);

        return this.Ok(response);
    }

    [HttpPost]
    public IActionResult ExecuteSampleJob()
    {
        jobDispatcher.Enqueue<SampleJob>();
        return this.Ok("Sample job was enqueued successfully.");
    }
}
#endif
