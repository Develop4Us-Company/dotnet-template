# Project structure

- **Backend**
  - `src/AppProject.Core.API`: ASP.NET Core API with authentication, exception middleware, CORS configuration, Rate Limiting, and service bootstrap.
  - `src/AppProject.Core.Controllers.<Module>` (for example, `AppProject.Core.Controllers.General`): REST controllers for each module.
  - `src/AppProject.Core.Services.<Module>` (for example, `AppProject.Core.Services.General`): transactional (CRUD) services with business rules.
  - `src/AppProject.Core.Services/<Module>` (for example, `AppProject.Core.Services/General`): shared read and summary services.
  - `src/AppProject.Core.Models` / `src/AppProject.Core.Models.<Module>`: DTOs and request objects. Use the common folder for shared artifacts and the module-named folder for specific items.
  - `src/AppProject.Core.Infrastructure.Database`: EF Core context, generic repository, entities, and `EntityTypeConfiguration`.
  - `src/AppProject.Core.Infrastructure.Email`: email sending abstraction via SendGrid.
  - `src/AppProject.Core.Infrastructure.AI`: GitHub Models integration for AI scenarios.
- **Frontend**
  - `src/AppProject.Web`: Blazor WebAssembly host, OIDC authentication, layout, navigation, and bootstrap.
  - `src/AppProject.Web.<Module>` (for example, `AppProject.Web.General`): module-specific pages and components loaded via lazy loading.
  - `src/AppProject.Web.ApiClient` / `src/AppProject.Web.ApiClient.<Module>`: Refit interfaces to consume the API (keep shared clients separate from module-specific ones).
  - `src/AppProject.Web.Models` / `src/AppProject.Web.Models.<Module>`: observable models used in the forms.
  - `src/AppProject.Web.Framework`: base components and pages (SearchControl, DataGridControl, ModelFormPage, etc.).
  - `src/AppProject.Web.Shared`: components shared across multiple modules (for example, dropdowns backed by grids).
- **Tests**
  - `src/AppProject.Core.Tests.<Module>` (for example, `AppProject.Core.Tests.General`): backend service unit tests using NUnit, Moq, Shouldly, and Bogus. Create additional projects as you add new modules or keep shared scenarios in projects without the module suffix.
  - `src/AppProject.Web.Tests.<Module>` (for example, `AppProject.Web.Tests.General`): starting point for frontend tests; adapt it for new modules or use shared projects when it makes sense.

# Project specifications
Here are a few project specifications.
* We use the English language in code and file names.
* The template already supports localization (`en-US`, `pt-BR`, and `es-ES`) in both the API and the frontend.
* The frontend uses Radzen for UI components, Refit for HTTP clients, and OIDC authentication with Auth0.
* The code style is validated with StyleCop (see `src/Stylecop.json`) and with the shared settings in `src/Directory.Build.props`; run the analyzer locally and keep the `using` directives only when necessary and ordered to avoid violations (thus avoiding unused usings).
* The backend and frontend projects run with the configured `TargetFramework` and use enabled `implicit usings` and `nullable`.
