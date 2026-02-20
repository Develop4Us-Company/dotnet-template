---
mode: agent
description: "Create a complete CRUD (backend + frontend) for a new entity in the AppProject .NET template"
---

# Create Full CRUD

Create a complete CRUD implementation for a new entity in the AppProject .NET template, covering both backend and frontend.

## Required Information

You MUST ask the user for the following before starting:
1. **Entity name** (e.g., `Product`, `Customer`, `Invoice`)
2. **Module name** (e.g., `General`, `Finance`, `Inventory`) — or if a new module is needed
3. **Entity fields** (name, type, required/optional, max length, foreign keys)
4. **Permission type** to use (existing or new)
5. **Whether summary needs aggregated fields** from related entities

## Instructions

Follow the skills in this exact order:

1. **If a new module is needed**, follow the `new-module` skill first
2. **If a new permission is needed**, follow the `new-permission` skill
3. Follow the `backend-crud` skill to create:
   - Entity DTO in `AppProject.Core.Models.<Module>`
   - Summary DTO in `AppProject.Core.Models/<Module>` (shared) or `AppProject.Core.Models.<Module>`
   - SearchRequest subclass (only if extra filters needed)
   - Database entity (`Tb` prefix) in `AppProject.Core.Infrastructure.Database/Entities/<Module>`
   - EntityTypeConfiguration in `AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/<Module>`
   - DbSet in ApplicationDbContext
   - Mapster config (only if property names differ)
   - Service interface and implementation
   - Summary service interface and implementation
   - CRUD controller and Summary controller
   - ExceptionCode enum values
4. Follow the `frontend-crud` skill to create:
   - Web observable model in `AppProject.Web.Models.<Module>`
   - Web summary model in `AppProject.Web.Models/<Module>` (shared) or `AppProject.Web.Models.<Module>`
   - Refit CRUD client in `AppProject.Web.ApiClient.<Module>`
   - Refit summary client in `AppProject.Web.ApiClient/<Module>` (shared)
   - Search/listing page in `AppProject.Web.<Module>/Pages`
   - Form page in `AppProject.Web.<Module>/Pages`
   - Shared DropDown component (if FK selections needed) in `AppProject.Web.Shared/<Module>/Components`
5. Follow the `localization` skill to add all resource keys to the three `.resx` files
6. Register assemblies if this is a new module (Bootstrap.cs, WebBootstrap.cs, App.razor, NavMenu.razor, .csproj files)
7. Remind the user to run the EF Core migration command manually — NEVER auto-generate migration files

## Final Output

Present a summary of all created files and remind the user to:
- Run the EF Core migration command
- Build the solution to verify compilation
- Run the tests
