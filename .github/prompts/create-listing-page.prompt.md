---
mode: agent
description: "Create a listing/search page for an existing entity"
---

# Create Listing Page

Create a search/listing page for an entity that already has a backend and Refit client implementation.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Country`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Columns to display** in the data grid
4. **Whether advanced filters are needed** (e.g., filter by parent entity)
5. **Whether a custom SearchRequest** exists or use base `SearchRequest`

## Instructions

Follow the search page section of the `frontend-crud` skill:

1. Create the search page at `src/AppProject.Web.<Module>/Pages/<EntityName>SummaryPage.razor`
2. The page must:
   - Use `@page "/<module_lowercase>/<plural_entity_lowercase>"`
   - Use `@attribute [Authorize]`
   - Inherit from `SearchPage<TRequest, <EntityName>Summary>`
   - Use `SearchControl` with title, SearchText, and Take bindings
   - Use `DataGridControl` with columns, New/Edit/Delete handlers
   - Implement `FetchDataAsync()` calling the summary Refit client
   - Implement `OnNewItemAsync()` opening the form dialog
   - Implement `OnEditItemAsync()` opening the form with the selected ID
   - Implement `OnDeleteItemAsync()` with confirmation dialog
3. If advanced filters are needed, add `AdvancedFilters` block with dropdown components
4. Add resource keys for:
   - Page title: `<Module>_<EntityName>SummaryPage_Title`
   - Column titles: `<Module>_<EntityName>SummaryPage_<Column>Column_Title`
5. Add menu item in `NavMenu.razor` if not already present

## Reference Implementation

Look at `src/AppProject.Web.General/Pages/CountrySummaryPage.razor` for a simple listing page and `StateSummaryPage.razor` for one with advanced filters.
