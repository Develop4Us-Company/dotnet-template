---
name: localization
description: Instructions for managing localization and resource files in the AppProject .NET template. Covers adding translation keys to .resx files, resource key naming conventions, placeholder formatting, and maintaining consistency across en-US, pt-BR, and es-ES translations. Use when the user needs to add or edit translations, resource keys, or localization support.
metadata:
  author: appproject
  version: "1.0"
---

# Localization & Resource Management

## Overview

The template supports three languages:
- `en-US` (English) — `Resource.resx`
- `pt-BR` (Brazilian Portuguese) — `Resource.pt-BR.resx`
- `es-ES` (Spanish) — `Resource.es-ES.resx`

**Location:** `src/AppProject.Resources/`

Both API and frontend consume resources via `StringResource.GetStringByKey()`.

## Resource Key Naming Conventions

### Page Titles
```
<Module>_<EntityName>SummaryPage_Title
<Module>_<EntityName>FormPage_Title
```

### Column Titles (grids)
```
<Module>_<EntityName>SummaryPage_<ColumnName>Column_Title
```

### Fieldset Titles
```
<Module>_<EntityName>FormPage_<FieldsetName>Fieldset_Title
```

### Field Labels
```
<Module>_<EntityName>FormPage_<FieldsetName>Fieldset_<FieldName>Field_Label
```

### ID Display Field
```
<Module>_<EntityName>FormPage_<FieldsetName>Fieldset_IdField_Text
```

### Validators
```
<Module>_<EntityName>FormPage_<FieldsetName>Fieldset_<FieldName>Field_Required
<Module>_<EntityName>FormPage_<FieldsetName>Fieldset_<FieldName>Field_InvalidLength
```

### Menu Items
```
NavMenu_<Module>_Title
NavMenu_<Module>_<EntityName>_Text
```

### Exception Messages
```
ExceptionCode_<Module>_<EntityName>_<ValidationName>
```

### Permission Names
```
Permission_<Module>_<Action>_Name
Permission_<Module>_<Action>_Description
```

## Adding New Resource Keys

When adding new keys, you MUST:

1. **Add to ALL three `.resx` files** — never leave translations missing
2. **Preserve existing comments** — each `.resx` has comment markers separating groups
3. **Follow alphabetical order** within each group
4. **Use the existing grouping/tabbing pattern** — forms separated, validations and menu together
5. **Use `{{}}` instead of `{}`** for placeholder values that should be treated as literal text by the parser

### Example Entry (Resource.resx — English)

```xml
<data name="General_CountryFormPage_Title" xml:space="preserve">
    <value>Country</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_Title" xml:space="preserve">
    <value>General</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_Label" xml:space="preserve">
    <value>Name</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_Required" xml:space="preserve">
    <value>Name is required.</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_InvalidLength" xml:space="preserve">
    <value>Name must have a maximum of 200 characters.</value>
</data>
```

### Example Entry (Resource.pt-BR.resx — Portuguese)

```xml
<data name="General_CountryFormPage_Title" xml:space="preserve">
    <value>País</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_Label" xml:space="preserve">
    <value>Nome</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_Required" xml:space="preserve">
    <value>Nome é obrigatório.</value>
</data>
```

### Example Entry (Resource.es-ES.resx — Spanish)

```xml
<data name="General_CountryFormPage_Title" xml:space="preserve">
    <value>País</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_Label" xml:space="preserve">
    <value>Nombre</value>
</data>
<data name="General_CountryFormPage_GeneralFieldset_NameField_Required" xml:space="preserve">
    <value>El nombre es obligatorio.</value>
</data>
```

## Placeholder Formatting

When you need to reserve future placeholder values in `.resx` files:

- **Use `{{Value}}`** instead of `{Value}` — prevents the parser from trying to interpolate
- Example: `The price is {{Price}}` → stored literally until template replaces it

## Using Resources in Code

### Blazor Components
```razor
@StringResource.GetStringByKey("General_CountryFormPage_Title")
```

### With Interpolation
```razor
@StringResource.GetStringByKey("General_CountryFormPage_GeneralFieldset_IdField_Text", this.Model.Id)
```

### In C# Code
```csharp
StringResource.GetStringByKey("ExceptionCode_General_Country_DuplicateName")
```

## Reusable Component Resource Keys

These keys are used by framework components and should NOT be duplicated:

| Key | Component | Purpose |
|-----|-----------|---------|
| `DataGridControl_NewButton_Text` | DataGridControl | "New" button label |
| `DataGridControl_AddButton_Text` | DataGridControl | "Add" button label |
| `DataGridControl_EditButton_Text` | DataGridControl | "Edit" button label |
| `DataGridControl_OpenButton_Text` | DataGridControl | "Open" button label |
| `DataGridControl_DeleteButton_Text` | DataGridControl | "Delete" button label |
| `ModelFormControl_SaveButton_Text` | ModelFormControl | "Save" button label |
| `ModelFormControl_ExecuteButton_Text` | ModelFormControl | "Execute" button label |
| `ModelFormControl_CancelButton_Text` | ModelFormControl | "Cancel" button label |
| `ModelFormControl_CloseButton_Text` | ModelFormControl | "Close" button label |
| `Dialog_Confirm_Delete_Message` | Shared | Delete confirmation message |

## Language Selector

The frontend uses `Blazored.LocalStorage` to persist the selected language. The `LanguageSelector.razor` component handles switching.

## Checklist

- [ ] Keys added to `Resource.resx` (English)
- [ ] Keys added to `Resource.pt-BR.resx` (Portuguese)
- [ ] Keys added to `Resource.es-ES.resx` (Spanish)
- [ ] Comments preserved in all `.resx` files
- [ ] Alphabetical order maintained within groups
- [ ] Placeholder values use `{{}}` format
- [ ] No duplicate keys
