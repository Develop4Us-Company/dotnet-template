#if DEBUG
using System.Security.Claims;
using AppProject.Core.Contracts;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppProject.Core.Controllers.General
{
    [Route("api/general/[controller]/[action]")]
    [ApiController]
    public class SampleController(
        ILogger<SampleController> logger,
        IUserContext userContext)
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
            return this.Ok(StringResource.GetStringByKey("Sample"));
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
    }
}
#endif