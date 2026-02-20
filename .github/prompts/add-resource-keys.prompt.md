---
mode: agent
description: "Add localization resource keys to all .resx files for a new entity or feature"
---

# Add Resource Keys

Add localization resource keys to all three `.resx` files for a new entity, page, or feature.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Customer`)
2. **Module name** (e.g., `General`, `Finance`)
3. **What needs translations:** page titles, column names, field labels, validators, menu items, exception messages, or all of them
4. **Field names** and their expected labels in English, Portuguese, and Spanish (or let me provide sensible translations)

## Instructions

Follow the `localization` skill to add keys.

### Standard Keys for a Complete CRUD

For each entity, you typically need these keys in all three `.resx` files:

**Summary Page:**
- `<Module>_<Entity>SummaryPage_Title` — Page title
- `<Module>_<Entity>SummaryPage_<Column>Column_Title` — For each grid column

**Form Page:**
- `<Module>_<Entity>FormPage_Title` — Form dialog title
- `<Module>_<Entity>FormPage_GeneralFieldset_Title` — Fieldset title
- `<Module>_<Entity>FormPage_GeneralFieldset_IdField_Text` — ID label
- `<Module>_<Entity>FormPage_GeneralFieldset_<Field>Field_Label` — Field label
- `<Module>_<Entity>FormPage_GeneralFieldset_<Field>Field_Required` — Required validator
- `<Module>_<Entity>FormPage_GeneralFieldset_<Field>Field_InvalidLength` — Length validator

**Menu:**
- `NavMenu_<Module>_Title` — Menu group title (if new module)
- `NavMenu_<Module>_<Entity>_Text` — Menu item text

**Exceptions:**
- `ExceptionCode_<Module>_<Entity>_DuplicateName` — Duplicate name message

### Rules
- Add to ALL three files: `Resource.resx`, `Resource.pt-BR.resx`, `Resource.es-ES.resx`
- Preserve existing comments and grouping
- Use `{{}}` for literal placeholder text
- Follow alphabetical order within groups
