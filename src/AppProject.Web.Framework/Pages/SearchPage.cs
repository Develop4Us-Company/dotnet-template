using System;
using System.Collections.ObjectModel;
using AppProject.Models;

namespace AppProject.Web.Framework.Pages;

public abstract class SearchPage<TRequest, TSummary> : AppProjectPageBase
    where TRequest : IRequest
    where TSummary : ISummary
{
    public IList<TSummary> Items { get; set; } = new List<TSummary>();

    public IList<TSummary> SelectedItems { get; set; } = new List<TSummary>();

    protected bool IsContextActionsDisabled => !this.SelectedItems.Any();

    public async Task ExecuteSearchAsync()
    {
        this.IsBusy = true;

        if (!await this.ValidateRequestAsync())
        {
            return;
        }

        this.SelectedItems = new List<TSummary>();
        this.Items = new List<TSummary>();

        this.Items = (await this.FetchDataAsync()).ToList();

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

    protected virtual Task<bool> ValidateRequestAsync()
    {
        return Task.FromResult(true);
    }

    protected abstract Task<IEnumerable<TSummary>> FetchDataAsync();
}
