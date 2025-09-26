using System;
using AppProject.Web.Framework.Components;

namespace AppProject.Web.Framework.Pages;

public abstract class AppProjectPageBase : AppProjectComponentBase
{
    public bool IsBusy { get; private set; }

    protected async Task SetBusyAsync(bool isBusy)
    {
        if (this.IsBusy != isBusy)
        {
            if (isBusy == true)
            {
                _ = this.ShowBusyIndicatorAsync();
            }
            else
            {
                await this.CloseDialogAsync();
            }

            this.IsBusy = isBusy;
            this.StateHasChanged();
        }
    }
}
