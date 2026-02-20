---
mode: agent
description: "Create a backend-only CRUD for a new entity (DTOs, database entity, services, controllers)"
---

# Create Backend CRUD

Create the backend (API) side of a CRUD for a new entity in the AppProject .NET template.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Customer`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Entity fields** (name, type, required/optional, max length, foreign keys)
4. **Permission type** to use
5. **Whether summary needs aggregated fields** from related entities

## Instructions

Follow the `backend-crud` skill to create all backend files:

1. Entity DTO (`IEntity` + `RowVersion` + DataAnnotations) in `AppProject.Core.Models.<Module>`
2. Summary DTO (`ISummary`) in `AppProject.Core.Models/<Module>` or `AppProject.Core.Models.<Module>`
3. SearchRequest subclass (only if extra filters needed beyond `SearchText` and `Take`)
4. Database entity (`Tb` prefix + `BaseEntity`) in `AppProject.Core.Infrastructure.Database/Entities/<Module>`
5. EntityTypeConfiguration with indexes in `AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/<Module>`
6. DbSet in `ApplicationDbContext`
7. Mapster config (only if property names differ between entity and DTO)
8. Service interface (`ITransientService` + `IGetEntity` + `IPostEntity` + `IPutEntity` + `IDeleteEntity`)
9. Service implementation (permission check → validation → repository → Mapster mapping)
10. Summary service interface (`ITransientService` + `IGetSummaries` + `IGetSummary`)
11. Summary service implementation
12. CRUD controller (`[Authorize]`, `[ApiController]`, route `api/<module>/[controller]/[action]`)
13. Summary controller
14. ExceptionCode enum value(s) for business rule violations
15. Resource keys for exception messages in all three `.resx` files

If this is a new module, also register the assemblies in `Bootstrap.cs` (`GetControllerAssemblies` and `GetServiceAssemblies`).

**IMPORTANT:** Remind the user to run the EF Core migration manually. NEVER auto-generate migration or snapshot files.

## Final Output

List all created files and the migration command to run.
