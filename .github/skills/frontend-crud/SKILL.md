---
name: frontend-crud
description: Step-by-step instructions for creating frontend CRUD pages in the AppProject .NET Blazor WebAssembly template, including observable models, Refit API clients, search/listing pages, form pages, dropdown components, and shared components. Use when the user wants to create frontend pages, Blazor components, Refit clients, or web models for a new entity.
metadata:
  author: appproject
  version: "1.0"
---

# Frontend CRUD Creation

Follow these steps to create a complete frontend CRUD for a new entity using Blazor WebAssembly. Use the **General** module (Country, State, City) as the reference.

## Step 1: Create the Web Observable Model

**Location:** `src/AppProject.Web.Models.<Module>/`

Web models mirror the API DTOs but implement `INotifyPropertyChanged` via `ObservableModel`:

```csharp
using System;
using AppProject.Models;

namespace AppProject.Web.Models.<Module>;

public class <EntityName> : ObservableModel, IEntity
{
    private Guid? id;
    private string name = default!;
    private byte[]? rowVersion;

    public Guid? Id { get => this.id; set => this.Set(ref this.id, value); }

    public string Name { get => this.name; set => this.Set(ref this.name, value); }

    // Add other properties with backing fields and Set()...

    public byte[]? RowVersion { get => this.rowVersion; set => this.Set(ref this.rowVersion, value); }
}
```

**Rules:**
- Use backing fields for ALL properties
- Use `this.Set(ref field, value)` in setters
- Inherit from `ObservableModel` and `IEntity`
- For FK fields: use `Guid` (required) or `Guid?` (optional)

### For entities with nested children (City pattern):

```csharp
public ICollection<CreateOrUpdateRequest<ChildEntity>>? ChangedChildRequests { get; set; }
public ICollection<DeleteRequest<Guid>>? DeletedChildRequests { get; set; }
```

## Step 2: Create the Web Summary Model

**Location:** `src/AppProject.Web.Models/<Module>/` (shared) or `src/AppProject.Web.Models.<Module>/`

```csharp
using System;
using AppProject.Models;

namespace AppProject.Web.Models.<Module>;

public class <EntityName>Summary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    // Add aggregated display fields
}
```

## Step 3: Create the Refit CRUD Client

**Location:** `src/AppProject.Web.ApiClient.<Module>/`

Routes MUST match the controller routes exactly:

```csharp
using System;
using AppProject.Models;
using AppProject.Web.Models.<Module>;
using Refit;

namespace AppProject.Web.ApiClient.<Module>;

public interface I<EntityName>Client
{
    [Get("/api/<module_lowercase>/<EntityName>/Get")]
    public Task<EntityResponse<<EntityName>>> GetAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);

    [Post("/api/<module_lowercase>/<EntityName>/Post")]
    public Task<KeyResponse<Guid>> PostAsync([Body] CreateOrUpdateRequest<<EntityName>> request, CancellationToken cancellationToken = default);

    [Put("/api/<module_lowercase>/<EntityName>/Put")]
    public Task<KeyResponse<Guid>> PutAsync([Body] CreateOrUpdateRequest<<EntityName>> request, CancellationToken cancellationToken = default);

    [Delete("/api/<module_lowercase>/<EntityName>/Delete")]
    public Task<EmptyResponse> DeleteAsync([Query] DeleteRequest<Guid> request, CancellationToken cancellationToken = default);
}
```

## Step 4: Create the Refit Summary Client

**Location:** `src/AppProject.Web.ApiClient/<Module>/` (shared)

```csharp
using System;
using AppProject.Models;
using AppProject.Web.Models.<Module>;
using Refit;

namespace AppProject.Web.ApiClient.<Module>;

public interface I<EntityName>SummaryClient
{
    [Get("/api/<module_lowercase>/<EntityName>Summary/GetSummaries")]
    public Task<SummariesResponse<<EntityName>Summary>> GetSummariesAsync([Query] SearchRequest request, CancellationToken cancellationToken = default);

    [Get("/api/<module_lowercase>/<EntityName>Summary/GetSummary")]
    public Task<SummaryResponse<<EntityName>Summary>> GetSummaryAsync([Query] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
```

If using a custom SearchRequest, replace `SearchRequest` with `<EntityName>SummarySearchRequest`.

## Step 5: Create the Search/Listing Page

**Location:** `src/AppProject.Web.<Module>/Pages/`

Search pages inherit from `SearchPage<TRequest, TSummary>`:

```razor
@page "/<module_lowercase>/<plural_entity_lowercase>"

@attribute [Authorize]

@inherits SearchPage<SearchRequest, <EntityName>Summary>

<SearchControl TRequest="SearchRequest" Request=@this.Request
    Title=@StringResource.GetStringByKey("<Module>_<EntityName>SummaryPage_Title") @bind-Take=@this.Request.Take
    @bind-SearchText=@this.Request.SearchText DisplayTakeInfo=@this.DisplayTakeInfo
    OnExecuteSearch=@this.ExecuteSearchAsync>

    @* Optional: Advanced filters go here *@

    <DataGridControl TItem="<EntityName>Summary" Items=@this.Items @bind-SelectedItems=@this.SelectedItems
        OnNewItem=@this.OnNewItemAsync OnEditItem=@this.OnEditItemAsync OnDeleteItem=@OnDeleteItemAsync>

        <RadzenDataGridColumn TItem="<EntityName>Summary"
            Title=@StringResource.GetStringByKey("<Module>_<EntityName>SummaryPage_NameColumn_Title")
            Property=@nameof(<EntityName>Summary.Name) />

        @* Add more columns as needed *@

    </DataGridControl>
</SearchControl>

@code {
    [Inject]
    private I<EntityName>SummaryClient <EntityName>SummaryClient { get; set; } = default!;

    [Inject]
    private I<EntityName>Client <EntityName>Client { get; set; } = default!;

    protected override async Task<IEnumerable<<EntityName>Summary>> FetchDataAsync()
    {
        var summariesResponse = await this.GetResultOrHandleExceptionAsync<SummariesResponse<<EntityName>Summary>>(
            () => this.<EntityName>SummaryClient.GetSummariesAsync(this.Request));

        return summariesResponse?.Summaries ?? Enumerable.Empty<<EntityName>Summary>();
    }

    private async Task OnNewItemAsync()
    {
        await this.OpenDialogAsync<<EntityName>FormPage, <EntityName>>(
            title: StringResource.GetStringByKey("<Module>_<EntityName>FormPage_Title"));
        await this.ExecuteSearchAsync();
    }

    private async Task OnEditItemAsync()
    {
        var selectedId = this.SelectedItems.FirstOrDefault()?.Id;

        if (selectedId.HasValue)
        {
            await this.OpenDialogAsync<<EntityName>FormPage, <EntityName>>(
                title: StringResource.GetStringByKey("<Module>_<EntityName>FormPage_Title"),
                parameters: new Dictionary<string, object>() { { "Id", selectedId } });
            await this.ExecuteSearchAsync();
        }
    }

    private async Task OnDeleteItemAsync()
    {
        var selectedIds = this.SelectedItems.Select(x => x.Id);

        if (selectedIds.Any() && await this.ConfirmAsync(StringResource.GetStringByKey("Dialog_Confirm_Delete_Message")))
        {
            foreach (var selectedId in selectedIds)
            {
                await this.HandleExceptionAsync(() => this.<EntityName>Client.DeleteAsync(new DeleteRequest<Guid> { Id = selectedId }));
            }

            await this.ExecuteSearchAsync();
        }
    }
}
```

### With Advanced Filters (State search pattern):

Add filters inside `SearchControl` using `AdvancedFilters`:
```razor
<AdvancedFilters>
    <ParentSummaryDropDownDataGridControl @bind-Value=@this.Request.ParentEntityId
        Style="width: 300px;" />
</AdvancedFilters>
```

## Step 6: Create the Form Page

**Location:** `src/AppProject.Web.<Module>/Pages/`

Form pages inherit from `ModelFormPage<TModel>`:

```razor
@attribute [Authorize]

@inherits ModelFormPage<<EntityName>>

<ModelFormControl TModel="<EntityName>" Model=@this.Model OnSave=@this.OnSaveAsync OnCancel=@this.OnCancelAsync>
    <FieldsetControl Title=@StringResource.GetStringByKey("<Module>_<EntityName>FormPage_GeneralFieldset_Title")>
        <RadzenRow>
            <RadzenColumn>
                <RadzenText TextStyle="TextStyle.Subtitle2">@StringResource.GetStringByKey("<Module>_<EntityName>FormPage_GeneralFieldset_IdField_Text", this.Model.Id)</RadzenText>
            </RadzenColumn>
        </RadzenRow>
        <RadzenRow>
            <RadzenColumn>
                <RadzenStack Orientation="Orientation.Horizontal" Gap="1rem" Wrap="FlexWrap.Wrap">

                    <RadzenFormField Text=@StringResource.GetStringByKey("<Module>_<EntityName>FormPage_GeneralFieldset_NameField_Label")>
                        <RadzenTextBox Name="NameField" @bind-Value=@this.Model.Name />
                        <RadzenRequiredValidator Component="NameField" Text=@StringResource.GetStringByKey("<Module>_<EntityName>FormPage_GeneralFieldset_NameField_Required") />
                        <RadzenLengthValidator Component="NameField" Max="200" Text=@StringResource.GetStringByKey("<Module>_<EntityName>FormPage_GeneralFieldset_NameField_InvalidLength") />
                    </RadzenFormField>

                    @* Add more fields following the same pattern *@

                </RadzenStack>
            </RadzenColumn>
        </RadzenRow>
    </FieldsetControl>
</ModelFormControl>

@code {
    [Parameter]
    public Guid? Id { get; set; }

    [Inject]
    private I<EntityName>Client <EntityName>Client { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (this.Id.HasValue)
        {
            var entityResponse = await this.GetResultOrHandleExceptionAsync<EntityResponse<<EntityName>>>(
                () => this.<EntityName>Client.GetAsync(new GetByIdRequest<Guid>{ Id = this.Id.Value }));

            if (entityResponse is not null)
            {
                this.SetModel(entityResponse.Entity);
            }
        }
    }

    private async Task OnSaveAsync()
    {
        var keyResponse = await this.GetResultOrHandleExceptionAsync<KeyResponse<Guid>>(
            async () =>
            {
                if (this.Model.Id.GetValueOrDefault() != Guid.Empty)
                {
                    return await this.<EntityName>Client.PutAsync(new CreateOrUpdateRequest<<EntityName>> { Entity = this.Model });
                }

                return await this.<EntityName>Client.PostAsync(new CreateOrUpdateRequest<<EntityName>> { Entity = this.Model });
            });

        if (keyResponse is not null)
        {
            this.Model.Id = keyResponse.Id;
            await this.CloseDialogAsync(this.Model);
        }
    }

    private Task OnCancelAsync() => this.CloseDialogAsync();
}
```

### FK Selection with DropDownDataGrid:

For forms that require selecting a parent entity, use a DropDown component:
```razor
<ParentSummaryDropDownDataGridControl @bind-Value=@this.Model.ParentEntityId />
<RadzenCustomValidator Component="ParentEntityField"
    Validator=@(() => this.Model.ParentEntityId != Guid.Empty)
    Text=@StringResource.GetStringByKey("...Required") />
```

## Step 7: Create Shared DropDown Component (if needed)

**Location:** `src/AppProject.Web.Shared/<Module>/Components/`

```razor
@inherits DropDownDataGridControl<<EntityName>Summary, Guid>

<RadzenDropDownDataGrid ... />

@code {
    [Inject]
    private I<EntityName>SummaryClient Client { get; set; } = default!;

    // See CountrySummaryDropDownDataGridControl.razor for full pattern
}
```

## Step 8: Add Resource Keys

Add all UI text keys to the three `.resx` files:
- Page titles: `<Module>_<EntityName>SummaryPage_Title`, `<Module>_<EntityName>FormPage_Title`
- Column titles: `<Module>_<EntityName>SummaryPage_<Column>Column_Title`
- Field labels: `<Module>_<EntityName>FormPage_GeneralFieldset_<Field>Field_Label`
- Validators: `..._Required`, `..._InvalidLength`
- Fieldset titles: `<Module>_<EntityName>FormPage_GeneralFieldset_Title`
- ID field: `<Module>_<EntityName>FormPage_GeneralFieldset_IdField_Text`

## Step 9: Add Menu Item

**Location:** `src/AppProject.Web/Layout/NavMenu.razor`

Add the navigation entry with the appropriate permission check.

## Step 10: Register Lazy Loading (if new module)

**Location:** `src/AppProject.Web/App.razor`

Add the assembly to `OnNavigateAsync()` conditions for the module's route prefix.

## DataGridControl Options

- `ShowNewAction` / `ShowEditAction` / `ShowDeleteAction` — toggle button visibility
- `PreferAddOverNew` — changes "New" button text to "Add" (for child items)
- `PreferOpenOverEdit` — changes "Edit" to "Open" (read-only)

## ModelFormControl Options

- `PreferExecuteOverSave` — changes "Save" to "Execute"
- `PreferCloseOverCancel` — applies closing style to cancel button
- `GlobalActions` — slot for custom action buttons

## Checklist

- [ ] Web observable model created with `ObservableModel`, `IEntity`, backing fields
- [ ] Web summary model created with `ISummary`
- [ ] Refit CRUD client with routes matching controller exactly
- [ ] Refit summary client created
- [ ] Search/listing page with `SearchPage` base, `SearchControl`, `DataGridControl`
- [ ] Form page with `ModelFormPage` base, `ModelFormControl`, `FieldsetControl`
- [ ] DropDown component created (if FK selections needed)
- [ ] Resource keys added to all three `.resx` files
- [ ] Menu item added to `NavMenu.razor`
- [ ] Lazy loading configured in `App.razor` (if new module)
- [ ] Assembly registered in `WebBootstrap.GetApiClientAssemblies()`
