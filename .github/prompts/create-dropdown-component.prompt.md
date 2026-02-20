---
mode: agent
description: "Add a new dropdown data grid component for selecting a related entity"
---

# Create Dropdown Component

Create a reusable dropdown data grid component (`DropDownDataGridControl`) for selecting a related entity in forms and search filters.

## Required Information

You MUST ask the user for:
1. **Entity name** whose summaries will be listed (e.g., `Country`, `State`, `Product`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Display columns** in the dropdown grid (usually just `Name`)
4. **Whether the value is required** (`Guid`) or optional (`Guid?`)

## Instructions

Follow the `shared-components` skill to create the component:

1. **Create the component** at `src/AppProject.Web.Shared/<Module>/Components/<EntityName>SummaryDropDownDataGridControl.razor`

2. The component must:
   - Inherit from `DropDownDataGridControl<<EntityName>Summary, TValue>` (where TValue is `Guid` or `Guid?`)
   - Inject the `I<EntityName>SummaryClient`
   - Load data on filter/search
   - Ensure the selected item loads even after pagination/filtering
   - Display relevant columns in the dropdown grid

3. **Reference implementations:**
   - `src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor`
   - `src/AppProject.Web.Shared/General/Components/StateSummaryDropDownDataGridControl.razor`

## Usage Examples

### In a form (required FK):
```razor
<EntitySummaryDropDownDataGridControl @bind-Value=@this.Model.EntityId />
<RadzenCustomValidator Component="EntityField"
    Validator=@(() => this.Model.EntityId != Guid.Empty)
    Text=@StringResource.GetStringByKey("...Required") />
```

### In search filters (optional):
```razor
<AdvancedFilters>
    <EntitySummaryDropDownDataGridControl @bind-Value=@this.Request.EntityId
        Style="width: 300px;" />
</AdvancedFilters>
```
