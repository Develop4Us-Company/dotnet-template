using System;
using System.ComponentModel;
using AppProject.Web.Models;

namespace AppProject.Web.Framework.Pages;

public abstract class ModelFormPage<TModel> : AppProjectPageBase, IDisposable
    where TModel : ObservableModel, new()
{
    protected bool IsDisposed { get; set; }

    protected virtual TModel Model { get; private set; } = default!;

    public virtual void Dispose()
    {
        if (this.IsDisposed)
        {
            return;
        }

        this.IsDisposed = true;

        this.UnsubscribeFromEntityChanges();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (this.Model is null)
        {
            this.SetModel(new TModel());
        }
    }

    protected void SetModel(TModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (this.Model == model)
        {
            return;
        }

        this.UnsubscribeFromEntityChanges();

        this.Model = model;

        this.Model.PropertyChanged += this.OnEntityPropertyChangedInternal;
    }

    protected virtual void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    private void OnEntityPropertyChangedInternal(object? sender, PropertyChangedEventArgs e)
    {
        _ = this.InvokeAsync(() => this.OnEntityPropertyChanged(sender, e));
    }

    private void UnsubscribeFromEntityChanges()
    {
        if (this.Model is null)
        {
            return;
        }

        this.Model.PropertyChanged -= this.OnEntityPropertyChangedInternal;
    }
}
