using System;
using AppProject.Web.Framework.Components;

namespace AppProject.Web.Framework.Pages;

public abstract class AppProjectPageBase : AppProjectComponentBase
{
    public bool IsBusy { get; protected set; }
}
