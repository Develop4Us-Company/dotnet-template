using System;
using AppProject.Resources;
using Radzen.Blazor;

namespace AppProject.Web.Framework.Components;

public class DropDownDataGridControl<TValue> : RadzenDropDownDataGrid<TValue>
{
    public DropDownDataGridControl()
    {
        this.EmptyText = StringResource.GetStringByKey("RadzenDropDownDataGrid_EmptyText");
        this.SearchTextPlaceholder = StringResource.GetStringByKey("RadzenDropDownDataGrid_SearchTextPlaceholder");
        this.AllowFiltering = true;
        this.AllowClear = true;
        this.AllowSorting = false;
    }
}
