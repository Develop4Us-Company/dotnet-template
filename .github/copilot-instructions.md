# GitHub Copilot Instructions — AppProject .NET Template

## Overview

This is a .NET full-stack template using **ASP.NET Core API** (backend) and **Blazor WebAssembly** (frontend). The project follows a modular architecture with clear separation of concerns. The General module (Country, State, City, Neighborhood) serves as the reference implementation for all new CRUDs and modules.

## Language & Conventions

- **All code, file names, class names, and comments MUST be in English.**
- The template supports localization for `en-US`, `pt-BR`, and `es-ES` via `.resx` resource files.
- Follow StyleCop rules (configured in `Stylecop.json` and `Directory.Build.props`). Keep `using` directives ordered and remove unused ones.
- The `TargetFramework` is `net10.0` with `ImplicitUsings` and `Nullable` enabled.
- Use primary constructors for services and controllers (C# 12+).

## Project Structure

### Backend
| Project | Purpose |
|---------|---------|
| `AppProject.Core.API` | ASP.NET Core API host, authentication, middleware, CORS, bootstrap |
| `AppProject.Core.Controllers.<Module>` | REST controllers per module |
| `AppProject.Core.Services.<Module>` | CRUD services with business rules |
| `AppProject.Core.Services/<Module>` | Shared read/summary services |
| `AppProject.Core.Models.<Module>` | Module-specific DTOs |
| `AppProject.Core.Models/<Module>` | Shared DTOs (summaries, search requests) |
| `AppProject.Core.Infrastructure.Database` | EF Core context, repository, entities, configurations, mappers, migrations |
| `AppProject.Core.Infrastructure.Email` | SendGrid email abstraction |
| `AppProject.Core.Infrastructure.Jobs` | Hangfire job abstraction |
| `AppProject.Exceptions` | `AppException` and `ExceptionCode` enum |
| `AppProject.Models` | Shared interfaces (`IEntity`, `ISummary`, `ITransientService`, etc.) and generic request/response types |
| `AppProject.Resources` | Localization `.resx` files |

### Frontend
| Project | Purpose |
|---------|---------|
| `AppProject.Web` | Blazor WASM host, layout, navigation, bootstrap, OIDC auth |
| `AppProject.Web.<Module>` | Module-specific Blazor pages and components (lazy loaded) |
| `AppProject.Web.ApiClient.<Module>` | Refit CRUD client interfaces per module |
| `AppProject.Web.ApiClient/<Module>` | Shared Refit summary client interfaces |
| `AppProject.Web.Models.<Module>` | Observable models per module (with `INotifyPropertyChanged`) |
| `AppProject.Web.Models/<Module>` | Shared observable summary models |
| `AppProject.Web.Framework` | Base components (`SearchControl`, `DataGridControl`, `ModelFormControl`, `FieldsetControl`) and base pages (`SearchPage`, `ModelFormPage`) |
| `AppProject.Web.Shared` | Reusable cross-module components (dropdown data grids, etc.) |

### Tests
| Project | Purpose |
|---------|---------|
| `AppProject.Core.Tests.<Module>` | Backend service unit tests (NUnit + Moq + Shouldly + Bogus) |
| `AppProject.Web.Tests.<Module>` | Frontend tests |

## Key Patterns

### Entity DTO → inherits `IEntity`, includes `RowVersion`, uses DataAnnotations
### Summary DTO → inherits `ISummary`, no DataAnnotations, read-only fields for grids/combos
### Database Entity → `Tb[Name]` pattern, `[Table("PluralName")]`, DataAnnotations for keys/columns
### Service Interface → implements `ITransientService` + generic contracts (`IGetEntity`, `IPostEntity`, `IPutEntity`, `IDeleteEntity`)
### Summary Service Interface → implements `ITransientService` + `IGetSummaries` + `IGetSummary`
### Controller → `[Route("api/<module>/[controller]/[action]")]`, `[ApiController]`, `[Authorize]`
### Web Model → inherits `ObservableModel` + `IEntity`, uses backing fields with `Set()` for property change notification
### Refit Client → matches controller routes exactly
### Search Page → inherits `SearchPage<TRequest, TSummary>`
### Form Page → inherits `ModelFormPage<TModel>`

## Critical Rules

1. **NEVER auto-generate EF Core migration or snapshot files.** Always instruct the user to run the migration command manually.
2. **Always validate permissions** in service methods using `IPermissionService.ValidateCurrentUserPermissionAsync`.
3. **Always validate for duplicates** before insert/update operations.
4. **Throw `AppException`** with the appropriate `ExceptionCode` for business rule violations.
5. **Use `Mapster`** (`Adapt`) for DTO ↔ entity mapping. Only create custom `IRegisterMapsterConfig` when property names differ.
6. **Use `IDatabaseRepository`** for all data access — never access `DbContext` directly from services.
7. **Add resource keys** to all three `.resx` files (`Resource.resx`, `Resource.pt-BR.resx`, `Resource.es-ES.resx`) when creating new UI elements.
8. **Register new modules** in `Bootstrap.cs` (controller + service assemblies), `WebBootstrap.cs` (API client assemblies), `App.razor` (lazy loading), `NavMenu.razor` (menu items), and the `.csproj` files.
9. **Use `{{}}` instead of `{}`** for placeholder values in `.resx` files.
10. **Only create `SearchRequest` subclasses** when additional filter fields are needed. Use the base `SearchRequest` otherwise.
11. Before adding a `ProjectReference`, verify whether the dependency is already accessible via shared assemblies to prevent unnecessary cycles.

## Available Skills

This project includes agent skills in `.github/skills/` that provide detailed step-by-step instructions for common tasks. Reference them for comprehensive guidance on:
- Project architecture and conventions
- Backend CRUD creation (entities, DTOs, services, controllers)
- Frontend CRUD creation (models, Refit clients, search/form pages)
- Database entities, EF configuration, and migrations
- Unit testing patterns
- Permission system
- New module creation
- Localization and resource management
