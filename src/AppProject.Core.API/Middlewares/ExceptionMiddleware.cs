using System;
using System.Net;
using System.Text.Json;
using AppProject.Exceptions;

namespace AppProject.Core.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionMiddleware> logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (AppException exception)
        {
            this.logger.LogError(exception.ToString());

            switch (exception.ExceptionCode)
            {
                case ExceptionCode.SecurityValidation:
                    await this.HandleExceptionAsync(context, exception.ExceptionCode, httpStatusCode: HttpStatusCode.Unauthorized);
                    return;
                case ExceptionCode.RequestValidation:
                    await this.HandleExceptionAsync(context, exception.ExceptionCode, exception.AdditionalInfo, httpStatusCode: HttpStatusCode.BadRequest);
                    return;
            }

            await this.HandleExceptionAsync(context, exception.ExceptionCode, exception.AdditionalInfo);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception.ToString());
            await this.HandleExceptionAsync(context);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, ExceptionCode exceptionCode = ExceptionCode.Generic, string? additionalInfo = null, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)httpStatusCode;
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ExceptionDetail()
            {
                ExceptionCode = exceptionCode,
                AdditionalInfo = additionalInfo,
            }));
    }
}
