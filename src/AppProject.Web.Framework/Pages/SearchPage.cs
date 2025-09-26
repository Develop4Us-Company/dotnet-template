using System;
using System.Collections.ObjectModel;
using AppProject.Models;

namespace AppProject.Web.Framework.Pages;

public abstract class SearchPage<TRequest, TSummary> : AppProjectPageBase
    where TRequest : IRequest, new()
    where TSummary : ISummary
{
    protected virtual TRequest Request { get; set; } = new TRequest();

    protected IList<TSummary> Items { get; set; } = new List<TSummary>();

    protected IList<TSummary> SelectedItems { get; set; } = new List<TSummary>();

    protected bool IsSingleItemSelected => this.SelectedItems.Count() == 1;

    protected bool HasItemsSelected => this.SelectedItems.Any();

    public async Task ExecuteSearchAsync()
    {
        await this.SetBusyAsync(true);

        if (!await this.ValidateRequestAsync())
        {
            await this.SetBusyAsync(false);
            return;
        }

        this.SelectedItems = new List<TSummary>();
        this.Items = new List<TSummary>();

        this.Items = (await this.FetchDataAsync()).ToList();

        await this.SetBusyAsync(false);
    }

    protected virtual Task<bool> ValidateRequestAsync()
    {
        return Task.FromResult(true);
    }

    protected abstract Task<IEnumerable<TSummary>> FetchDataAsync();
}
