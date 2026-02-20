---
name: dotnet-template-architecture
description: Comprehensive reference for the AppProject .NET template architecture, project structure, conventions, and configuration. Use when the user asks about how the project is structured, naming conventions, configuration files, external integrations (Auth0, SendGrid, GitHub AI), or general architecture questions.
metadata:
  author: appproject
  version: "1.0"
---

# AppProject .NET Template — Architecture & Conventions

## Project Overview

This is a .NET full-stack template using **ASP.NET Core API** (backend) and **Blazor WebAssembly** (frontend). It follows a modular architecture with clear separation of concerns. The **General** module (Country, State, City, Neighborhood) serves as the reference implementation for all new CRUDs and modules.

## Language & Coding Conventions

- **All code, file names, class names, and comments MUST be in English.**
- The template supports localization for `en-US`, `pt-BR`, and `es-ES` via `.resx` resource files.
- Follow StyleCop rules (configured in `Stylecop.json` and `Directory.Build.props`). Keep `using` directives ordered and remove unused ones.
- The `TargetFramework` is `net10.0` with `ImplicitUsings` and `Nullable` enabled.
- Use primary constructors for services and controllers (C# 12+).
- Use `LangVersion` latest.

## Backend Project Structure

| Project | Purpose |
|---------|---------|
| `AppProject.Core.API` | ASP.NET Core API host, authentication, middleware, CORS, Rate Limiting, bootstrap |
| `AppProject.Core.Contracts` | Shared backend contracts (`IUserContext`, `UserInfo`, `CacheKeys`) |
| `AppProject.Core.Controllers.<Module>` | REST controllers per module (e.g., `AppProject.Core.Controllers.General`) |
| `AppProject.Core.Services.<Module>` | CRUD services with business rules (e.g., `AppProject.Core.Services.General`) |
| `AppProject.Core.Services/<Module>` | Shared read/summary services (e.g., `AppProject.Core.Services/General`) |
| `AppProject.Core.Models.<Module>` | Module-specific DTOs (e.g., `AppProject.Core.Models.General`) |
| `AppProject.Core.Models/<Module>` | Shared DTOs — summaries, search requests (e.g., `AppProject.Core.Models/General`) |
| `AppProject.Core.Infrastructure.Database` | EF Core context, generic repository, entities, EntityTypeConfiguration, mappers, migrations |
| `AppProject.Core.Infrastructure.Email` | SendGrid email abstraction |
| `AppProject.Core.Infrastructure.Jobs` | Hangfire job abstraction |
| `AppProject.Exceptions` | `AppException` and `ExceptionCode` enum |
| `AppProject.Models` | Shared interfaces (`IEntity`, `ISummary`, `ITransientService`, etc.) and generic request/response types |
| `AppProject.Resources` | Localization `.resx` files |
| `AppProject.Utils` | Utility/helper classes |

## Frontend Project Structure

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

## Test Project Structure

| Project | Purpose |
|---------|---------|
| `AppProject.Core.Tests.<Module>` | Backend service unit tests (NUnit + Moq + Shouldly + Bogus) |
| `AppProject.Web.Tests.<Module>` | Frontend tests |

## Key Configuration Files

### Backend Configuration
- `src/AppProject.Core.API/appsettings.json` — production base; contains `<<SET_...>>` placeholders
- `src/AppProject.Core.API/appsettings.Development.json` — local development settings

### Frontend Configuration
- `src/AppProject.Web/wwwroot/appsettings.json` — production frontend settings
- `src/AppProject.Web/wwwroot/appsettings.Development.json` — `Api:BaseUrl` → `https://localhost:7121`

### Assembly Registration Files
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — `GetControllerAssemblies()` and `GetServiceAssemblies()`
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — `GetApiClientAssemblies()`
- `src/AppProject.Web/App.razor` — lazy loading in `OnNavigateAsync()`
- `src/AppProject.Web/AppProject.Web.csproj` — `ProjectReference` and `BlazorWebAssemblyLazyLoad` entries
- `src/AppProject.Web/Layout/NavMenu.razor` — menu items and permission checks

### Project Constants
- `src/AppProject.Web/Constants/AppProjectConstants.cs` — `ProjectName`, storage keys
- `src/AppProject.Web/Constants/ThemeConstants.cs` — theme storage keys

## Shared vs. Module-Specific Content

| Root Project (Shared) | Module Project (Specific) |
|---|---|
| `AppProject.Core.Models/<Module>` | `AppProject.Core.Models.<Module>` |
| `AppProject.Core.Services/<Module>` | `AppProject.Core.Services.<Module>` |
| `AppProject.Web.ApiClient/<Module>` | `AppProject.Web.ApiClient.<Module>` |
| `AppProject.Web.Models/<Module>` | `AppProject.Web.Models.<Module>` |
| `AppProject.Web.Shared/<Module>` | `AppProject.Web.<Module>` |

**Rule:** Place files in the shared (root) project ONLY when they are consumed by multiple modules. By default, prefer the module-specific project.

## External Integrations

### Auth0
- Single Page Application type
- API with identifier matching `Auth0:Audience`
- `post_login` Action to inject `email`, `name`, `roles` into JWT
- Config keys: `Authority`, `ClientId`, `Audience`

### SendGrid
- Config keys: `SendEmail:ApiKey`, `SendEmail:FromEmailAddress`, `SendEmail:FromName`
- Email templates use Razor (see `SampleEmailTemplate.cshtml`)

### GitHub AI Models
- Config keys: `AI:Endpoint` (default `https://models.github.ai/inference`), `AI:Token`

### Administrator User
- On first API startup, bootstrap creates/updates the admin user defined in `SystemAdminUser`

## DI Registration

- Services implementing `ITransientService`, `IScopedService`, or `ISingletonService` are automatically registered via Scrutor assembly scanning in `Bootstrap.ConfigureServices()`
- Mapster configurations implementing `IRegisterMapsterConfig` are auto-loaded in `Bootstrap.ConfigureMapper()`
- EF Core `IEntityTypeConfiguration<T>` implementations are auto-loaded by `ApplicationDbContext.OnModelCreating()`
- Refit interfaces are auto-registered in `WebBootstrap.ConfigureRefit()`

## Critical Rules

1. **NEVER auto-generate EF Core migration or snapshot files.** Always instruct the user to run the migration command manually.
2. **Always validate permissions** in service methods using `IPermissionService.ValidateCurrentUserPermissionAsync`.
3. **Always validate for duplicates** before insert/update operations.
4. **Throw `AppException`** with the appropriate `ExceptionCode` for business rule violations.
5. **Use `Mapster`** (`Adapt`) for DTO ↔ entity mapping. Only create custom `IRegisterMapsterConfig` when property names differ.
6. **Use `IDatabaseRepository`** for all data access — never access `DbContext` directly from services.
7. **Add resource keys** to all three `.resx` files when creating new UI elements.
8. **Register new modules** in all bootstrap and configuration files.
9. **Use `{{}}` instead of `{}`** for placeholder values in `.resx` files.
10. Before adding a `ProjectReference`, verify whether the dependency is already accessible via shared assemblies.
