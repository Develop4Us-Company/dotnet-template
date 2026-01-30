# DOTNET-TEMPLATE

Este é o template que desenvolvemos para permitir que você crie softwares de forma estruturada, seguindo boas práticas de arquitetura e organização de projetos.

Também criamos um curso completo demonstrando a construção deste template passo a passo, explicando as decisões técnicas e a estrutura utilizada ao longo do desenvolvimento.

Curso: https://www.udemy.com/course/arquitetura-dotnet/?referralCode=8AD718DAF69DFED4FDD3

---

## 🔗 Materiais Visuais do Template

### Diagrama da Estrutura do Template  
https://resin-quill-61538840.figma.site/

### Arquitetura do Template .NET  
https://night-taste-61903732.figma.site/

---

Selecione o idioma:
[English](#dotnet-template-en)
[Portugues](#dotnet-template-pt)

# dotnet-template-en
Template to create .NET projects

## Table of Contents
- [Quick guide to using the template](#quick-guide-to-using-the-template)
- [Project structure](#project-structure)
- [Project specifications](#project-specifications)
- [External integrations](#external-integrations)
- [CRUD example](#crud-example)
  - [Backend](#backend)
  - [Frontend](#frontend)
  - [Tests](#tests)
- [Preparing for production](#preparing-for-production)

## Quick guide to using the template

### Prerequisites
- .NET SDK (the `TargetFramework` is already defined in `Directory.Build.props`).
- Visual Studio or Visual Studio Code with the C# extension to work with the solution.
- SQL Server. You can spin up a local container with the command:
  ```bash
  docker run --name appproject-sqlserver -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- Active Auth0, SendGrid, and GitHub accounts (with access to GitHub Models) to fill in the integrations described below.
- Optional: install the global `dotnet-ef` tool to run migration commands (`dotnet tool install --global dotnet-ef`).
- Important: do not let Copilot, Codex, or any other generator automatically create EF Core snapshot or migration files. Run the migration script/command manually (for example, `dotnet ef migrations add ...`) to make sure the code is generated correctly.

### Step-by-step to set up the environment
1. Clone the repository and restore the dependencies with `dotnet restore AppProject.slnx`.
2. Verify the .NET installation with `dotnet --info` and confirm that .NET is installed.
3. Make sure the `src/AppProject.Core.API/appsettings.Development.json` file is configured to point to your local resources (for example, the connection string `Server=localhost,1433;Database=AppProject;...`) before running the API. The other values (Auth0, SendGrid, GitHub Models, etc.) also contain placeholders for you to fill in.
4. Configure the local (or containerized) SQL Server and validate the connection with `sqlcmd` or your tool of choice.
5. Fill in the placeholders in `src/AppProject.Core.API/appsettings.json` and `src/AppProject.Web/wwwroot/appsettings.json` before generating production builds. These files contain `<<SET_...>>` markers that flag what must be configured.
6. Configure the external integrations (Auth0, SendGrid, and GitHub Models) following the detailed instructions later on and copy the generated values into the configuration files.
7. Run the API with `dotnet run --project src/AppProject.Core.API` (default port `https://localhost:7121`).
8. Run the frontend with `dotnet run --project src/AppProject.Web` (default port `https://localhost:7035`).
9. Go to `https://localhost:7035` in your browser to use the application and to `https://localhost:7121/swagger` to test the endpoints.

### Configuration file checklist
- `src/AppProject.Core.API/appsettings.json` — base file used in production. Fill in the placeholders:
  - `<<SET_SQLSERVER_DATABASE_CONNECTION_STRING>>` and `<<SET_HANGFIRE_SQLSERVER_CONNECTION_STRING>>`: connection strings (they can be the same).
  - `<<SET_AUTH0_AUTHORITY>>`, `<<SET_AUTH0_CLIENT_ID>>`, `<<SET_AUTH0_AUDIENCE>>`: Auth0 application data.
  - `<<SET_SYSTEM_ADMIN_NAME>>`, `<<SET_SYSTEM_ADMIN_EMAIL>>`: administrator user that is created automatically.
  - `<<SET_ALLOWED_CORS_ORIGINS>>`: URLs allowed to consume the API.
  - `<<SET_ALLOWED_HOSTS>>`: hosts accepted when the application runs in production.
  - `<<SET_SENDGRID_API_KEY>>`, `<<SET_SENDGRID_FROM_EMAIL>>`, `<<SET_SENDGRID_FROM_NAME>>`: email sending credentials.
  - `<<SET_GITHUB_AI_ENDPOINT>>`, `<<SET_GITHUB_AI_TOKEN>>`: GitHub Models integration configuration.
- `src/AppProject.Core.API/appsettings.Development.json` — already points to local connections (`Server=localhost,1433;...`) and keeps placeholders for sensitive credentials (Auth0, SendGrid, GitHub Models). Adjust it to your environment and avoid committing sensitive data.
- `src/AppProject.Web/wwwroot/appsettings.json` — frontend placeholders (`Auth0` and `Api:BaseUrl`). The published file must point to the production URLs.
- `src/AppProject.Web/wwwroot/appsettings.Development.json` — has `Api:BaseUrl` pointing to `https://localhost:7121` and keeps Auth0 placeholders.
- `src/AppProject.Web/Constants/AppProjectConstants.cs` — update the `ProjectName` constant with the name you want to use for your application/project.
- `src/AppProject.Web/Constants/ThemeConstants.cs` — keeps the theme storage keys aligned with the project name.
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — when creating new modules, register the assemblies in the `GetControllerAssemblies()` and `GetServiceAssemblies()` methods.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — include the Refit client assemblies in `GetApiClientAssemblies()` and validate `Api:BaseUrl`.
- `src/AppProject.Web/App.razor` — register additional assemblies in the `OnNavigateAsync` method to enable lazy loading for new modules.
- `src/AppProject.Web/AppProject.Web.csproj` — add new `ProjectReference` entries and `BlazorWebAssemblyLazyLoad` items when creating additional modules.
- `src/AppProject.Web/Layout/NavMenu.razor` — include menu items and permissions for the new modules.
- `src/AppProject.Resources/Resource*.resx` — keep translations in sync when adding new text. Preserve the comments that name each logical group, keep splitting entries per form (validations and menu remain grouped), and when you need to reserve future placeholders, use `{{}}` instead of `{}` so the parser keeps the literal text.

## Project structure
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

## Project specifications
Here are a few project specifications.
* We use the English language in code and file names.
* The template already supports localization (`en-US`, `pt-BR`, and `es-ES`) in both the API and the frontend.
* The frontend uses Radzen for UI components, Refit for HTTP clients, and OIDC authentication with Auth0.
* The code style is validated with StyleCop (see `Stylecop.json`) and with the shared settings in `Directory.Build.props`; run the analyzer locally and keep the `using` directives only when necessary and ordered to avoid violations (thus avoiding unused usings).
* The backend and frontend projects run with the configured `TargetFramework` and use enabled `implicit usings` and `nullable`.

## External integrations
The sections below describe the registrations required so every integration works. After completing each step, copy the values to the `appsettings` files.

### Auth0
1. Create an application of type **Single Page Application**.
2. Configure the application logo in *Settings* if you want.
3. Fill in the URLs (adjust the ports if you change `launchSettings.json`):
   - **Allowed Callback URLs**: `https://localhost:7035/authentication/login-callback`, `https://localhost:7121/swagger/oauth2-redirect.html`
   - **Allowed Logout URLs**: `https://localhost:7035`, `https://localhost:7121/swagger/`
   - **Allowed Web Origins**: `https://localhost:7035`, `https://localhost:7121`
4. Create an **API** in Auth0 and use as the *Identifier* the same value configured in `Auth0:Audience` (`https://appproject.api` by default). Then go to **Access Settings** and check **Allow Offline Access**.
5. To include `email`, `name`, and `roles` in the JWT, create a `post_login` Action with the script below:
   ```javascript
   if (api.accessToken) {
       if (event.user && event.user.email) {
         api.accessToken.setCustomClaim("email", event.user.email);
       }

       if (event.user && event.user.name) {
         api.accessToken.setCustomClaim("name", event.user.name);
       }

       if (event.authorization && event.authorization.roles) {
         api.accessToken.setCustomClaim("roles", event.authorization.roles);
       }
     }
   ```
6. Copy `Authority` and `ClientId` from the Single Page Application you created and copy the API `Audience` to the `appsettings`, keeping the `https://` prefix for both `Authority` and `Audience`. For example:
   ```json
   "Authority": "https://yourauth0domain.us.auth0.com",
   "ClientId": "yourclientid",
   "Audience": "https://youraudience.com"
   ```
> Note: When opening Swagger, clear the browser cache so it does not reuse parameters from other projects.

### SendGrid
1. Create an account on the [SendGrid website](https://sendgrid.com/).
2. Configure an identity (domain authentication or single sender). Authorize the identity through the email you receive.
3. In the **Email API > Integration Guide** menu, generate an API Key.
4. Send the first test email and confirm the delivery in the dashboard.
5. Copy the key and the configured sender (`SendEmail:ApiKey`, `SendEmail:FromEmailAddress`, `SendEmail:FromName`).
6. When creating new emails, model the body in a Razor template (`SampleEmailTemplate.cshtml` and `SampleEmailModel` in `AppProject.Core.Infrastructure.Email` are examples) and only fill the model inside the service, avoiding inline strings.

### GitHub AI Models
1. Follow the official documentation: <https://docs.github.com/en/github-models/use-github-models/prototyping-with-ai-models>.
2. Generate a token with permission to use the models hosted by GitHub at [https://github.com/settings/tokens](https://github.com/settings/tokens).
3. Fill `AI:Endpoint` (default `https://models.github.ai/inference`) and `AI:Token` with the generated token.

### Administrator user
When the API runs for the first time, the bootstrap creates or updates the administrator user defined in `SystemAdminUser`. Use this user to ensure at least one account has permission to access every registry.

## CRUD example
The example below shows, step by step, how the General module implements the Country, State, City, and Neighborhood registries. Follow the same steps when creating new modules.

### Backend

#### 1. Identify the module
First, identify which module will contain your new entity. For example, imagine the registries for states, cities, and countries. These registries belong to a General module. Therefore, we use the General folders whenever possible inside the project.

##### If you need to create a new module
If you determine a new module is required, use the **General** module as a reference and create/adjust the items below (replace `NewModule` with the desired name):
- **Backend**
  - Project `AppProject.Core.Models.NewModule` with the module DTOs.
  - Project `AppProject.Core.Services.NewModule` containing the CRUD services.
  - `NewModule` folder inside `AppProject.Core.Services` for the summary services.
  - Project `AppProject.Core.Controllers.NewModule`.
  - `Entities/NewModule` and `EntityTypeConfiguration/NewModule` folders inside `AppProject.Core.Infrastructure.Database` for the entities and EF Core configurations.
  - Specific migrations in the `AppProject.Core.Infrastructure.Database` project.
- **Frontend**
  - Project `AppProject.Web.NewModule` with the Blazor pages and components.
  - Project `AppProject.Web.ApiClient.NewModule` with the CRUD Refit interfaces.
  - `NewModule` folder inside `AppProject.Web.ApiClient` for CRUD Refit interfaces shared among other modules.
  - Project `AppProject.Web.Models.NewModule` with the client observable models.
  - `NewModule` folder inside `AppProject.Web.Models` for client observable models shared across modules.
  - `NewModule` folder in `AppProject.Web.Shared` for reusable components (dropdowns, cards, etc.).
- **Tests**
  - Project `AppProject.Core.Tests.NewModule` covering the backend services.
  - Project `AppProject.Web.Tests.NewModule` (optional) for UI/integration scenarios.

You must also edit the following files to register the new module assembly:
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — include the assembly in the `GetControllerAssemblies()` and `GetServiceAssemblies()` methods.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — register the assembly in `GetApiClientAssemblies()` and adjust lazy loading if necessary.
- `src/AppProject.Web/App.razor` — add the assembly to the `OnNavigateAsync` conditions to enable lazy loading.
- `src/AppProject.Web/AppProject.Web.csproj` — create the `ProjectReference` entries and `BlazorWebAssemblyLazyLoad` items.
- `src/AppProject.Web/Layout/NavMenu.razor` — include the menu item and related permissions.
- `src/AppProject.Resources/Resource*.resx` — add the new translation keys, following the existing structure, keeping the comments, and matching the existing tabs and characteristics.

Before adding a `ProjectReference`, verify whether the dependency is already accessible via shared assemblies (for example, `AppProject.Core.Services` already references the Jobs projects, so a new module does not need to reference Jobs directly). This prevents unnecessary cycles and builds.

#### 2. Content shared across modules
If you are adding files shared among modules, place those files in the root project instead of the module-specific project. For instance, imagine you are adding the Customer table. Customer can be used in several modules (invoice, financial, etc.). In that case, rather than creating a General or Customer module, the right approach is to place it in the root project inside a folder representing the shared module.

Below is a list of root projects where you can create folders representing parts of shared modules:
* `AppProject.Core.Models` — shared DTOs and requests. Always analyze whether the files must be shared with other modules. Prefer keeping files in the module project instead of the shared project.
* `AppProject.Core.Services` — common services, for example, summaries visible in multiple modules. However, not every summary should live here. Only summaries shared between modules belong here.
* `AppProject.Web` — layout, authentication, bootstrap, and navigation components.
* `AppProject.Web.ApiClient` — Refit interfaces reused by more than one module.
* `AppProject.Web.Models` — observable models used by multiple modules.
* `AppProject.Web.Shared` — generic Blazor components (dropdowns, cards, helper controls).
* `AppProject.Resources` — translations reused in different areas.

#### 3. Adding DTOs to the API
The project contains API-side DTOs and WEB-side (client) DTOs. They differ because the WEB-side DTOs can have change notifications (`INotifyPropertyChanged`), while the API-side ones do not.

DTOs are added to the projects below:
* AppProject.Core.Models (for DTOs shared between modules; create internal folders with the module name when needed — for example, `AppProject.Core.Models/General`).
* AppProject.Core.Models.<Module> (for example, `AppProject.Core.Models.General`) for module-specific DTOs.
* Create additional projects such as `AppProject.Core.Models.<NewModule>` if you need to separate DTOs per module.

In a CRUD scenario we usually have two DTO types: Entity DTOs (inheriting from `IEntity`) and Summary DTOs (inheriting from `ISummary`).

##### Entity DTOs
Entity DTOs represent the table fields and must inherit from `IEntity`. They need to expose `RowVersion` to support optimistic concurrency and use DataAnnotations to validate required fields, maximum length, and basic rules. References:
- `AppProject.Core.Models.General/Country.cs` — simple entity with `Name`, `Code`, and `RowVersion`.
- `AppProject.Core.Models.General/State.cs` — adds `CountryId`, validated by `RequiredGuid`.
- `AppProject.Core.Models.General/City.cs` — besides the main fields, it keeps the `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` collections to synchronize neighborhoods.
- `AppProject.Core.Models.General/Neighborhood.cs` — basic neighborhood structure used in both the API and the frontend.

###### Entity validations
Validation exceptions return `ExceptionDetail` with the `RequestValidation` code. All custom attributes live in `AppProject.Models.CustomValidators`:
- `ValidateCollectionAttribute` guarantees cascading validation for lists.
- `RequiredGuidAttribute` prevents sending empty GUIDs.

The middleware [`AppProject.Core.API/Middlewares/ExceptionMiddleware.cs`](./src/AppProject.Core.API/Middlewares/ExceptionMiddleware.cs) converts exceptions into standardized responses, while `Bootstrap.ConfigureValidations` forces `AppException` to be thrown when `ModelState` is invalid.

For relationships, keep only the identifiers (for example, `StateId` in `City`). When dealing with aggregated entities, such as city neighborhoods, use the parent DTO collections (`City.cs`) of `CreateOrUpdateRequest`/`DeleteRequest` to indicate inserts, updates, and deletions.

##### Summary DTOs
Use summary DTOs to power grids, combos, and other read queries. They inherit from `ISummary`, have no DataAnnotations, and contain only the fields needed to display information in the UI:
- `AppProject.Core.Models/General/CountrySummary.cs` — example from the General module with `Id` and `Name`.
- `AppProject.Core.Models/General/StateSummary.cs` — example from the General module with `CountryName` and `CountryId`.
- `AppProject.Core.Models/General/CitySummary.cs` — example from the General module with `StateName`, `StateId`, `CountryName`, and `CountryId`.

For advanced searches, use `SearchRequest` as the base class and add specific properties:
- `AppProject.Core.Models/General/StateSummarySearchRequest.cs` — allows filtering states by `CountryId`.
- `AppProject.Core.Models/General/CitySummarySearchRequest.cs` — filters cities by `StateId`.
Avoid creating empty `SearchRequest` derivatives. Only create a class derived from SearchRequest when extra filters exist. For example, there is no `CountrySummarySearchRequest` because we do not add extra filters to the country search the way we do for the StateSummarySearchRequest and CitySummarySearchRequest.

Always evaluate whether the summary should bring aggregated names instead of full objects. That simplifies serialization and avoids unnecessary loads or reference cycles.

#### 4. Adding database entities
The database entities live in `AppProject.Core.Infrastructure.Database/Entities` and follow the `Tb[Name]` pattern. Recommendations:
- `TbCountry.cs`, `TbState.cs`, `TbCity.cs`, `TbNeighborhood.cs` — keep DataAnnotations for keys, column sizes, and relationships. Use plural table names (`[Table("Countries")]`) and configure navigation collections (`States`, `Cities`, `Neighborhoods`) to simplify loading.
- Store only the information required for persistence; any additional logic must stay in the services.
- Apply `MaxLength` to text columns and keep plausible values for fields shared with the DTOs.

Relationships are modeled with foreign key fields (for example, `CountryId` in `TbState`) and navigation properties with `[ForeignKey]`. When creating additional entities, follow the same pattern so EF Core configures the constraints automatically.

##### Adding the EntityTypeConfiguration files
Configuration classes complement the entities with indexes, extra constraints, and EF Core-specific rules. They live in `AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/[Module]` and follow the `Tb[Name]Configuration` pattern. Examples:
- `TbCountryConfiguration.cs` — defines a unique index for `Name`.
- `TbStateConfiguration.cs` — creates an index to speed up searches by `Name`.
- `TbCityConfiguration.cs` and `TbNeighborhoodConfiguration.cs` — configure indexes for dependent entities.

All of them inherit from `IEntityTypeConfiguration<T>` and are automatically loaded by `ApplicationDbContext`. If you need new constraints (for example, composite indexes), implement them in these files instead of bloating the entities with extra logic.

Important: when adding a new file that inherits from `IEntityTypeConfiguration`, you do not need to register it manually in `ApplicationDbContext`; the `OnModelCreating` method already scans the assembly and applies each configuration.

##### Adding DbSet to ApplicationDbContext
Update [`ApplicationDbContext`](./src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs) whenever you create a new entity. Each table must contain a `DbSet<T>` with a plural name (for example, `Countries`, `States`, `Cities`, `Neighborhoods`). Keeping this convention improves readability and avoids mismatches between EF Core and the database.

##### Running migrations
To generate the database scripts, we need to run the Entity Framework migration. Open the terminal in the project's src folder and run the following command:

```bash
dotnet ef migrations add MigrationName --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

Replace MigrationName with something that identifies your migration, such as the name of one of the tables you are modifying or a subject related to the change.

This command creates the migration script files under the Migrations folder of the `AppProject.Core.Infrastructure.Database` project.

Important: you do not need to apply the migration manually because it is applied automatically when the API starts.

#### 5. Adding Mapster configuration classes
By default, Mapster can map entities to DTOs when the property names match. Create additional configurations only when you need to transform fields or include relationship data. In the General module we use:
- `AppProject.Core.Infrastructure.Database/Mapper/General/StateSummaryMapsterConfig.cs` — injects `CountryName` when mapping `TbState` → `StateSummary`.
- `AppProject.Core.Infrastructure.Database/Mapper/General/CitySummaryMapsterConfig.cs` — exposes `StateName` and `CountryName` from the related entities.

These classes implement `IRegisterMapsterConfig` and are loaded in the bootstrap (`Bootstrap.ConfigureMapper`). When adding new configurations:
1. Create the `[Dto]MapsterConfig.cs` file inside the `Mapper/[Module]` folder.
2. Configure `TypeAdapterConfig` in the `Register` method.
3. Avoid complex logic in the mapper; use the services to handle business rules.

Services centralize business rules, validations, and repository orchestration. They live in `AppProject.Core.Services` for shared items and in `AppProject.Core.Services.<Module>` (for example, `AppProject.Core.Services.General`) for module-specific implementations. All compatible types are automatically registered in DI by `Bootstrap.ConfigureServices`.

##### Service class interfaces
- `ICountryService.cs`, `IStateService.cs`, and `ICityService.cs` implement `ITransientService` and the generic contracts `IGetEntity`, `IPostEntity`, `IPutEntity`, and `IDeleteEntity`. That standardizes the CRUD signatures and keeps the API consistent.
- `ICityService` adds `GetNeighborhoodEntitiesAsync`, which returns neighborhoods associated with a city using `GetByParentIdRequest<Guid>`.
- The summary interfaces (`ICountrySummaryService.cs`, `IStateSummaryService.cs`, `ICitySummaryService.cs`) expose `IGetSummaries`/`IGetSummary` with their respective requests (`SearchRequest`, `StateSummarySearchRequest`, `CitySummarySearchRequest`). Follow this approach when creating new summaries.

##### Service classes
- `CountryService.cs`, `StateService.cs`, and `CityService.cs` are responsible for:
  1. Validating permissions with `IPermissionService.ValidateCurrentUserPermissionAsync` using `PermissionType.System_ManageSettings`.
  2. Running business validations (`ValidateCountryAsync`, `ValidateStateAsync`, `ValidateCityAsync`) to avoid duplicates and inconsistencies.
  3. Using `IDatabaseRepository` to query (`GetFirstOrDefaultAsync`, `GetByConditionAsync`), insert (`InsertAndSaveAsync`), update (`UpdateAndSaveAsync`), or delete (`DeleteAndSaveAsync`) records.
  4. Mapping DTOs ↔ entities via `Mapster` (`Adapt`).
- `CityService` deserves attention because it handles aggregates:
  - Neighborhood persistence occurs through the `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` lists from the city DTO.
  - The `ValidateCityAsync` and `ValidateNeighborhoodsBelongToCityAsync` methods avoid duplicate names and ensure the neighborhoods truly belong to the city being edited.
  - Bulk inserts use `InsertAsync`/`UpdateAsync` followed by `SaveAsync` to guarantee atomicity.
- Every service throws `AppException` with the appropriate `ExceptionCode` (`EntityNotFound`, `General_*_DuplicateName`, etc.), ensuring messages are translated through resources.

##### Summary service classes
- `CountrySummaryService.cs`, `StateSummaryService.cs`, and `CitySummaryService.cs` handle read queries. They call `GetByConditionAsync` with filters (`SearchText`, `Take`, `CountryId`, `StateId`) and use `SummariesResponse<T>` to return immutable collections.
- When `GetSummaryAsync` cannot find the record, they throw `AppException(ExceptionCode.EntityNotFound)` to stay consistent with the write services.
- As you expand the template, follow this pattern: keep read services free from expensive permission validations (unless specific requirements exist) and centralize filters in request objects so the frontend can reuse them.
- Avoid redundant `Include` statements in summary queries when the columns are already projected by the view/adapter.

#### 7. Creating controller classes
Controllers live in module-specific projects such as `AppProject.Core.Controllers.<Module>` (for example, `AppProject.Core.Controllers.General`). They expose only the logic needed to receive the requests, call the services, and return the standardized result (`Ok(...)`). Examples:
- `CountryController.cs`, `StateController.cs`, `CityController.cs` — implement CRUD endpoints for each entity.
- `CityController` also offers `GetNeighborhoodsAsync` to fetch related neighborhoods.
- `CountrySummaryController.cs`, `StateSummaryController.cs`, `CitySummaryController.cs` — expose query endpoints (`GetSummariesAsync`, `GetSummaryAsync`).

General guidelines:
- Apply `[Authorize]` to protect the endpoints and `[ApiController]` to enable automatic model validation.
- Use the `api/<module>/[controller]/[action]` route pattern (for example, `api/general/Country/Post`).
- Receive parameters using `[FromQuery]` for searches (`GetByIdRequest`, `DeleteRequest`) and `[FromBody]` for mutations (`CreateOrUpdateRequest`).
- Always return `IActionResult` with `Ok(...)` to keep consistency and simplify the global error handling.

### Frontend
The frontend is a **Blazor WebAssembly** application that consumes the API via Refit and uses Radzen components. The General module implementation serves as the guide for new modules.

#### Overview and key files
- Client-side models live in `AppProject.Web.Models` and `AppProject.Web.Models.<Module>` (for example, `AppProject.Web.Models.General`). All of them inherit from [`ObservableModel`](./src/AppProject.Web.Models/ObservableModel.cs), which implements `INotifyPropertyChanged` to update the UI automatically.
- Entity classes such as [`Country.cs`](./src/AppProject.Web.Models.General/Country.cs), [`State.cs`](./src/AppProject.Web.Models.General/State.cs), [`City.cs`](./src/AppProject.Web.Models.General/City.cs), and [`Neighborhood.cs`](./src/AppProject.Web.Models.General/Neighborhood.cs) mirror the API DTOs. In the case of `City`, we keep the `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` collections to send nested changes.
- Summaries used in grids and combos stay in folders like [`AppProject.Web.Models/<Module>`](./src/AppProject.Web.Models/General). Examples for the General module: [`CountrySummary.cs`](./src/AppProject.Web.Models/General/CountrySummary.cs), [`StateSummary.cs`](./src/AppProject.Web.Models/General/StateSummary.cs), and [`CitySummary.cs`](./src/AppProject.Web.Models/General/CitySummary.cs).

#### HTTP clients with Refit
- CRUD interfaces live in projects such as [`AppProject.Web.ApiClient.<Module>`](./src/AppProject.Web.ApiClient.General). Example: [`ICityClient.cs`](./src/AppProject.Web.ApiClient.General/ICityClient.cs) covers every `CityController` endpoint in the General module.
- Summaries have separate clients inside folders like [`AppProject.Web.ApiClient/<Module>`](./src/AppProject.Web.ApiClient/General). See the General module files such as [`ICitySummaryClient.cs`](./src/AppProject.Web.ApiClient/General/ICitySummaryClient.cs) and [`ICountrySummaryClient.cs`](./src/AppProject.Web.ApiClient/General/ICountrySummaryClient.cs).
- The bootstrap [`WebBootstrap.cs`](./src/AppProject.Web/Bootstraps/WebBootstrap.cs) dynamically registers every Refit interface defined in the assemblies listed by `GetApiClientAssemblies()`. When you add a new module, include the corresponding assembly.

#### Search pages
- Search pages inherit from [`SearchPage<TRequest,TSummary>`](./src/AppProject.Web.Framework/Pages/SearchPage.cs), which encapsulates executing searches, selecting items, and displaying alerts when the `Take` limit is reached.
- The [`SearchControl.razor`](./src/AppProject.Web.Framework/Components/SearchControl.razor) component provides a standard form with a text field, advanced filters, and a configurable alert.
- Examples:
  - [`CountrySummaryPage.razor`](./src/AppProject.Web.General/Pages/CountrySummaryPage.razor) shows the grid with countries and the `New`, `Edit`, and `Delete` operations.
- [`StateSummaryPage.razor`](./src/AppProject.Web.General/Pages/StateSummaryPage.razor) adds a country filter via `CountrySummaryDropDownDataGridControl`.
- [`CitySummaryPage.razor`](./src/AppProject.Web.General/Pages/CitySummaryPage.razor) filters by state and displays additional columns.
- The standard grid is [`DataGridControl.razor`](./src/AppProject.Web.Framework/Components/DataGridControl.razor), which accepts `GlobalActions`, `ContextActions`, and integrates with multi-selection.

#### Form pages and nested items
- Forms inherit from [`ModelFormPage<TModel>`](./src/AppProject.Web.Framework/Pages/ModelFormPage.cs) and use the [`ModelFormControl.razor`](./src/AppProject.Web.Framework/Components/ModelFormControl.razor) component.
- Reuse the model properties (`TModel`) directly for binds and selections instead of creating duplicate properties (for example, avoid `SelectedId`/`SelectedAnotherField`).
- DropDown components must inherit from `DropDownDataGridControl`. When using them, ensure `TValue` is correct (`Guid` for required DTO fields and `Guid?` if the field is optional).
- `ModelFormControl` allows important customizations:
  - `ShowNewAction`, `ShowEditAction`, and `ShowDeleteAction` in `DataGridControl` control which buttons are shown.
  - `PreferAddOverNew` changes the default button text to "Add" (as in [`CityFormPage.razor`](./src/AppProject.Web.General/Pages/CityFormPage.razor) when managing neighborhoods).
  - `PreferOpenOverEdit` displays "Open" instead of "Edit", useful for read-only pages.
  - `PreferExecuteOverSave` renames the main button to "Execute", suitable for processing screens.
  - `PreferCloseOverCancel` applies the closing style to the secondary button.
- For nested relationships, follow the `CityFormPage` example:
  - Load child records via a dedicated client (`ICityClient.GetNeighborhoodsAsync`).
  - When inserting/updating an item, add it to `ChangedNeighborhoodRequests`.
  - When deleting, move the identifier to `DeletedNeighborhoodRequests`.
  - Use specialized dialogs (such as [`NeighborhoodFormPage.razor`](./src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor)) to edit child items.
- Always consult the Radzen component documentation before setting attributes like `Min`/`Max`. For decimal values use expressions like `@(0.01m)` in `RadzenNumeric` to ensure the value is treated as `decimal`, and avoid redundant validators when the property already has `[Required]` in the model.

#### Full flow for the General module records
1. **Country**
   - Search: [`CountrySummaryPage.razor`](./src/AppProject.Web.General/Pages/CountrySummaryPage.razor) calls `ICountrySummaryClient` and, after confirming deletions, uses `ICountryClient.DeleteAsync`.
   - Form: [`CountryFormPage.razor`](./src/AppProject.Web.General/Pages/CountryFormPage.razor) opens inside `DialogService`, reusing the same component to create or edit. Visual validations use `RadzenRequiredValidator` and `RadzenLengthValidator`.
2. **State**
   - Search: [`StateSummaryPage.razor`](./src/AppProject.Web.General/Pages/StateSummaryPage.razor) adds a country filter via [`CountrySummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor).
   - Form: [`StateFormPage.razor`](./src/AppProject.Web.General/Pages/StateFormPage.razor) requires selecting a country. The `Guid` validation uses `RadzenCustomValidator` to block `Guid.Empty`.
3. **City**
   - Search: [`CitySummaryPage.razor`](./src/AppProject.Web.General/Pages/CitySummaryPage.razor) combines state filters, ordering, and extra columns (`StateName`, `CountryName`).
   - Form: [`CityFormPage.razor`](./src/AppProject.Web.General/Pages/CityFormPage.razor) uses another `DataGridControl` to manage neighborhoods and sets `PreferAddOverNew` to reflect inserting child items. The `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` lists update whenever the user confirms the neighborhood dialog.
4. **Neighborhoods**
   - Child form: [`NeighborhoodFormPage.razor`](./src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor) inherits from `ModelFormPage<Neighborhood>` and returns the object via `CloseDialogAsync`. The component is used both to create and edit records within the city.

#### Reusable components
- [`DataGridControl.razor`](./src/AppProject.Web.Framework/Components/DataGridControl.razor) wraps `RadzenDataGrid` with multi-selection, localization, and configurable buttons (`ShowNewAction`, `ShowEditAction`, `ShowDeleteAction`, `PreferAddOverNew`, `PreferOpenOverEdit`).
- [`ModelFormControl.razor`](./src/AppProject.Web.Framework/Components/ModelFormControl.razor) standardizes the form header, includes `GlobalActions` slots, and exposes the `PreferExecuteOverSave` / `PreferCloseOverCancel` toggles.
- [`FieldsetControl.razor`](./src/AppProject.Web.Framework/Components/FieldsetControl.razor) creates collapsible fieldsets with centralized translations.
- [`SearchControl.razor`](./src/AppProject.Web.Framework/Components/SearchControl.razor) provides basic/advanced filters and the automatic alert for `Take`.
- [`DropDownDataGridControl.cs`](./src/AppProject.Web.Framework/Components/DropDownDataGridControl.cs) adjusts the `RadzenDropDownDataGrid` texts and is reused by [`CountrySummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor) and [`StateSummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/StateSummaryDropDownDataGridControl.razor), ensuring the selected item loads even after pagination/filtering.
- [`BusyIndicatorControl.razor`](./src/AppProject.Web.Framework/Components/BusyIndicatorControl.razor) is used by [`AppProjectComponentBase`](./src/AppProject.Web.Framework/Components/AppProjectComponentBase.cs) to display progress dialogs and handle exceptions (including `ApiException` responses from Refit).
- Layout and global preferences:
  - [`LanguageSelector.razor`](./src/AppProject.Web/Layout/LanguageSelector.razor) persists the selected culture using `Blazored.LocalStorage`.
  - [`ThemeToggle.razor`](./src/AppProject.Web/Layout/ThemeToggle.razor) toggles between the themes defined in [`ThemeConstants`](./src/AppProject.Web/Constants/ThemeConstants.cs).
  - [`Login.razor`](./src/AppProject.Web/Layout/Login.razor) summarizes the OIDC authentication flow (login/logout).

#### Localization and resources
- Both the API and the frontend consume the resources defined in `AppProject.Resources`. The helper [`StringResource.cs`](./src/AppProject.Resources/StringResource.cs) reads [`Resource.resx`](./src/AppProject.Resources/Resource.resx), [`Resource.pt-BR.resx`](./src/AppProject.Resources/Resource.pt-BR.resx), and [`Resource.es-ES.resx`](./src/AppProject.Resources/Resource.es-ES.resx).
- When adding new screens or messages, include the keys in all three files to keep multilingual support.
- Preserve the comments and grouping (forms separated, validations and menus together) when editing the `.resx` files; each new key must have its own line following the existing pattern to make diffs easier.
- When you need to reserve placeholders for values interpolated later, use `{{Value}}` instead of `{Value}` so the resource parser treats the text as literal until the template replaces it.
- Reusable components consume specific keys (`DataGridControl_NewButton_Text`, `DataGridControl_AddButton_Text`, `ModelFormControl_SaveButton_Text`, `ModelFormControl_ExecuteButton_Text`, among others). Adjust these keys to customize labels without changing the code.

#### Lazy loading, navigation, and bootstrap
- [`AppProject.Web/App.razor`](./src/AppProject.Web/App.razor) loads assemblies on demand. Routes starting with `general` load `AppProject.Web.General.dll`. For new modules, replicate the logic by adding the route prefix and the assembly.
- [`WebBootstrap.ConfigureRefit`](./src/AppProject.Web/Bootstraps/WebBootstrap.cs) registers all HTTP clients. Update `GetApiClientAssemblies()` with the new assemblies so every interface is registered automatically.
- [`AppProjectConstants`](./src/AppProject.Web/Constants/AppProjectConstants.cs) defines the name displayed in the header and the storage prefixes; adjust it when customizing the template.

#### Menu and permissions
- [`NavMenu.razor`](./src/AppProject.Web/Layout/NavMenu.razor) queries `IPermissionClient` and displays the General module items only for users with `PermissionType.System_ManageSettings`.
- When creating new modules, add the corresponding menu items and evaluate which permissions they require.

### Tests
The unit tests live in projects such as `src/AppProject.Core.Tests.<Module>` (for example, `src/AppProject.Core.Tests.General`) and rely on **NUnit**, **Moq**, **Shouldly**, and **Bogus**. They validate both positive scenarios and expected exceptions.

- [`CountryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CountryServiceTests.cs): covers reading, inserting, updating, and deleting countries, and validates duplicates and authorization.
- [`StateServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/StateServiceTests.cs): ensures validation of duplicate names per country and the CRUD behavior for states.
- [`CityServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CityServiceTests.cs): exercises the nested neighborhood logic, duplicates, and relationships during `Post`, `Put`, and `Delete`.
- [`CountrySummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CountrySummaryServiceTests.cs): tests text filters and the handling of missing entities.
- [`StateSummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/StateSummaryServiceTests.cs): evaluates filters by `CountryId` and individual retrieval.
- [`CitySummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CitySummaryServiceTests.cs): ensures filters by `StateId` and `SearchText` work and that the proper exceptions are thrown.

Each test class follows the Arrange/Act/Assert pattern, initializing `IDatabaseRepository` and `IPermissionService` *mocks* and using `Bogus` to generate reliable data. The helper method `AssertAppExceptionAsync` (defined in each test class) simplifies verifying the messages/`ExceptionCode` returned by the services. When creating new scenarios:
- Configure the permission mock to return `Task.CompletedTask` (keeping the default service behavior).
- Use Moq `Setup`/`ReturnsAsync` to simulate EF Core queries (for example, `GetFirstOrDefaultAsync`, `HasAnyAsync`, `GetByConditionAsync`).
- Validate both happy paths and exception flows, ensuring business rules run before touching the database (`HasAnyAsync`) and afterward (`InsertAndSaveAsync`, `UpdateAsync`, etc.).
- Prefer `Shouldly` for readable asserts (`response.Entity.ShouldBe(expectedCountry)`), keeping consistency and clear messages.

Run all tests with:
```bash
dotnet test AppProject.slnx
```
When creating new modules, replicate the structure in `AppProject.Core.Tests.<Module>` and `AppProject.Web.Tests.<Module>` (or keep shared projects with named subfolders) to cover business rules and queries.

## Preparing for production
- Fill every placeholder in `appsettings.json` and `wwwroot/appsettings.json` with real values (production connections, Auth0, SendGrid, GitHub Models, public URLs, etc.).
- Set `ASPNETCORE_ENVIRONMENT=Production` for the API and `DOTNET_ENVIRONMENT=Production` for the published frontend.
- Update `Cors:AllowedOrigins` and `AllowedHosts` with the official domains.
- Register the new URLs in Auth0 (callback, logout, and web origins) and generate a `ClientSecret` if needed.
- Ensure the production database is created and the migrations are applied (`dotnet ef database update` or automatic migration at startup).
- Generate a dedicated SendGrid key and validate the domain/sender used by the product.
- Generate a GitHub token for production and keep only the necessary endpoint in `AI:Endpoint`.
- Adjust `SystemAdminUser` to an email that is actually monitored by the operations team.
- Review the logging configuration (`Serilog`) and consider sending logs to a persistent sink in production.
- Confirm whether Hangfire uses a separate database or a suitable connection string for the environment.
- Remove sample data and validate user permissions before go-live.
- Use environment variables or Azure App Configuration/Secrets Manager to store sensitive credentials, avoiding repository exposure.
- Run `dotnet publish -c Release src/AppProject.Core.API/AppProject.Core.API.csproj` and `dotnet publish -c Release src/AppProject.Web/AppProject.Web.csproj` to generate the artifacts that will be deployed to production environments.
- Configure pipelines (GitHub Actions, Azure DevOps, etc.) to run `dotnet test` and publish the projects automatically, ensuring migrations and configurations are applied before deployment.

# dotnet-template-pt
Template para criar projetos em .NET

## Sumário
- [Guia rápido de uso do template](#guia-rápido-de-uso-do-template)
- [Estrutura de projetos](#estrutura-de-projetos)
- [Especificações do projeto](#especificações-do-projeto)
- [Integrações externas](#integrações-externas)
- [Exemplo de CRUD](#exemplo-de-crud)
  - [Backend](#backend)
  - [Frontend](#frontend)
  - [Testes](#testes)
- [Preparando para produção](#preparando-para-produção)

## Guia rápido de uso do template

### Pré-requisitos
- .NET SDK (o `TargetFramework` já está definido em `Directory.Build.props`).
- Visual Studio ou Visual Studio Code com a extensão C# para trabalhar com a solução.
- SQL Server. É possível subir um container local com o comando:
  ```bash
  docker run --name appproject-sqlserver -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- Contas ativas no Auth0, SendGrid e GitHub (com acesso aos GitHub Models) para preencher as integrações descritas a seguir.
- Opcional: instale a ferramenta global `dotnet-ef` para rodar comandos de migração (`dotnet tool install --global dotnet-ef`).
- Importante: não permita que Copilot, Codex ou outro gerador crie automaticamente os arquivos de snapshot ou migration do EF Core. Execute manualmente o script/comando de migração (por exemplo, `dotnet ef migrations add ...`) para garantir que o código seja gerado corretamente.

### Passo a passo para configurar o ambiente
1. Clone o repositório e restaure as dependências com `dotnet restore AppProject.slnx`.
2. Verifique a instalação do .NET com `dotnet --info` e confirme que o .NET está instalado.
3. Garanta que o arquivo `src/AppProject.Core.API/appsettings.Development.json` esteja configurado para apontar para seus recursos locais (ex.: connection string `Server=localhost,1433;Database=AppProject;...`) antes de subir a API. Os demais valores (Auth0, SendGrid, GitHub Models etc.) também tem placeholders para você preencher.
4. Configure o SQL Server local (ou container) e valide a conexão com `sqlcmd` ou ferramenta de sua preferência.
5. Preencha os placeholders de `src/AppProject.Core.API/appsettings.json` e `src/AppProject.Web/wwwroot/appsettings.json` antes de gerar builds para produção. Esses arquivos contêm marcadores `<<SET_...>>` que sinalizam o que precisa ser configurado.
6. Configure as integrações externas (Auth0, SendGrid e GitHub Models) seguindo as instruções detalhadas mais adiante e copie os valores gerados para os arquivos de configuração.
7. Execute a API com `dotnet run --project src/AppProject.Core.API` (porta padrão `https://localhost:7121`).
8. Execute o frontend com `dotnet run --project src/AppProject.Web` (porta padrão `https://localhost:7035`).
9. Acesse `https://localhost:7035` no navegador para utilizar a aplicação e `https://localhost:7121/swagger` para testar os endpoints.

### Checklist de arquivos de configuração
- `src/AppProject.Core.API/appsettings.json` — arquivo base usado em produção. Preencha os placeholders:
  - `<<SET_SQLSERVER_DATABASE_CONNECTION_STRING>>` e `<<SET_HANGFIRE_SQLSERVER_CONNECTION_STRING>>`: strings de conexão (podem ser iguais).
  - `<<SET_AUTH0_AUTHORITY>>`, `<<SET_AUTH0_CLIENT_ID>>`, `<<SET_AUTH0_AUDIENCE>>`: dados do aplicativo Auth0.
  - `<<SET_SYSTEM_ADMIN_NAME>>`, `<<SET_SYSTEM_ADMIN_EMAIL>>`: usuário administrador que será criado automaticamente.
  - `<<SET_ALLOWED_CORS_ORIGINS>>`: URLs autorizadas a consumir a API.
  - `<<SET_ALLOWED_HOSTS>>`: hosts aceitos quando a aplicação estiver em produção.
  - `<<SET_SENDGRID_API_KEY>>`, `<<SET_SENDGRID_FROM_EMAIL>>`, `<<SET_SENDGRID_FROM_NAME>>`: credenciais de envio de e-mail.
  - `<<SET_GITHUB_AI_ENDPOINT>>`, `<<SET_GITHUB_AI_TOKEN>>`: configurações da integração com GitHub Models.
- `src/AppProject.Core.API/appsettings.Development.json` — já aponta para conexões locais (`Server=localhost,1433;...`) e mantém placeholders para credenciais sensíveis (Auth0, SendGrid, GitHub Models). Ajuste conforme o seu ambiente e evite versionar dados sigilosos.
- `src/AppProject.Web/wwwroot/appsettings.json` — placeholders do frontend (`Auth0` e `Api:BaseUrl`). O arquivo publicado deve apontar para as URLs de produção.
- `src/AppProject.Web/wwwroot/appsettings.Development.json` — traz `Api:BaseUrl` apontando para `https://localhost:7121` e mantém placeholders para Auth0.
- `src/AppProject.Web/Constants/AppProjectConstants.cs` — atualize a constante `ProjectName` com o nome que você deseja usar para o seu aplicativo/projeto.
- `src/AppProject.Web/Constants/ThemeConstants.cs` — mantém as chaves de armazenamento de tema alinhadas ao nome do projeto.
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — ao criar novos módulos, registre os assemblies nos métodos `GetControllerAssemblies()` e `GetServiceAssemblies()`.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — inclua os assemblies de clientes Refit em `GetApiClientAssemblies()` e valide o `Api:BaseUrl`.
- `src/AppProject.Web/App.razor` — registre assemblies adicionais no método `OnNavigateAsync` para habilitar o lazy loading de novos módulos.
- `src/AppProject.Web/AppProject.Web.csproj` — adicione novos `ProjectReference` e entradas `BlazorWebAssemblyLazyLoad` quando criar módulos adicionais.
- `src/AppProject.Web/Layout/NavMenu.razor` — inclua itens de menu e permissões para os novos módulos.
- `src/AppProject.Resources/Resource*.resx` — mantenha as traduções sincronizadas ao adicionar novos textos. Preserve os comentários que nomeiam cada grupo/logical key, continue quebrando as entradas por formulário (validações e menu permanecem agrupadas) e, quando precisar reservar placeholders futuros, use `{{}}` em vez de `{}` para que o parser mantenha o texto literal.

## Estrutura de projetos
- **Backend**
  - `src/AppProject.Core.API`: API ASP.NET Core com autenticação, middleware de exceção, configuração de CORS, Rate Limiting e bootstrap de serviços.
  - `src/AppProject.Core.Controllers.<Módulo>` (ex.: `AppProject.Core.Controllers.General`): controllers REST de cada módulo.
  - `src/AppProject.Core.Services.<Módulo>` (ex.: `AppProject.Core.Services.General`): serviços transacionais (CRUD) com regras de negócio específicas.
  - `src/AppProject.Core.Services/<Módulo>` (ex.: `AppProject.Core.Services/General`): serviços de leitura e summaries compartilhados.
  - `src/AppProject.Core.Models` / `src/AppProject.Core.Models.<Módulo>`: DTOs e objetos de request. Use a pasta comum para artefatos compartilhados e a pasta com o nome do módulo para itens específicos.
  - `src/AppProject.Core.Infrastructure.Database`: contexto EF Core, repositório genérico, entidades e `EntityTypeConfiguration`.
  - `src/AppProject.Core.Infrastructure.Email`: abstração de envio de e-mails via SendGrid.
  - `src/AppProject.Core.Infrastructure.AI`: integração com GitHub Models para cenários de IA.
- **Frontend**
  - `src/AppProject.Web`: host Blazor WebAssembly, autenticação OIDC, layout, navegação e bootstrap.
  - `src/AppProject.Web.<Módulo>` (ex.: `AppProject.Web.General`): páginas e componentes específicos do módulo carregados via lazy loading.
  - `src/AppProject.Web.ApiClient` / `src/AppProject.Web.ApiClient.<Módulo>`: interfaces Refit para consumo da API (separe clientes compartilhados dos específicos de módulo).
  - `src/AppProject.Web.Models` / `src/AppProject.Web.Models.<Módulo>`: modelos observáveis usados nos formulários.
  - `src/AppProject.Web.Framework`: componentes e páginas base (SearchControl, DataGridControl, ModelFormPage etc.).
  - `src/AppProject.Web.Shared`: componentes compartilhados por múltiplos módulos (ex.: dropdowns com grid).
- **Testes**
  - `src/AppProject.Core.Tests.<Módulo>` (ex.: `AppProject.Core.Tests.General`): testes unitários das services do backend utilizando NUnit, Moq, Shouldly e Bogus. Crie projetos adicionais conforme novos módulos ou mantenha cenários compartilhados em projetos sem sufixo de módulo.
  - `src/AppProject.Web.Tests.<Módulo>` (ex.: `AppProject.Web.Tests.General`): ponto de partida para testes do frontend; adapte para novos módulos ou use projetos compartilhados quando fizer sentido.

## Especificações do projeto
Seguem algumas especificações do projeto.
* Usamos o idioma Inglês nos códigos e nos nomes de arquivos.
* O template já suporta localização (`en-US`, `pt-BR` e `es-ES`) tanto na API quanto no frontend.
* O frontend usa Radzen para os componentes UI, Refit para os clientes HTTP e autenticação OIDC com Auth0.
* O estilo de código é validado com StyleCop (veja `Stylecop.json`) e com as configurações compartilhadas em `Directory.Build.props`; rode o analisador localmente e mantenha os `using` apenas quando necessários e ordenados para evitar violações (evitando assim usings não utilizados).
* Os projetos backend e frontend executam com o `TargetFramework` e utilizam `implicit usings` e `nullable` habilitados.

## Integrações externas
As seções abaixo descrevem os cadastros necessários para que todas as integrações funcionem. Após concluir cada etapa, copie os valores para os arquivos `appsettings`.

### Auth0
1. Crie uma aplicação do tipo **Single Page Application**.
2. Configure o logo da aplicação em *Settings* se desejar.
3. Preencha as URLs (ajuste as portas caso altere o `launchSettings.json`):
   - **Allowed Callback URLs**: `https://localhost:7035/authentication/login-callback`, `https://localhost:7121/swagger/oauth2-redirect.html`
   - **Allowed Logout URLs**: `https://localhost:7035`, `https://localhost:7121/swagger/`
   - **Allowed Web Origins**: `https://localhost:7035`, `https://localhost:7121`
4. Crie uma **API** no Auth0 e use como *Identifier* o mesmo valor configurado em `Auth0:Audience` (`https://appproject.api` por padrão). Em seguida, acesse **Access Settings** e marque a opção **Allow Offline Access**.
5. Para incluir `email`, `name` e `roles` no JWT, crie uma Action do tipo `post_login` com o script abaixo:
   ```javascript
   if (api.accessToken) {
       if (event.user && event.user.email) {
         api.accessToken.setCustomClaim("email", event.user.email);
       }

       if (event.user && event.user.name) {
         api.accessToken.setCustomClaim("name", event.user.name);
       }

       if (event.authorization && event.authorization.roles) {
         api.accessToken.setCustomClaim("roles", event.authorization.roles);
       }
     }
   ```
6. Copie `Authority` e `ClientId` da aplicação Single Page Application que você criou e copie o `Audience` da API para os `appsettings`, mantendo o prefixo `https://` tanto para o `Authority` quanto para o `Audience`. Por exemplo:
   ```json
   "Authority": "https://seuauth0domain.us.auth0.com",
   "ClientId": "seuclientid",
   "Audience": "https://seuaudience.com"
   ```
> Observação: Ao subir o Swagger, limpe o cache do navegador para evitar que ele reutilize parâmetros de outros projetos.

### SendGrid
1. Crie uma conta no [site do SendGrid](https://sendgrid.com/).
2. Configure uma identidade (domain authentication ou single sender). Autorize a identidade através do e-mail recebido.
3. No menu **Email API > Integration Guide**, gere uma API Key.
4. Envie o primeiro e-mail de teste e confirme o envio no painel.
5. Copie a key e o remetente configurado (`SendEmail:ApiKey`, `SendEmail:FromEmailAddress`, `SendEmail:FromName`).
6. Quando criar novos e-mails, modele o corpo em um template Razor (`SampleEmailTemplate.cshtml` e `SampleEmailModel` servem de exemplo em `AppProject.Core.Infrastructure.Email`) e apenas preencha o modelo na service, evitando strings inline.

### GitHub AI Models
1. Siga a documentação oficial: <https://docs.github.com/en/github-models/use-github-models/prototyping-with-ai-models>.
2. Gere um token com permissão para usar os modelos hospedados pelo GitHub em [https://github.com/settings/tokens](https://github.com/settings/tokens).
3. Preencha `AI:Endpoint` (padrão `https://models.github.ai/inference`) e `AI:Token` com o token gerado.

### Usuário administrador
Ao subir a API pela primeira vez, o bootstrap cria ou atualiza o usuário administrador definido em `SystemAdminUser`. Utilize esse usuário para garantir que existe ao menos uma conta com permissão para acessar todos os cadastros.

## Exemplo de CRUD
O exemplo abaixo mostra, passo a passo, como o módulo General implementa os cadastros de País, Estado, Cidade e Bairros. Siga as mesmas etapas quando criar novos módulos.

### Backend

#### 1. Identifique qual o módulo
Primeiramente, identifique qual o módulo que você deseja colocar a sua nova entidade. Por exemplo, imagine os cadastros de estados, cidades e países. Esses cadastros são de um módulo Geral. Por isso, usaremos as pastas General sempre que possível dentro do projeto.

##### Se for necessário criar um novo módulo
Caso seja identificado que precisa criar um novo módulo, use o módulo **General** como referência e crie/ajuste os itens abaixo (substitua `NovoModulo` pelo nome desejado):
- **Backend**
  - Projeto `AppProject.Core.Models.NovoModulo` com os DTOs do módulo.
  - Projeto `AppProject.Core.Services.NovoModulo` contendo as services de CRUD.
  - Pasta `NovoModulo` dentro de `AppProject.Core.Services` para os serviços de summary.
  - Projeto `AppProject.Core.Controllers.NovoModulo`.
  - Pastas `Entities/NovoModulo` e `EntityTypeConfiguration/NovoModulo` dentro de `AppProject.Core.Infrastructure.Database` para as entidades e configurações EF Core.
  - Migrações específicas no projeto `AppProject.Core.Infrastructure.Database`.
- **Frontend**
  - Projeto `AppProject.Web.NovoModulo` com as páginas e componentes Blazor.
  - Projeto `AppProject.Web.ApiClient.NovoModulo` com as interfaces Refit de CRUD.
  - Pasta `NovoModulo` dentro de `AppProject.Web.ApiClient` para as interfaces Refit de CRUD que sejam compartilhadas entre outros módulos.
  - Projeto `AppProject.Web.Models.NovoModulo` com os modelos observáveis do client.
  - Pasta `NovoModulo` dentro de `AppProject.Web.Models` para os modelos observáveis do client compartilhados entre outros módulos.
  - Pasta `NovoModulo` em `AppProject.Web.Shared` para componentes compartilháveis (dropdowns, cards etc.).
- **Testes**
  - Projeto `AppProject.Core.Tests.NovoModulo` cobrindo as services do backend.
  - Projeto `AppProject.Web.Tests.NovoModulo` (opcional) para cenários de UI/integração.

Também será preciso editar os seguintes arquivos para registrar o assembly do novo módulo:
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — inclua o assembly nos métodos `GetControllerAssemblies()` e `GetServiceAssemblies()`.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — registre o assembly em `GetApiClientAssemblies()` e ajuste o carregamento lazy se necessário.
- `src/AppProject.Web/App.razor` — adicione o assembly às condições de `OnNavigateAsync` para habilitar o lazy loading.
- `src/AppProject.Web/AppProject.Web.csproj` — crie os `ProjectReference` e entradas `BlazorWebAssemblyLazyLoad`.
- `src/AppProject.Web/Layout/NavMenu.razor` — inclua o item de menu e as permissões relacionadas.
- `src/AppProject.Resources/Resource*.resx` — adicione as novas chaves de tradução, seguindo a mesma estrutura existente, mantendo os comentários e replicando as tabulações e características das existentes.

Antes de adicionar um `ProjectReference`, valide se a dependência já está acessível via assemblies compartilhados (por exemplo, `AppProject.Core.Services` já referencia os projetos de Jobs, portanto um novo módulo não precisa referenciar Jobs diretamente). Isso evita ciclos e compilações desnecessárias.

#### 2. Conteúdos compartilhados entre módulos
Caso você esteja adicionando arquivos que são compartilhados entre os módulos, será necessário colocar esses arquivos no projeto raiz em vez do projeto que leva o nome do módulo. Por exemplo, imagine que você esteja adicionando a tabela Customer. Customer é uma tabela que pode ser usada em vários módulos (invoice, financial, etc.). Nesse caso, em vez de ter um módulo General ou Customer, o ideal seria colocar no projeto raiz, dentro de uma pasta que leva o nome do módulo.

Veja a seguir uma lista dos projetos raíz onde podemos criar pastas que representam parte dos módulos que serão compartilhados:
* `AppProject.Core.Models` — DTOs e requests compartilhados. Analise sempre se os arquivos deverão ser compartilhados entre outros módulos ou não. A preferência é que os arquivos fiquem no projeto do próprio módulo ao invés de ficarem nos compartilhados.
* `AppProject.Core.Services` — serviços comuns, como por exemplo, summaries visíveis em vários módulos. No entanto, não significa que qualquer summary deva ser colocado aqui. Apenas summaries que serão compartilhados entre os módulos.
* `AppProject.Web` — componentes de layout, autenticação, bootstrap e navegação.
* `AppProject.Web.ApiClient` — interfaces Refit reutilizadas em mais de um módulo.
* `AppProject.Web.Models` — modelos observáveis usados por múltiplos módulos.
* `AppProject.Web.Shared` — componentes Blazor genéricos (dropdowns, cards, controles auxiliares).
* `AppProject.Resources` — traduções reutilizadas em diferentes áreas.

#### 3. Adicionando os DTOs na API
No projeto, há os DTOs do lado da API e os DTOs do lado da WEB (ou client). Eles são diferentes, porque do lado da WEB os DTOs podem ter notificações de mudanças (INotifyPropertyChanged), enquanto que no lado da API não tem.

Os DTOs são adicionados nos projetos:
* AppProject.Core.Models (para DTOs compartilhados entre os módulos; crie pastas internas com o nome do módulo quando necessário — por exemplo, `AppProject.Core.Models/General`).
* AppProject.Core.Models.<Módulo> (ex.: `AppProject.Core.Models.General`) para DTOs específicos de um módulo.
* Crie projetos adicionais como `AppProject.Core.Models.<NovoModulo>` se precisar separar DTOs por módulo.

Normalmente num CRUD, teremos dois tipos de DTOs: os do tipo Entidade (herdando de IEntity) e os do tipo Summary (herdando de ISummary). 

##### DTOs de entidades
Os DTOs de entidade representam os campos das tabelas e devem herdar de `IEntity`. Eles precisam expor `RowVersion` para suportar concorrência otimista e utilizar DataAnnotations para validar obrigatoriedade, tamanho máximo e regras básicas. Referências:
- `AppProject.Core.Models.General/Country.cs` — entidade simples com `Name`, `Code` e `RowVersion`.
- `AppProject.Core.Models.General/State.cs` — adiciona `CountryId` validado por `RequiredGuid`.
- `AppProject.Core.Models.General/City.cs` — além dos campos principais, mantém as coleções `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` para sincronizar bairros.
- `AppProject.Core.Models.General/Neighborhood.cs` — estrutura básica de bairros, utilizada tanto na API quanto no frontend.

###### Validações de entidades
As exceções de validação retornam `ExceptionDetail` com o código `RequestValidation`. Todos os atributos personalizados residem em `AppProject.Models.CustomValidators`:
- `ValidateCollectionAttribute` garante a validação em cascata de listas.
- `RequiredGuidAttribute` impede o envio de GUIDs vazios.

O middleware [`AppProject.Core.API/Middlewares/ExceptionMiddleware.cs`](./src/AppProject.Core.API/Middlewares/ExceptionMiddleware.cs) converte exceções em respostas padronizadas, enquanto `Bootstrap.ConfigureValidations` força o lançamento de `AppException` quando o `ModelState` é inválido.

Para relacionamentos, mantenha apenas os identificadores (ex.: `StateId` em `City`). Ao manipular entidades agregadas, como bairros da cidade, utilize as coleções de `CreateOrUpdateRequest`/`DeleteRequest` do DTO pai (`City.cs`) para indicar inserções, atualizações e exclusões.

##### DTOs de summaries
Use DTOs de summary para alimentar grids, combos e demais consultas de leitura. Eles herdam de `ISummary`, não possuem DataAnnotations e contêm apenas os campos necessários para exibir informações na interface:
- `AppProject.Core.Models/General/CountrySummary.cs` — exemplo do módulo General que mantém `Id` e `Name`.
- `AppProject.Core.Models/General/StateSummary.cs` — exemplo do módulo General com `CountryName` e `CountryId`.
- `AppProject.Core.Models/General/CitySummary.cs` — exemplo do módulo General com `StateName`, `StateId`, `CountryName` e `CountryId`.

Para pesquisas avançadas, utilize `SearchRequest` como classe base e adicione propriedades específicas:
- `AppProject.Core.Models/General/StateSummarySearchRequest.cs` — permite filtrar estados por `CountryId`.
- `AppProject.Core.Models/General/CitySummarySearchRequest.cs` — filtra cidades por `StateId`.
Mas, evite criar `SearchRequest` derivadas vazias. Só crie uma classe derivada de SearchRequest caso tenha filtros adicionais. Por exemplo, não temos uma CountrySummarySearchRequest, porque na pesquisa de countries não adicionamos filtros adicionais igual fizemos com o StateSummarySearchRequest e o CitySummarySearchRequest.

Sempre avalie se o summary deve trazer nomes agregados em vez de objetos completos. Isso facilita a serialização e evita cargas desnecessárias ou ciclos de referência.

#### 4. Adicionando as entidades de banco
As entidades de banco residem em `AppProject.Core.Infrastructure.Database/Entities` e seguem o padrão `Tb[Nome]`. Recomendações:
- `TbCountry.cs`, `TbState.cs`, `TbCity.cs`, `TbNeighborhood.cs` — mantêm DataAnnotations para chaves, tamanho de colunas e relacionamentos. Utilize nomes de tabela no plural (`[Table("Countries")]`) e configure coleções de navegação (`States`, `Cities`, `Neighborhoods`) para facilitar o carregamento.
- Armazene apenas informações necessárias para persistência; qualquer lógica adicional deve ficar nas services.
- Aplique `MaxLength` em colunas de texto e mantenha valores plausíveis para os campos compartilhados com os DTOs.

Os relacionamentos são modelados com campos de chave estrangeira (ex.: `CountryId` em `TbState`) e propriedades de navegação com `[ForeignKey]`. Ao criar entidades adicionais, siga o mesmo padrão para garantir que o EF Core configure as constraints automaticamente.

##### Adicionando os arquivos EntityTypeConfiguration
Classes de configuração complementam as entidades com índices, restrições adicionais e regras específicas do EF Core. Elas ficam em `AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/[Modulo]` e seguem o padrão `Tb[Nome]Configuration`. Exemplos:
- `TbCountryConfiguration.cs` — define índice único para `Name`.
- `TbStateConfiguration.cs` — cria índice para facilitar buscas por `Name`.
- `TbCityConfiguration.cs` e `TbNeighborhoodConfiguration.cs` — configuram índices para as entidades dependentes.

Todas herdam de `IEntityTypeConfiguration<T>` e são carregadas automaticamente por `ApplicationDbContext`. Caso precise adicionar novas constraints (por exemplo, índices compostos), implemente-as nesses arquivos em vez de inflar as entidades com lógica adicional.

Importante: ao adicionar um novo arquivo herdando de `IEntityTypeConfiguration`, não é necessário registrar manualmente no `ApplicationDbContext`; o método `OnModelCreating` já percorre o assembly e aplica cada configuração.

##### Adicionando DbSet no ApplicationDbContext
Atualize [`ApplicationDbContext`](./src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs) sempre que criar uma nova entidade. Cada tabela deve possuir um `DbSet<T>` com nome no plural (por exemplo, `Countries`, `States`, `Cities`, `Neighborhoods`). Manter essa convenção facilita a leitura e evita divergências entre EF Core e o banco.

##### Rodando migrations
Para que possamos criar os scripts do banco, nós precisamos rodar o migration do Entity Framework. Para isso, abra o terminal na pasta src do projeto e execute o comando a seguir:

```bash
dotnet ef migrations add MigrationName --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

No lugar de MigrationName, dê um nome que identifique o seu migration, como talvez o nome de uma das tabelas que você está modificando ou algum assunto que remeta à alteração.

Esse comando fará com que, na pasta Migrations do projeto AppProject.Core.Infrastructure.Database, contenha os arquivos de script de migração.

Importante: não precisa aplicar o migration, pois ele já é aplicado automaticamente quando a API sobe.

#### 5. Adicionando as classes de configuração do Mapster
Por padrão, o Mapster consegue mapear entidades para DTOs quando os nomes das propriedades coincidem. Crie configurações adicionais apenas quando precisar transformar campos ou incluir dados de relacionamentos. No módulo General utilizamos:
- `AppProject.Core.Infrastructure.Database/Mapper/General/StateSummaryMapsterConfig.cs` — injeta `CountryName` ao mapear `TbState` → `StateSummary`.
- `AppProject.Core.Infrastructure.Database/Mapper/General/CitySummaryMapsterConfig.cs` — expõe `StateName` e `CountryName` a partir das entidades relacionadas.

Essas classes implementam `IRegisterMapsterConfig` e são carregadas no bootstrap (`Bootstrap.ConfigureMapper`). Ao adicionar novas configurações:
1. Crie o arquivo `[Dto]MapsterConfig.cs` dentro da pasta `Mapper/[Modulo]`.
2. Configure o `TypeAdapterConfig` no método `Register`.
3. Evite lógica complexa no mapper; utilize as services para manipular regras de negócio.

As services centralizam regras de negócio, validações e orquestração do repositório. Elas residem em `AppProject.Core.Services` para itens compartilhados e em `AppProject.Core.Services.<Módulo>` (por exemplo, `AppProject.Core.Services.General`) para implementações específicas. Todos os tipos compatíveis são registrados automaticamente na DI por `Bootstrap.ConfigureServices`.

##### Interface da classe de serviço
- `ICountryService.cs`, `IStateService.cs`, `ICityService.cs` implementam `ITransientService` e os contratos genéricos `IGetEntity`, `IPostEntity`, `IPutEntity` e `IDeleteEntity`. Isso padroniza as assinaturas de CRUD e mantém a API consistente.
- `ICityService` adiciona `GetNeighborhoodEntitiesAsync`, que retorna bairros associados a uma cidade usando `GetByParentIdRequest<Guid>`.
- As interfaces de summary (`ICountrySummaryService.cs`, `IStateSummaryService.cs`, `ICitySummaryService.cs`) expõem `IGetSummaries`/`IGetSummary` com seus respectivos requests (`SearchRequest`, `StateSummarySearchRequest`, `CitySummarySearchRequest`). Utilize essa abordagem ao criar novos summaries.

##### Classes de serviço
- `CountryService.cs`, `StateService.cs` e `CityService.cs` são responsáveis por:
  1. Validar permissões com `IPermissionService.ValidateCurrentUserPermissionAsync` usando `PermissionType.System_ManageSettings`.
  2. Executar validações de negócio (`ValidateCountryAsync`, `ValidateStateAsync`, `ValidateCityAsync`) para evitar duplicidades e inconsistências.
  3. Usar `IDatabaseRepository` para consultar (`GetFirstOrDefaultAsync`, `GetByConditionAsync`), inserir (`InsertAndSaveAsync`), atualizar (`UpdateAndSaveAsync`) ou excluir (`DeleteAndSaveAsync`) registros.
  4. Mapear DTOs ↔ entidades via `Mapster` (`Adapt`).
- `CityService` merece destaque porque manipula agregados:
  - Persistência dos bairros ocorre por meio das listas `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` vindas do DTO de cidade.
  - Os métodos `ValidateCityAsync` e `ValidateNeighborhoodsBelongToCityAsync` evitam nomes duplicados e garantem que os bairros realmente pertençam à cidade em edição.
  - Inserções múltiplas utilizam `InsertAsync`/`UpdateAsync` seguida de `SaveAsync` para garantir atomicidade.
- Todas as services lançam `AppException` com `ExceptionCode` apropriado (`EntityNotFound`, `General_*_DuplicateName`, etc.), assegurando mensagens traduzidas via resources.

##### Classes de serviços de summaries
- `CountrySummaryService.cs`, `StateSummaryService.cs` e `CitySummaryService.cs` tratam consultas de leitura. Eles chamam `GetByConditionAsync` com filtros (`SearchText`, `Take`, `CountryId`, `StateId`) e utilizam `SummariesResponse<T>` para devolver coleções imutáveis.
- Quando `GetSummaryAsync` não encontra o registro, as classes lançam `AppException(ExceptionCode.EntityNotFound)` para manter consistência com as services de escrita.
- Ao expandir o template, siga este padrão: mantenha serviços de leitura livres de validações de permissão custosas (a não ser que haja requisitos específicos) e centralize filtros em objetos de request para reutilização no frontend.
- Evite `Include` redundantes nas queries de summary quando as colunas já são projetadas pela view/adapter.

#### 7. Criando as classes de controller
Os controllers ficam em projetos específicos de cada módulo, como `AppProject.Core.Controllers.<Módulo>` (ex.: `AppProject.Core.Controllers.General`). Eles expõem apenas a lógica necessária para receber as requests, chamar as services e retornar o resultado padronizado (`Ok(...)`). Exemplos:
- `CountryController.cs`, `StateController.cs`, `CityController.cs` — implementam endpoints de CRUD para cada entidade.
- `CityController` também oferece `GetNeighborhoodsAsync` para consultar os bairros relacionados.
- `CountrySummaryController.cs`, `StateSummaryController.cs`, `CitySummaryController.cs` — expõem endpoints de consulta (`GetSummariesAsync`, `GetSummaryAsync`).

Diretrizes gerais:
- Aplique `[Authorize]` para proteger os endpoints e `[ApiController]` para habilitar validação automática de modelo.
- Utilize o padrão de rota `api/<módulo>/[controller]/[action]` (por exemplo, `api/general/Country/Post`).
- Receba parâmetros usando `[FromQuery]` para buscas (`GetByIdRequest`, `DeleteRequest`) e `[FromBody]` para mutações (`CreateOrUpdateRequest`).
- Retorne sempre `IActionResult` com `Ok(...)` para manter consistência e facilitar tratamento global de erros.

### Frontend
O frontend é um aplicativo **Blazor WebAssembly** que consome a API via Refit e utiliza os componentes do Radzen. A implementação do módulo General serve como guia para novos módulos.

#### Visão geral e arquivos principais
- Os modelos client-side ficam em `AppProject.Web.Models` e `AppProject.Web.Models.<Módulo>` (ex.: `AppProject.Web.Models.General`). Todos herdam de [`ObservableModel`](./src/AppProject.Web.Models/ObservableModel.cs), que implementa `INotifyPropertyChanged` para atualizar a UI automaticamente.
- As classes de entidade, como [`Country.cs`](./src/AppProject.Web.Models.General/Country.cs), [`State.cs`](./src/AppProject.Web.Models.General/State.cs), [`City.cs`](./src/AppProject.Web.Models.General/City.cs) e [`Neighborhood.cs`](./src/AppProject.Web.Models.General/Neighborhood.cs), espelham os DTOs da API. No caso de `City`, mantemos as coleções `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` para enviar alterações aninhadas.
- Os summaries utilizados em grids e combos ficam em pastas como [`AppProject.Web.Models/<Módulo>`](./src/AppProject.Web.Models/General). Exemplos para o módulo General: [`CountrySummary.cs`](./src/AppProject.Web.Models/General/CountrySummary.cs), [`StateSummary.cs`](./src/AppProject.Web.Models/General/StateSummary.cs) e [`CitySummary.cs`](./src/AppProject.Web.Models/General/CitySummary.cs).

#### Clientes HTTP com Refit
- As interfaces de CRUD residem em projetos como [`AppProject.Web.ApiClient.<Módulo>`](./src/AppProject.Web.ApiClient.General). Exemplo: [`ICityClient.cs`](./src/AppProject.Web.ApiClient.General/ICityClient.cs) cobre todos os endpoints de `CityController` no módulo General.
- Os summaries possuem clientes separados em pastas como [`AppProject.Web.ApiClient/<Módulo>`](./src/AppProject.Web.ApiClient/General). Veja os arquivos do módulo General, como [`ICitySummaryClient.cs`](./src/AppProject.Web.ApiClient/General/ICitySummaryClient.cs) e [`ICountrySummaryClient.cs`](./src/AppProject.Web.ApiClient/General/ICountrySummaryClient.cs).
- O bootstrap [`WebBootstrap.cs`](./src/AppProject.Web/Bootstraps/WebBootstrap.cs) registra dinamicamente todas as interfaces Refit definidas nos assemblies listados por `GetApiClientAssemblies()`. Ao adicionar um módulo novo, inclua o assembly correspondente.

#### Páginas de pesquisa (Search)
- As páginas de pesquisa herdam de [`SearchPage<TRequest,TSummary>`](./src/AppProject.Web.Framework/Pages/SearchPage.cs), que encapsula a execução de buscas, a seleção de itens e a exibição de alertas quando o limite `Take` é atingido.
- O componente [`SearchControl.razor`](./src/AppProject.Web.Framework/Components/SearchControl.razor) disponibiliza formulário padrão com campo de texto, filtros avançados e alerta configurável.
- Exemplos:
  - [`CountrySummaryPage.razor`](./src/AppProject.Web.General/Pages/CountrySummaryPage.razor) mostra o grid com países e operações `New`, `Edit` e `Delete`.
- [`StateSummaryPage.razor`](./src/AppProject.Web.General/Pages/StateSummaryPage.razor) adiciona filtro por país via `CountrySummaryDropDownDataGridControl`.
- [`CitySummaryPage.razor`](./src/AppProject.Web.General/Pages/CitySummaryPage.razor) filtra por estado e exibe colunas adicionais.
- O grid padrão é [`DataGridControl.razor`](./src/AppProject.Web.Framework/Components/DataGridControl.razor), que aceita `GlobalActions`, `ContextActions` e se integra com seleção múltipla.

#### Páginas de formulário e itens aninhados
- Formulários herdam de [`ModelFormPage<TModel>`](./src/AppProject.Web.Framework/Pages/ModelFormPage.cs) e utilizam o componente [`ModelFormControl.razor`](./src/AppProject.Web.Framework/Components/ModelFormControl.razor).
- Reutilize diretamente as propriedades do modelo (`TModel`) para binds e seleções em vez de criar propriedades duplicadas (por exemplo, evite `SelectedId`/`SelectedAnotherField`). 
- Componentes DropDowns devem herdar do `DropDownDataGridControl`. Ao usar esses componentes atente-se para que o `TValue` fique correto (`Guid` para campos obrigatórios no DTO e `Guid?` se o campo não for obrigatório).
- `ModelFormControl` permite customizações importantes:
  - `ShowNewAction`, `ShowEditAction` e `ShowDeleteAction` em `DataGridControl` controlam quais botões são exibidos.
  - `PreferAddOverNew` troca o texto padrão do botão para “Add” (caso de [`CityFormPage.razor`](./src/AppProject.Web.General/Pages/CityFormPage.razor) ao gerenciar bairros).
  - `PreferOpenOverEdit` exibe “Open” no lugar de “Edit”, útil para telas somente leitura.
  - `PreferExecuteOverSave` renomeia o botão principal para “Executar”, adequado para telas de processamento.
  - `PreferCloseOverCancel` aplica o estilo de fechamento ao botão secundário.
- Para relacionamentos aninhados, siga o exemplo de `CityFormPage`:
  - Carregue registros filhos via cliente dedicado (`ICityClient.GetNeighborhoodsAsync`).
  - Ao inserir/alterar um item, adicione-o em `ChangedNeighborhoodRequests`.
  - Ao excluir, mova o identificador para `DeletedNeighborhoodRequests`.
  - Use diálogos especializados (como [`NeighborhoodFormPage.razor`](./src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor)) para editar os itens filhos.
- Consulte sempre a documentação dos componentes Radzen antes de configurar atributos como `Min`/`Max`. Para valores decimais use expressões `@(0.01m)` em `RadzenNumeric`, garantindo que o valor seja tratado como `decimal`, e evite validadores redundantes quando a propriedade já possui `[Required]` no modelo.

#### Fluxo completo dos cadastros General
1. **País**
   - Pesquisa: [`CountrySummaryPage.razor`](./src/AppProject.Web.General/Pages/CountrySummaryPage.razor) chama `ICountrySummaryClient` e, após confirmar exclusões, usa `ICountryClient.DeleteAsync`.
   - Formulário: [`CountryFormPage.razor`](./src/AppProject.Web.General/Pages/CountryFormPage.razor) abre em `DialogService`, reutilizando o mesmo componente para criar ou editar. As validações visuais usam `RadzenRequiredValidator` e `RadzenLengthValidator`.
2. **Estado**
   - Pesquisa: [`StateSummaryPage.razor`](./src/AppProject.Web.General/Pages/StateSummaryPage.razor) acrescenta filtro de país via [`CountrySummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor).
   - Formulário: [`StateFormPage.razor`](./src/AppProject.Web.General/Pages/StateFormPage.razor) exige a seleção de um país. A validação do `Guid` usa `RadzenCustomValidator` para impedir `Guid.Empty`.
3. **Cidade**
   - Pesquisa: [`CitySummaryPage.razor`](./src/AppProject.Web.General/Pages/CitySummaryPage.razor) combina filtros por estado, ordenação e colunas adicionais (`StateName`, `CountryName`).
   - Formulário: [`CityFormPage.razor`](./src/AppProject.Web.General/Pages/CityFormPage.razor) utiliza outro `DataGridControl` para gerenciar bairros e marca `PreferAddOverNew` para refletir a ação de inserir filhos. As listas `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` são atualizadas sempre que o usuário confirma o diálogo de bairro.
4. **Bairros**
   - Formulário filho: [`NeighborhoodFormPage.razor`](./src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor) herda de `ModelFormPage<Neighborhood>` e retorna o objeto via `CloseDialogAsync`. O componente é usado tanto para criar quanto para editar registros dentro da cidade.

#### Componentes reutilizáveis
- [`DataGridControl.razor`](./src/AppProject.Web.Framework/Components/DataGridControl.razor) encapsula o `RadzenDataGrid` com seleção múltipla, localização e botões configuráveis (`ShowNewAction`, `ShowEditAction`, `ShowDeleteAction`, `PreferAddOverNew`, `PreferOpenOverEdit`).
- [`ModelFormControl.razor`](./src/AppProject.Web.Framework/Components/ModelFormControl.razor) padroniza o cabeçalho de formulários, inclui slots `GlobalActions` e os toggles `PreferExecuteOverSave` / `PreferCloseOverCancel`.
- [`FieldsetControl.razor`](./src/AppProject.Web.Framework/Components/FieldsetControl.razor) gera fieldsets colapsáveis com traduções centralizadas.
- [`SearchControl.razor`](./src/AppProject.Web.Framework/Components/SearchControl.razor) fornece filtros básicos/avançados e alerta automático para o `Take`.
- [`DropDownDataGridControl.cs`](./src/AppProject.Web.Framework/Components/DropDownDataGridControl.cs) ajusta textos do `RadzenDropDownDataGrid` e é reaproveitado em [`CountrySummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor) e [`StateSummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/StateSummaryDropDownDataGridControl.razor), garantindo que o item selecionado seja carregado mesmo após paginação/filtragem.
- [`BusyIndicatorControl.razor`](./src/AppProject.Web.Framework/Components/BusyIndicatorControl.razor) é utilizado por [`AppProjectComponentBase`](./src/AppProject.Web.Framework/Components/AppProjectComponentBase.cs) para exibir diálogos de progresso e tratar exceções (incluindo respostas `ApiException` do Refit).
- Layout e preferências globais:
  - [`LanguageSelector.razor`](./src/AppProject.Web/Layout/LanguageSelector.razor) persiste a cultura selecionada usando `Blazored.LocalStorage`.
  - [`ThemeToggle.razor`](./src/AppProject.Web/Layout/ThemeToggle.razor) alterna entre os temas definidos em [`ThemeConstants`](./src/AppProject.Web/Constants/ThemeConstants.cs).
  - [`Login.razor`](./src/AppProject.Web/Layout/Login.razor) resume o fluxo de autenticação OIDC (login/logout).

#### Localização e resources
- Tanto a API quanto o frontend consomem os resources definidos em `AppProject.Resources`. O helper [`StringResource.cs`](./src/AppProject.Resources/StringResource.cs) lê os arquivos [`Resource.resx`](./src/AppProject.Resources/Resource.resx), [`Resource.pt-BR.resx`](./src/AppProject.Resources/Resource.pt-BR.resx) e [`Resource.es-ES.resx`](./src/AppProject.Resources/Resource.es-ES.resx).
- Ao adicionar novas telas ou mensagens, inclua as chaves nos três arquivos para manter o suporte multilíngue.
- Preserve os comentários e a organização por grupos (formulários separados, validações e menus juntos) ao editar os `.resx`; cada nova key deve estar em sua própria linha seguindo o padrão existente para facilitar o diff.
- Quando precisar reservar placeholders para valores interpolados posteriormente use `{{Value}}` ao invés de `{Value}` para que o parser dos resources trate o texto como literal até que o template faça o replace.
- Os componentes reutilizáveis consomem chaves específicas (`DataGridControl_NewButton_Text`, `DataGridControl_AddButton_Text`, `ModelFormControl_SaveButton_Text`, `ModelFormControl_ExecuteButton_Text`, entre outras). Ajuste essas chaves para personalizar rótulos sem alterar o código.

#### Lazy loading, navegação e bootstrap
- [`AppProject.Web/App.razor`](./src/AppProject.Web/App.razor) carrega assemblies sob demanda. Rotas que começam com `general` carregam `AppProject.Web.General.dll`. Para novos módulos, replique a lógica adicionando o prefixo de rota e o assembly.
- [`WebBootstrap.ConfigureRefit`](./src/AppProject.Web/Bootstraps/WebBootstrap.cs) registra todos os clientes HTTP. Atualize `GetApiClientAssemblies()` com os novos assemblies para que as interfaces sejam registradas automaticamente.
- [`AppProjectConstants`](./src/AppProject.Web/Constants/AppProjectConstants.cs) define o nome exibido no cabeçalho e os prefixos de armazenamento; ajuste ao personalizar o template.

#### Menu e permissões
- [`NavMenu.razor`](./src/AppProject.Web/Layout/NavMenu.razor) consulta `IPermissionClient` e exibe os itens do módulo General apenas para usuários com `PermissionType.System_ManageSettings`.
- Ao criar novos módulos, adicione os itens de menu correspondentes e avalie quais permissões serão exigidas.

### Testes
Os testes unitários residem em projetos como `src/AppProject.Core.Tests.<Módulo>` (ex.: `src/AppProject.Core.Tests.General`) e utilizam **NUnit**, **Moq**, **Shouldly** e **Bogus**. Eles validam tanto cenários positivos quanto exceções esperadas.

- [`CountryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CountryServiceTests.cs): cobre leitura, inserção, atualização e exclusão de países, além de validar duplicidade e autorização.
- [`StateServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/StateServiceTests.cs): garante a validação de nomes duplicados por país e o comportamento do CRUD de estados.
- [`CityServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CityServiceTests.cs): exercita a lógica de bairros aninhados, duplicidades e relacionamentos durante `Post`, `Put` e `Delete`.
- [`CountrySummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CountrySummaryServiceTests.cs): testa filtros por texto e o tratamento de entidades inexistentes.
- [`StateSummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/StateSummaryServiceTests.cs): avalia filtros por `CountryId` e busca individual.
- [`CitySummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CitySummaryServiceTests.cs): assegura que filtros por `StateId` e `SearchText` funcionem e que exceções sejam lançadas corretamente.

Cada classe de teste segue o padrão Arrange/Act/Assert, inicializando *mocks* do `IDatabaseRepository` e `IPermissionService` e utilizando `Bogus` para gerar dados confiáveis. O método utilitário `AssertAppExceptionAsync` (definido em cada classe de testes) simplifica a verificação de mensagens/`ExceptionCode` retornados pelas services. Ao criar novos cenários:
- Configure o mock de permissões para retornar `Task.CompletedTask` (mantendo o comportamento padrão das services).
- Use `Setup`/`ReturnsAsync` do Moq para simular consultas EF Core (ex.: `GetFirstOrDefaultAsync`, `HasAnyAsync`, `GetByConditionAsync`).
- Valide tanto fluxos felizes quanto fluxos de exceção, garantindo que regras de negócio sejam testadas antes de tocar o banco (`HasAnyAsync`) e após (`InsertAndSaveAsync`, `UpdateAsync` etc.).
- Prefira `Shouldly` para asserts legíveis (`response.Entity.ShouldBe(expectedCountry)`), mantendo consistência e mensagens claras.

Execute todos os testes com:
```bash
dotnet test AppProject.slnx
```
Ao criar novos módulos, replique a estrutura em `AppProject.Core.Tests.<Módulo>` e `AppProject.Web.Tests.<Módulo>` (ou mantenha projetos compartilhados com subpastas nomeadas) para cobrir regras de negócio e consultas.

## Preparando para produção
- Preencha todos os placeholders em `appsettings.json` e `wwwroot/appsettings.json` com valores reais (conexões de produção, Auth0, SendGrid, GitHub Models, URLs públicas etc.).
- Configure `ASPNETCORE_ENVIRONMENT=Production` para a API e `DOTNET_ENVIRONMENT=Production` para o frontend publicado.
- Atualize `Cors:AllowedOrigins` e `AllowedHosts` com os domínios oficiais.
- Cadastre novas URLs no Auth0 (callback, logout e origens web) e gere um `ClientSecret` se necessário.
- Garanta que o banco de dados de produção esteja criado e com as migrações aplicadas (`dotnet ef database update` ou migration automática no startup).
- Gere uma key dedicada no SendGrid e valide o domínio/remetente usado pelo produto.
- Gere um token GitHub exclusivo para produção e mantenha apenas o endpoint necessário em `AI:Endpoint`.
- Ajuste o `SystemAdminUser` para um e-mail realmente monitorado pela equipe de operações.
- Revise as configurações de logging (`Serilog`) e considere direcionar os logs para um sink persistente em produção.
- Confira se o Hangfire utiliza uma base separada ou uma connection string adequada para o ambiente.
- Remova dados de exemplo e valide permissões de usuários antes do go-live.
- Use variáveis de ambiente ou Azure App Configuration/Secrets Manager para armazenar credenciais sensíveis, evitando publicá-las em repositórios.
- Execute `dotnet publish -c Release src/AppProject.Core.API/AppProject.Core.API.csproj` e `dotnet publish -c Release src/AppProject.Web/AppProject.Web.csproj` para gerar os artefatos que serão enviados aos ambientes de produção.
- Configure pipelines (GitHub Actions, Azure DevOps, etc.) para rodar `dotnet test` e publicar os projetos automaticamente, garantindo que migrações e configurações sejam aplicadas antes do deploy.
