---
mode: agent
description: "Create a new module with all required projects, folders, and registrations"
---

# Create New Module

Create a brand new module in the AppProject .NET template with all required projects, folders, and bootstrap registrations.

## Required Information

You MUST ask the user for:
1. **Module name** (e.g., `Finance`, `HR`, `Inventory`)
2. **Initial entities** to create in the module
3. **Permission name** for the module

## Instructions

Follow the `new-module` skill to:

1. **Create backend projects:**
   - `AppProject.Core.Models.<Module>` — DTOs
   - `AppProject.Core.Services.<Module>` — CRUD services
   - `AppProject.Core.Controllers.<Module>` — Controllers
   - Database folders: `Entities/<Module>`, `EntityTypeConfiguration/<Module>`, `Mapper/<Module>`
   - Shared folders: `AppProject.Core.Services/<Module>`, `AppProject.Core.Models/<Module>`

2. **Create frontend projects:**
   - `AppProject.Web.<Module>` — Pages and components
   - `AppProject.Web.ApiClient.<Module>` — Refit CRUD clients
   - `AppProject.Web.Models.<Module>` — Observable models
   - Shared folders: `AppProject.Web.ApiClient/<Module>`, `AppProject.Web.Models/<Module>`, `AppProject.Web.Shared/<Module>/Components`

3. **Create test projects:**
   - `AppProject.Core.Tests.<Module>`
   - `AppProject.Web.Tests.<Module>` (optional)

4. **Register assemblies in:**
   - `Bootstrap.cs` → `GetControllerAssemblies()` and `GetServiceAssemblies()`
   - `WebBootstrap.cs` → `GetApiClientAssemblies()`
   - `App.razor` → `OnNavigateAsync()` for lazy loading
   - `AppProject.Web.csproj` → `ProjectReference` and `BlazorWebAssemblyLazyLoad`
   - `NavMenu.razor` → menu items with permission check

5. **Add to solution:**
   - Run `dotnet sln add` for each new project

6. **Add resource keys** to all three `.resx` files

After creating the module structure, proceed with the initial entities using the `backend-crud` and `frontend-crud` skills.
