---
mode: agent
description: "Create a frontend-only CRUD for an entity (web models, Refit clients, search page, form page)"
---

# Create Frontend CRUD

Create the frontend (Blazor WebAssembly) side of a CRUD for an entity that already has a backend implementation.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Customer`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Entity fields** (to mirror in the observable model)
4. **API routes** (to match in the Refit client) — or verify from existing controllers
5. **Whether the entity has related/parent entities** (for dropdown components)

## Instructions

Follow the `frontend-crud` skill to create all frontend files:

1. Web observable model (`ObservableModel` + `IEntity` + backing fields + `Set()`) in `AppProject.Web.Models.<Module>`
2. Web summary model (`ISummary`) in `AppProject.Web.Models/<Module>` or `AppProject.Web.Models.<Module>`
3. Refit CRUD client matching controller routes in `AppProject.Web.ApiClient.<Module>`
4. Refit summary client in `AppProject.Web.ApiClient/<Module>`
5. Search/listing page (`SearchPage` base + `SearchControl` + `DataGridControl`) in `AppProject.Web.<Module>/Pages`
6. Form page (`ModelFormPage` base + `ModelFormControl` + `FieldsetControl`) in `AppProject.Web.<Module>/Pages`
7. Shared DropDown component if the entity will be used as a FK selector in `AppProject.Web.Shared/<Module>/Components`
8. Resource keys for all UI text in the three `.resx` files
9. Menu item in `NavMenu.razor` with permission check

If this is a new module, also:
- Register the API client assembly in `WebBootstrap.GetApiClientAssemblies()`
- Configure lazy loading in `App.razor`
- Add `ProjectReference` and `BlazorWebAssemblyLazyLoad` to `AppProject.Web.csproj`

## Final Output

List all created files and remind the user to build the solution.
