---
name: shared-components
description: Reference for creating and using shared reusable components in the AppProject .NET Blazor template, including DropDownDataGrid, DataGridControl, ModelFormControl, FieldsetControl, SearchControl, and BusyIndicatorControl. Use when the user wants to create shared dropdown components, customize form/grid behavior, or understand the framework's base components.
metadata:
  author: appproject
  version: "1.0"
---

# Shared & Framework Components

## Framework Base Components

All framework components live in `src/AppProject.Web.Framework/`.

### SearchControl

**File:** `src/AppProject.Web.Framework/Components/SearchControl.razor`

Standard search form with text field, advanced filters, and configurable alert.

```razor
<SearchControl TRequest="SearchRequest" Request=@this.Request
    Title=@StringResource.GetStringByKey("Page_Title")
    @bind-Take=@this.Request.Take
    @bind-SearchText=@this.Request.SearchText
    DisplayTakeInfo=@this.DisplayTakeInfo
    OnExecuteSearch=@this.ExecuteSearchAsync>

    <AdvancedFilters>
        @* Advanced filter components go here *@
    </AdvancedFilters>

    <DataGridControl ... />

</SearchControl>
```

### DataGridControl

**File:** `src/AppProject.Web.Framework/Components/DataGridControl.razor`

Wraps `RadzenDataGrid` with multi-selection, localization, and configurable buttons.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `TItem` | Type | The summary type |
| `Items` | `IEnumerable<TItem>` | Data items |
| `SelectedItems` | `IList<TItem>` | Selected items (two-way binding) |
| `OnNewItem` | `EventCallback` | New button handler |
| `OnEditItem` | `EventCallback` | Edit button handler |
| `OnDeleteItem` | `EventCallback` | Delete button handler |
| `ShowNewAction` | `bool` | Show New button (default: true) |
| `ShowEditAction` | `bool` | Show Edit button (default: true) |
| `ShowDeleteAction` | `bool` | Show Delete button (default: true) |
| `PreferAddOverNew` | `bool` | Show "Add" instead of "New" |
| `PreferOpenOverEdit` | `bool` | Show "Open" instead of "Edit" |

**Slots:**
- `GlobalActions` — custom action buttons
- `ContextActions` — context-specific actions
- Default child content for `RadzenDataGridColumn` definitions

### ModelFormControl

**File:** `src/AppProject.Web.Framework/Components/ModelFormControl.razor`

Standard form wrapper with header, save/cancel buttons, and customization options.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `TModel` | Type | The model type |
| `Model` | `TModel` | Form model |
| `OnSave` | `EventCallback` | Save handler |
| `OnCancel` | `EventCallback` | Cancel handler |
| `PreferExecuteOverSave` | `bool` | Show "Execute" instead of "Save" |
| `PreferCloseOverCancel` | `bool` | Show "Close" instead of "Cancel" |

**Slots:**
- `GlobalActions` — custom action buttons
- Default child content for form fields

### FieldsetControl

**File:** `src/AppProject.Web.Framework/Components/FieldsetControl.razor`

Collapsible fieldset with centralized translations.

```razor
<FieldsetControl Title=@StringResource.GetStringByKey("Fieldset_Title")>
    @* Form fields go here *@
</FieldsetControl>
```

### BusyIndicatorControl

**File:** `src/AppProject.Web.Framework/Components/BusyIndicatorControl.razor`

Used by `AppProjectComponentBase` to display progress dialogs and handle exceptions (including `ApiException` from Refit).

## Framework Base Pages

### SearchPage<TRequest, TSummary>

**File:** `src/AppProject.Web.Framework/Pages/SearchPage.cs`

Base class for listing/search pages. Provides:
- `Request` property (search parameters)
- `Items` collection (results)
- `SelectedItems` collection
- `DisplayTakeInfo` flag (alert when limit reached)
- `ExecuteSearchAsync()` method
- `FetchDataAsync()` abstract method (override to call API)
- `OpenDialogAsync<TPage, TModel>()` for opening form dialogs
- `ConfirmAsync()` for delete confirmation
- `HandleExceptionAsync()` for error handling

### ModelFormPage<TModel>

**File:** `src/AppProject.Web.Framework/Pages/ModelFormPage.cs`

Base class for form pages. Provides:
- `Model` property
- `SetModel()` method (for loading existing data)
- `CloseDialogAsync()` for closing the dialog (with or without result)
- `GetResultOrHandleExceptionAsync()` for API calls with error handling

## Creating a DropDown DataGrid Component

**Location:** `src/AppProject.Web.Shared/<Module>/Components/`

DropDown components inherit from `DropDownDataGridControl` and wrap `RadzenDropDownDataGrid`.

### Reference: CountrySummaryDropDownDataGridControl

```razor
@inherits DropDownDataGridControl<CountrySummary, Guid>

@* The DropDownDataGrid implementation *@

@code {
    [Inject]
    private ICountrySummaryClient Client { get; set; } = default!;

    // Load data on filter/search
    // Ensure selected item loads even after pagination
}
```

**Key points:**
- Generic type: `DropDownDataGridControl<TSummary, TValue>`
- `TValue` is `Guid` for required FK fields, `Guid?` for optional
- Must handle loading the selected item on edit (even if not in current page)
- Used in both search pages (advanced filters) and form pages (FK selection)

### Using DropDown in Forms

```razor
<ParentSummaryDropDownDataGridControl @bind-Value=@this.Model.ParentEntityId />
<RadzenCustomValidator Component="ParentEntityField"
    Validator=@(() => this.Model.ParentEntityId != Guid.Empty)
    Text=@StringResource.GetStringByKey("...Required") />
```

### Using DropDown in Search Filters

```razor
<AdvancedFilters>
    <ParentSummaryDropDownDataGridControl @bind-Value=@this.Request.ParentEntityId
        Style="width: 300px;" />
</AdvancedFilters>
```

## Nested Items Pattern (City → Neighborhoods)

For entities with child collections managed in the same form:

1. **Main form** has a `DataGridControl` for child items with `PreferAddOverNew`
2. **Child form** is opened as a dialog that returns the child model
3. Changes tracked in `ChangedChildRequests` and `DeletedChildRequests`
4. Load existing children via dedicated client method (e.g., `ICityClient.GetNeighborhoodsAsync`)

See `CityFormPage.razor` and `NeighborhoodFormPage.razor` for the complete pattern.

## Radzen Component Tips

- Use `RadzenRequiredValidator` for required text fields
- Use `RadzenLengthValidator` for max length validation
- Use `RadzenCustomValidator` for custom logic (e.g., `Guid.Empty` check)
- For decimal values in `RadzenNumeric`, use `@(0.01m)` for `Min`/`Max`
- Avoid redundant validators when model has `[Required]`
- Check Radzen documentation for specific component attributes
