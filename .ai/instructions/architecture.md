# Architecture Instructions

## Overview

The application is a modular .NET full-stack template with an ASP.NET Core API
and a Blazor WebAssembly frontend. The `General` module (Country, State, City,
and Neighborhood) is the reference implementation.

## Backend

| Project | Purpose |
| --- | --- |
| `AppProject.Core.API` | API host, authentication, middleware, CORS, rate limiting, and bootstrap |
| `AppProject.Core.Contracts` | Shared backend contracts such as `IUserContext`, `UserInfo`, and `CacheKeys` |
| `AppProject.Core.Controllers.<Module>` | REST controllers for a module |
| `AppProject.Core.Services.<Module>` | Module-specific CRUD services and business rules |
| `AppProject.Core.Services/<Module>` | Read and summary services shared across modules |
| `AppProject.Core.Models.<Module>` | Module-specific DTOs and request objects |
| `AppProject.Core.Models/<Module>` | DTOs and requests shared across modules |
| `AppProject.Core.Infrastructure.Database` | EF Core context, repository, entities, configurations, mappers, and migrations |
| `AppProject.Core.Infrastructure.Email` | SendGrid email abstraction |
| `AppProject.Core.Infrastructure.Jobs` | Hangfire job abstraction |
| `AppProject.Exceptions` | `AppException` and `ExceptionCode` |
| `AppProject.Models` | Shared interfaces and generic request/response models |
| `AppProject.Resources` | Localization resources |
| `AppProject.Utils` | Utility and helper classes |

## Frontend

| Project | Purpose |
| --- | --- |
| `AppProject.Web` | Blazor WASM host, layout, navigation, bootstrap, and OIDC authentication |
| `AppProject.Web.<Module>` | Lazy-loaded module pages and components |
| `AppProject.Web.ApiClient.<Module>` | Module-specific Refit CRUD clients |
| `AppProject.Web.ApiClient/<Module>` | Refit summary clients shared across modules |
| `AppProject.Web.Models.<Module>` | Module-specific observable models |
| `AppProject.Web.Models/<Module>` | Observable models shared across modules |
| `AppProject.Web.Framework` | Base components and pages |
| `AppProject.Web.Shared` | Reusable cross-module components |

## Tests

| Project | Purpose |
| --- | --- |
| `AppProject.Core.Tests.<Module>` | Backend unit tests using NUnit, Moq, Shouldly, and Bogus |
| `AppProject.Web.Tests.<Module>` | Frontend tests |

## Placement rule

Place an artifact in a shared root project only when multiple modules consume it.
Prefer the module-specific project by default.

## Registration

New modules may require changes to:

- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` for controller and service assemblies;
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` for Refit client assemblies;
- `src/AppProject.Web/App.razor` for lazy loading;
- `src/AppProject.Web/AppProject.Web.csproj` for project references and
  `BlazorWebAssemblyLazyLoad` entries;
- `src/AppProject.Web/Layout/NavMenu.razor` for navigation and permission checks.

Services implementing `ITransientService`, `IScopedService`, or
`ISingletonService` are registered through Scrutor assembly scanning. Mapster
configurations implementing `IRegisterMapsterConfig`, EF Core
`IEntityTypeConfiguration<T>` implementations, and Refit clients are also
discovered by the existing bootstrap mechanisms.

For explanatory documentation, see [`../../docs/architecture.md`](../../docs/architecture.md).
