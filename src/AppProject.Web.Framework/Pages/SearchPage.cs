using System;
using System.Collections.ObjectModel;
using AppProject.Models;

namespace AppProject.Web.Framework.Pages;

public abstract class SearchPage<TRequest, TSummary> : AppProjectPageBase
    where TRequest : IRequest
    where TSummary : ISummary
{
    public ObservableCollection<TSummary> Items { get; private set; } = new ObservableCollection<TSummary>();

    public ObservableCollection<TSummary> SelectedItems { get; set; } = new ObservableCollection<TSummary>();

    public async Task ExecuteSearchAsync(bool validateRequest = true)
    {
        this.IsBusy = true;

        if (validateRequest)
        {
            await this.ValidateRequestAsync();
        }

        this.SelectedItems.Clear();
        this.Items.Clear();

        var items = await this.FetchDataAsync();

        foreach (var item in items)
        {
            this.Items.Add(item);
        }

        this.IsBusy = false;

        this.StateHasChanged();
    }

    public virtual void OnSelectedItem(TSummary summary)
    {
        this.SelectedItems.Add(summary);
    }

    public virtual void OnDeselectedItem(TSummary summary)
    {
        this.SelectedItems.Remove(summary);
    }

    protected virtual async Task ValidateRequestAsync()
    {
        await Task.CompletedTask;
    }

    protected abstract Task<IEnumerable<TSummary>> FetchDataAsync();
}
