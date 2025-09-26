using System;
using System.ComponentModel;
using AppProject.Models;
using AppProject.Web.Models;

namespace AppProject.Web.Framework.Pages;

public abstract class FormPage<TEntity> : AppProjectPageBase, IDisposable
    where TEntity : ObservableModel, IEntity, new()
{
    protected bool IsDisposed { get; set; }

    protected virtual TEntity? Entity { get; private set; }

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

        if (this.Entity is null)
        {
            this.SetEntity(new TEntity());
        }
    }

    protected void SetEntity(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        if (this.Entity == entity)
        {
            return;
        }

        this.UnsubscribeFromEntityChanges();

        this.Entity = entity;

        this.Entity.PropertyChanged += this.OnEntityPropertyChangedInternal;
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
        if (this.Entity is null)
        {
            return;
        }

        this.Entity.PropertyChanged -= this.OnEntityPropertyChangedInternal;
    }
}
