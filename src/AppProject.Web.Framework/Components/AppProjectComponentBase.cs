using System;
using System.Text.Json;
using AppProject.Exceptions;
using AppProject.Resources;
using AppProject.Web.Framework.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;
using Refit;

namespace AppProject.Web.Framework.Components;

public abstract class AppProjectComponentBase : ComponentBase
{
    private const string CenteredPositionStyle = "top:50% !important; left:50% !important; transform:translate(-50%,-50%) !important; position:fixed !important; min-height:auto; min-width:auto; width:auto";

    protected bool IsBusy { get; private set; }

    [Inject]
    protected NotificationService NotificationService { get; set; } = default!;

    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected Task NotifySuccessAsync(string message, string? title = null, double duration = 10000, Action<object>? notificationClick = null, object? payload = null) =>
        this.NotifyAsync(NotificationSeverity.Success, message, title, duration, notificationClick, payload);

    protected Task NotifyWarningAsync(string message, string? title = null, double duration = 10000, Action<object>? notificationClick = null, object? payload = null) =>
        this.NotifyAsync(NotificationSeverity.Warning, message, title, duration, notificationClick, payload);

    protected Task NotifyErrorAsync(string message, string? title = null, double duration = 10000, Action<object>? notificationClick = null, object? payload = null) =>
        this.NotifyAsync(NotificationSeverity.Error, message, title, duration, notificationClick, payload);

    protected Task NotifyInfoAsync(string message, string? title = null, double duration = 10000, Action<object>? notificationClick = null, object? payload = null) =>
        this.NotifyAsync(NotificationSeverity.Info, message, title, duration, notificationClick, payload);

    protected async Task ShowErrorMessageAsync(string message, string? title = null)
    {
        await this.DialogService.Alert(
            message,
            title ?? StringResource.GetStringByKey("Dialog_Error_Title"),
            new AlertOptions()
            {
                OkButtonText = StringResource.GetStringByKey("Dialog_Error_OkButton_Text")
            });
    }

    protected async Task ShowInfoMessageAsync(string message, string? title = null)
    {
        await this.DialogService.Alert(
            message,
            title ?? StringResource.GetStringByKey("Dialog_Info_Title"),
            new AlertOptions()
            {
                OkButtonText = StringResource.GetStringByKey("Dialog_Info_OkButton_Text")
            });
    }

    protected async Task<bool> ConfirmAsync(string message, string? title = null)
    {
        return (await this.DialogService.Confirm(
            message,
            title ?? StringResource.GetStringByKey("Dialog_Confirm_Title"),
            new ConfirmOptions()
            {
                OkButtonText = StringResource.GetStringByKey("Dialog_Confirm_YesButton_Text"),
                CancelButtonText = StringResource.GetStringByKey("Dialog_Confirm_NoButton_Text")
            })) ?? false;
    }

    protected async Task ShowBusyIndicatorAsync(string? message = null)
    {
        if (this.IsBusy)
        {
            return;
        }

        this.IsBusy = true;

        message ??= StringResource.GetStringByKey("Dialog_Busy_Message");

        RenderFragment<DialogService> content = _ => builder =>
        {
            builder.OpenComponent<BusyIndicatorControl>(0);

            builder.AddAttribute(1, "Message", message);

            builder.CloseComponent();
        };

        await this.DialogService.OpenAsync(
            string.Empty,
            content,
            new DialogOptions
            {
                ShowTitle = false,
                Style = CenteredPositionStyle,
                CloseDialogOnEsc = false
            });
    }

    protected async Task CloseBusyIndicatorAsync()
    {
        if (!this.IsBusy)
        {
            return;
        }

        this.IsBusy = false;

        await this.CloseDialogAsync();

        this.StateHasChanged();
    }

    protected Task NavigateToPageAsync<TPage>(Dictionary<string, object>? routeParameters = null, Dictionary<string, object>? queryParameters = null, bool forceLoad = false)
        where TPage : AppProjectPageBase
    {
        this.NavigationManager.NavigateTo($"{typeof(TPage).Name}{this.GetParameters(routeParameters, queryParameters)}", forceLoad: forceLoad);

        return Task.CompletedTask;
    }

    protected async Task OpenDialogAsync<TPage>(string? title = null, Dictionary<string, object>? parameters = null, bool showClose = false, bool isDraggable = true, bool isResizable = true)
        where TPage : AppProjectPageBase
    {
        await this.OpenDialogWithDynamicAsync<TPage>(title, parameters, showClose, isDraggable, isResizable);
    }

    protected async Task<TResult> OpenDialogAsync<TPage, TResult>(string? title = null, Dictionary<string, object>? parameters = null, bool showClose = false, bool isDraggable = true, bool isResizable = true)
        where TPage : AppProjectPageBase
    {
        return await this.OpenDialogWithDynamicAsync<TPage>(title, parameters, showClose, isDraggable, isResizable);
    }

    protected Task CloseDialogAsync()
    {
        this.DialogService.Close();

        return Task.CompletedTask;
    }

    protected Task CloseDialogAsync<TResult>(TResult result)
    {
        this.DialogService.Close(result);

        return Task.CompletedTask;
    }

    protected async Task<TResult?> GetResultOrHandleExceptionAsync<TResult>(Func<Task<TResult>> operation, Func<Exception, Task<bool>>? exceptionHandler = null, bool showMessage = true, bool showBusyIndicator = true)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        if (showBusyIndicator)
        {
            _ = this.ShowBusyIndicatorAsync();
        }

        try
        {
            var result = await operation();

            if (showBusyIndicator)
            {
                await this.CloseBusyIndicatorAsync();
            }

            return result;
        }
        catch (Exception exception)
        {
            if (showBusyIndicator)
            {
                await this.CloseBusyIndicatorAsync();
            }

            await this.HandleExceptionAsync(exceptionHandler, showMessage, exception);

            return default;
        }
    }

    protected async Task HandleExceptionAsync(Func<Task> operation, Func<Exception, Task<bool>>? exceptionHandler = null, bool showMessage = true, bool showBusyIndicator = true)
    {
        if (operation is null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        if (showBusyIndicator)
        {
            _ = this.ShowBusyIndicatorAsync();
        }

        try
        {
            await operation();

            if (showBusyIndicator)
            {
                await this.CloseBusyIndicatorAsync();
            }
        }
        catch (Exception exception)
        {
            if (showBusyIndicator)
            {
                await this.CloseBusyIndicatorAsync();
            }

            await this.HandleExceptionAsync(exceptionHandler, showMessage, exception);
        }
    }

    private async Task HandleExceptionAsync(Func<Exception, Task<bool>>? exceptionHandler, bool showMessage, Exception exception)
    {
        var appException = exception as AppException;

        if (exception is ApiException apiException && !string.IsNullOrEmpty(apiException.Content))
        {
            appException = JsonSerializer.Deserialize<AppException>(apiException.Content);
        }

        var wasHandled = exceptionHandler is not null
            ? await exceptionHandler.Invoke(appException ?? exception)
            : false;

        if (!wasHandled && showMessage)
        {
            if (appException is not null)
            {
                var exceptionMessage = StringResource.GetStringByKey(appException.ExceptionCode.ToString());

                if (string.IsNullOrEmpty(exceptionMessage))
                {
                    exceptionMessage = StringResource.GetStringByKey("Exception_UnhandledException_Message");
                }

                await ShowExceptionMessageAsync(exceptionMessage, appException.AdditionalInfo);

                return;
            }

            await ShowExceptionMessageAsync(StringResource.GetStringByKey("Exception_UnhandledException_Message"), exception.Message);
        }

        async Task ShowExceptionMessageAsync(string message, string? additionalInfo)
        {
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += $" {StringResource.GetStringByKey("Exception_AdditionalInfo_Message", additionalInfo)}";
            }

            await this.ShowErrorMessageAsync(message);
        }
    }

    private Task NotifyAsync(NotificationSeverity severity, string message, string? title = null, double duration = 10000, Action<object>? notificationClick = null, object? payload = null)
    {
        this.NotificationService.Notify(new NotificationMessage()
        {
            Severity = severity,
            Summary = title,
            Duration = duration,
            Detail = message,
            Click = notificationClick,
            Payload = payload
        });

        return Task.CompletedTask;
    }

    private async Task<dynamic> OpenDialogWithDynamicAsync<TPage>(string? title, Dictionary<string, object>? parameters, bool showClose, bool isDraggable, bool isResizable)
        where TPage : AppProjectPageBase
    {
        var options = new DialogOptions()
        {
            ShowClose = showClose,
            Draggable = isDraggable,
            Resizable = isResizable,
            Style = CenteredPositionStyle
        };

        return await this.DialogService.OpenAsync<TPage>(title, parameters, options);
    }

    private string GetParameters(Dictionary<string, object>? routeParameters = null, Dictionary<string, object>? queryParameters = null)
    {
        var result = string.Empty;

        if (routeParameters != null && routeParameters.Any())
        {
            result = $"/{string.Join("/", routeParameters.Select(x => this.GetRouteParameterValue(x.ToString())))}";
        }

        if (queryParameters != null && queryParameters.Any())
        {
            result += $"?{string.Join("&", queryParameters.Select(x => this.GetQueryParameterValue(x.Key, x.ToString())))}";
        }

        return result;
    }

    private string GetRouteParameterValue(string value) => Uri.EscapeDataString(value);

    private string GetQueryParameterValue(string name, string value) => $"{name}={Uri.EscapeDataString(value)}";
}
