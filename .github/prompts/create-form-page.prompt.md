---
mode: agent
description: "Create a form/registration page for an existing entity"
---

# Create Form Page

Create a form (registration/edit) page for an entity that already has a backend and Refit client implementation.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Country`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Form fields** (labels, types, validations, max lengths)
4. **Whether the entity has FK fields** requiring dropdown selectors
5. **Whether the entity has nested/child items** (like City → Neighborhoods)

## Instructions

Follow the form page section of the `frontend-crud` skill:

1. Create the form page at `src/AppProject.Web.<Module>/Pages/<EntityName>FormPage.razor`
2. The page must:
   - Use `@attribute [Authorize]`
   - Inherit from `ModelFormPage<<EntityName>>`
   - Use `ModelFormControl` with `OnSave` and `OnCancel` handlers
   - Use `FieldsetControl` for grouping fields
   - Display the ID as read-only text
   - Use `RadzenFormField` with appropriate Radzen input components
   - Use validators: `RadzenRequiredValidator`, `RadzenLengthValidator`, `RadzenCustomValidator`
   - Accept `[Parameter] public Guid? Id` for edit mode
   - Load existing data in `OnInitializedAsync` if `Id` has value
   - Handle Post (new) vs Put (edit) in `OnSaveAsync`
   - Close dialog with `CloseDialogAsync` after successful save

### For FK Selection Fields

Use dropdown components inheriting from `DropDownDataGridControl`:
```razor
<ParentSummaryDropDownDataGridControl @bind-Value=@this.Model.ParentEntityId />
<RadzenCustomValidator Component="ParentField"
    Validator=@(() => this.Model.ParentEntityId != Guid.Empty)
    Text=@StringResource.GetStringByKey("...Required") />
```

### For Nested Child Items

Follow the `CityFormPage.razor` pattern:
- Add a `DataGridControl` with `PreferAddOverNew` for child items
- Open child form dialogs that return the child model
- Track changes in `ChangedChildRequests` and `DeletedChildRequests`

3. Add resource keys for:
   - Form title: `<Module>_<EntityName>FormPage_Title`
   - Fieldset titles: `<Module>_<EntityName>FormPage_<Fieldset>Fieldset_Title`
   - Field labels: `..._<Field>Field_Label`
   - Validators: `..._<Field>Field_Required`, `..._<Field>Field_InvalidLength`
   - ID field: `..._IdField_Text`

## Reference Implementations

- **Simple form:** `src/AppProject.Web.General/Pages/CountryFormPage.razor`
- **Form with FK:** `src/AppProject.Web.General/Pages/StateFormPage.razor`
- **Form with nested items:** `src/AppProject.Web.General/Pages/CityFormPage.razor`
- **Child form dialog:** `src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor`
