using System;
using System.Security.Claims;
using Serilog.Context;

namespace AppProject.Core.API.Middlewares;

public class SerilogUserEnricherMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var userEmail = context.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.FindFirstValue("email") ?? "Anonymous"
            : "Anonymous";

        using (LogContext.PushProperty("UserEmail", userEmail))
        {
            await next(context);
        }
    }
}
