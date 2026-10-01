# CRUD example

The example below shows, step by step, how the General module implements the Country, State, City, and Neighborhood registries. Follow the same steps when creating new modules.

## Backend

### 1. Identify the module
First, identify which module will contain your new entity. For example, imagine the registries for states, cities, and countries. These registries belong to a General module. Therefore, we use the General folders whenever possible inside the project.

#### If you need to create a new module
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

### 2. Content shared across modules
If you are adding files shared among modules, place those files in the root project instead of the module-specific project. For instance, imagine you are adding the Customer table. Customer can be used in several modules (invoice, financial, etc.). In that case, rather than creating a General or Customer module, the right approach is to place it in the root project inside a folder representing the shared module.

Below is a list of root projects where you can create folders representing parts of shared modules:
* `AppProject.Core.Models` — shared DTOs and requests. Always analyze whether the files must be shared with other modules. Prefer keeping files in the module project instead of the shared project.
* `AppProject.Core.Services` — common services, for example, summaries visible in multiple modules. However, not every summary should live here. Only summaries shared between modules belong here.
* `AppProject.Web` — layout, authentication, bootstrap, and navigation components.
* `AppProject.Web.ApiClient` — Refit interfaces reused by more than one module.
* `AppProject.Web.Models` — observable models used by multiple modules.
* `AppProject.Web.Shared` — generic Blazor components (dropdowns, cards, helper controls).
* `AppProject.Resources` — translations reused in different areas.

### 3. Adding DTOs to the API
The project contains API-side DTOs and WEB-side (client) DTOs. They differ because the WEB-side DTOs can have change notifications (`INotifyPropertyChanged`), while the API-side ones do not.

DTOs are added to the projects below:
* AppProject.Core.Models (for DTOs shared between modules; create internal folders with the module name when needed — for example, `AppProject.Core.Models/General`).
* AppProject.Core.Models.<Module> (for example, `AppProject.Core.Models.General`) for module-specific DTOs.
* Create additional projects such as `AppProject.Core.Models.<NewModule>` if you need to separate DTOs per module.

In a CRUD scenario we usually have two DTO types: Entity DTOs (inheriting from `IEntity`) and Summary DTOs (inheriting from `ISummary`).

#### Entity DTOs
Entity DTOs represent the table fields and must inherit from `IEntity`. They need to expose `RowVersion` to support optimistic concurrency and use DataAnnotations to validate required fields, maximum length, and basic rules. References:
- `AppProject.Core.Models.General/Country.cs` — simple entity with `Name`, `Code`, and `RowVersion`.
- `AppProject.Core.Models.General/State.cs` — adds `CountryId`, validated by `RequiredGuid`.
- `AppProject.Core.Models.General/City.cs` — besides the main fields, it keeps the `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` collections to synchronize neighborhoods.
- `AppProject.Core.Models.General/Neighborhood.cs` — basic neighborhood structure used in both the API and the frontend.

##### Entity validations
Validation exceptions return `ExceptionDetail` with the `RequestValidation` code. All custom attributes live in `AppProject.Models.CustomValidators`:
- `ValidateCollectionAttribute` guarantees cascading validation for lists.
- `RequiredGuidAttribute` prevents sending empty GUIDs.

The middleware [`AppProject.Core.API/Middlewares/ExceptionMiddleware.cs`](../src/AppProject.Core.API/Middlewares/ExceptionMiddleware.cs) converts exceptions into standardized responses, while `Bootstrap.ConfigureValidations` forces `AppException` to be thrown when `ModelState` is invalid.

For relationships, keep only the identifiers (for example, `StateId` in `City`). When dealing with aggregated entities, such as city neighborhoods, use the parent DTO collections (`City.cs`) of `CreateOrUpdateRequest`/`DeleteRequest` to indicate inserts, updates, and deletions.

#### Summary DTOs
Use summary DTOs to power grids, combos, and other read queries. They inherit from `ISummary`, have no DataAnnotations, and contain only the fields needed to display information in the UI:
- `AppProject.Core.Models/General/CountrySummary.cs` — example from the General module with `Id` and `Name`.
- `AppProject.Core.Models/General/StateSummary.cs` — example from the General module with `CountryName` and `CountryId`.
- `AppProject.Core.Models/General/CitySummary.cs` — example from the General module with `StateName`, `StateId`, `CountryName`, and `CountryId`.

For advanced searches, use `SearchRequest` as the base class and add specific properties:
- `AppProject.Core.Models/General/StateSummarySearchRequest.cs` — allows filtering states by `CountryId`.
- `AppProject.Core.Models/General/CitySummarySearchRequest.cs` — filters cities by `StateId`.
Avoid creating empty `SearchRequest` derivatives. Only create a class derived from SearchRequest when extra filters exist. For example, there is no `CountrySummarySearchRequest` because we do not add extra filters to the country search the way we do for the StateSummarySearchRequest and CitySummarySearchRequest.

Always evaluate whether the summary should bring aggregated names instead of full objects. That simplifies serialization and avoids unnecessary loads or reference cycles.

### 4. Adding database entities
The database entities live in `AppProject.Core.Infrastructure.Database/Entities` and follow the `Tb[Name]` pattern. Recommendations:
- `TbCountry.cs`, `TbState.cs`, `TbCity.cs`, `TbNeighborhood.cs` — keep DataAnnotations for keys, column sizes, and relationships. Use plural table names (`[Table("Countries")]`) and configure navigation collections (`States`, `Cities`, `Neighborhoods`) to simplify loading.
- Store only the information required for persistence; any additional logic must stay in the services.
- Apply `MaxLength` to text columns and keep plausible values for fields shared with the DTOs.

Relationships are modeled with foreign key fields (for example, `CountryId` in `TbState`) and navigation properties with `[ForeignKey]`. When creating additional entities, follow the same pattern so EF Core configures the constraints automatically.

#### Adding the EntityTypeConfiguration files
Configuration classes complement the entities with indexes, extra constraints, and EF Core-specific rules. They live in `AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/[Module]` and follow the `Tb[Name]Configuration` pattern. Examples:
- `TbCountryConfiguration.cs` — defines a unique index for `Name`.
- `TbStateConfiguration.cs` — creates an index to speed up searches by `Name`.
- `TbCityConfiguration.cs` and `TbNeighborhoodConfiguration.cs` — configure indexes for dependent entities.

All of them inherit from `IEntityTypeConfiguration<T>` and are automatically loaded by `ApplicationDbContext`. If you need new constraints (for example, composite indexes), implement them in these files instead of bloating the entities with extra logic.

Important: when adding a new file that inherits from `IEntityTypeConfiguration`, you do not need to register it manually in `ApplicationDbContext`; the `OnModelCreating` method already scans the assembly and applies each configuration.

#### Adding DbSet to ApplicationDbContext
Update [`ApplicationDbContext`](../src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs) whenever you create a new entity. Each table must contain a `DbSet<T>` with a plural name (for example, `Countries`, `States`, `Cities`, `Neighborhoods`). Keeping this convention improves readability and avoids mismatches between EF Core and the database.

#### Running migrations
To generate the database scripts, we need to run the Entity Framework migration. Open the terminal in the project's src folder and run the following command:

```bash
dotnet ef migrations add MigrationName --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

Replace MigrationName with something that identifies your migration, such as the name of one of the tables you are modifying or a subject related to the change.

This command creates the migration script files under the Migrations folder of the `AppProject.Core.Infrastructure.Database` project.

Important: you do not need to apply the migration manually because it is applied automatically when the API starts.

### 5. Adding Mapster configuration classes
By default, Mapster can map entities to DTOs when the property names match. Create additional configurations only when you need to transform fields or include relationship data. In the General module we use:
- `AppProject.Core.Infrastructure.Database/Mapper/General/StateSummaryMapsterConfig.cs` — injects `CountryName` when mapping `TbState` → `StateSummary`.
- `AppProject.Core.Infrastructure.Database/Mapper/General/CitySummaryMapsterConfig.cs` — exposes `StateName` and `CountryName` from the related entities.

These classes implement `IRegisterMapsterConfig` and are loaded in the bootstrap (`Bootstrap.ConfigureMapper`). When adding new configurations:
1. Create the `[Dto]MapsterConfig.cs` file inside the `Mapper/[Module]` folder.
2. Configure `TypeAdapterConfig` in the `Register` method.
3. Avoid complex logic in the mapper; use the services to handle business rules.

Services centralize business rules, validations, and repository orchestration. They live in `AppProject.Core.Services` for shared items and in `AppProject.Core.Services.<Module>` (for example, `AppProject.Core.Services.General`) for module-specific implementations. All compatible types are automatically registered in DI by `Bootstrap.ConfigureServices`.

### 6. Adding service classes

#### Service class interfaces
- `ICountryService.cs`, `IStateService.cs`, and `ICityService.cs` implement `ITransientService` and the generic contracts `IGetEntity`, `IPostEntity`, `IPutEntity`, and `IDeleteEntity`. That standardizes the CRUD signatures and keeps the API consistent.
- `ICityService` adds `GetNeighborhoodEntitiesAsync`, which returns neighborhoods associated with a city using `GetByParentIdRequest<Guid>`.
- The summary interfaces (`ICountrySummaryService.cs`, `IStateSummaryService.cs`, `ICitySummaryService.cs`) expose `IGetSummaries`/`IGetSummary` with their respective requests (`SearchRequest`, `StateSummarySearchRequest`, `CitySummarySearchRequest`). Follow this approach when creating new summaries.

#### Service classes
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

#### Summary service classes
- `CountrySummaryService.cs`, `StateSummaryService.cs`, and `CitySummaryService.cs` handle read queries. They call `GetByConditionAsync` with filters (`SearchText`, `Take`, `CountryId`, `StateId`) and use `SummariesResponse<T>` to return immutable collections.
- When `GetSummaryAsync` cannot find the record, they throw `AppException(ExceptionCode.EntityNotFound)` to stay consistent with the write services.
- As you expand the template, follow this pattern: keep read services free from expensive permission validations (unless specific requirements exist) and centralize filters in request objects so the frontend can reuse them.
- Avoid redundant `Include` statements in summary queries when the columns are already projected by the view/adapter.

### 7. Creating controller classes
Controllers live in module-specific projects such as `AppProject.Core.Controllers.<Module>` (for example, `AppProject.Core.Controllers.General`). They expose only the logic needed to receive the requests, call the services, and return the standardized result (`Ok(...)`). Examples:
- `CountryController.cs`, `StateController.cs`, `CityController.cs` — implement CRUD endpoints for each entity.
- `CityController` also offers `GetNeighborhoodsAsync` to fetch related neighborhoods.
- `CountrySummaryController.cs`, `StateSummaryController.cs`, `CitySummaryController.cs` — expose query endpoints (`GetSummariesAsync`, `GetSummaryAsync`).

General guidelines:
- Apply `[Authorize]` to protect the endpoints and `[ApiController]` to enable automatic model validation.
- Use the `api/<module>/[controller]/[action]` route pattern (for example, `api/general/Country/Post`).
- Receive parameters using `[FromQuery]` for searches (`GetByIdRequest`, `DeleteRequest`) and `[FromBody]` for mutations (`CreateOrUpdateRequest`).
- Always return `IActionResult` with `Ok(...)` to keep consistency and simplify the global error handling.

## Frontend
The frontend is a **Blazor WebAssembly** application that consumes the API via Refit and uses Radzen components. The General module implementation serves as the guide for new modules.

### Overview and key files
- Client-side models live in `AppProject.Web.Models` and `AppProject.Web.Models.<Module>` (for example, `AppProject.Web.Models.General`). All of them inherit from [`ObservableModel`](../src/AppProject.Web.Models/ObservableModel.cs), which implements `INotifyPropertyChanged` to update the UI automatically.
- Entity classes such as [`Country.cs`](../src/AppProject.Web.Models.General/Country.cs), [`State.cs`](../src/AppProject.Web.Models.General/State.cs), [`City.cs`](../src/AppProject.Web.Models.General/City.cs), and [`Neighborhood.cs`](../src/AppProject.Web.Models.General/Neighborhood.cs) mirror the API DTOs. In the case of `City`, we keep the `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` collections to send nested changes.
- Summaries used in grids and combos stay in folders like [`AppProject.Web.Models/<Module>`](../src/AppProject.Web.Models/General). Examples for the General module: [`CountrySummary.cs`](../src/AppProject.Web.Models/General/CountrySummary.cs), [`StateSummary.cs`](../src/AppProject.Web.Models/General/StateSummary.cs), and [`CitySummary.cs`](../src/AppProject.Web.Models/General/CitySummary.cs).

### HTTP clients with Refit
- CRUD interfaces live in projects such as [`AppProject.Web.ApiClient.<Module>`](../src/AppProject.Web.ApiClient.General). Example: [`ICityClient.cs`](../src/AppProject.Web.ApiClient.General/ICityClient.cs) covers every `CityController` endpoint in the General module.
- Summaries have separate clients inside folders like [`AppProject.Web.ApiClient/<Module>`](../src/AppProject.Web.ApiClient/General). See the General module files such as [`ICitySummaryClient.cs`](../src/AppProject.Web.ApiClient/General/ICitySummaryClient.cs) and [`ICountrySummaryClient.cs`](../src/AppProject.Web.ApiClient/General/ICountrySummaryClient.cs).
- The bootstrap [`WebBootstrap.cs`](../src/AppProject.Web/Bootstraps/WebBootstrap.cs) dynamically registers every Refit interface defined in the assemblies listed by `GetApiClientAssemblies()`. When you add a new module, include the corresponding assembly.

### Search pages
- Search pages inherit from [`SearchPage<TRequest,TSummary>`](../src/AppProject.Web.Framework/Pages/SearchPage.cs), which encapsulates executing searches, selecting items, and displaying alerts when the `Take` limit is reached.
- The [`SearchControl.razor`](../src/AppProject.Web.Framework/Components/SearchControl.razor) component provides a standard form with a text field, advanced filters, and a configurable alert.
- Examples:
  - [`CountrySummaryPage.razor`](../src/AppProject.Web.General/Pages/CountrySummaryPage.razor) shows the grid with countries and the `New`, `Edit`, and `Delete` operations.
- [`StateSummaryPage.razor`](../src/AppProject.Web.General/Pages/StateSummaryPage.razor) adds a country filter via `CountrySummaryDropDownDataGridControl`.
- [`CitySummaryPage.razor`](../src/AppProject.Web.General/Pages/CitySummaryPage.razor) filters by state and displays additional columns.
- The standard grid is [`DataGridControl.razor`](../src/AppProject.Web.Framework/Components/DataGridControl.razor), which accepts `GlobalActions`, `ContextActions`, and integrates with multi-selection.

### Form pages and nested items
- Forms inherit from [`ModelFormPage<TModel>`](../src/AppProject.Web.Framework/Pages/ModelFormPage.cs) and use the [`ModelFormControl.razor`](../src/AppProject.Web.Framework/Components/ModelFormControl.razor) component.
- Reuse the model properties (`TModel`) directly for binds and selections instead of creating duplicate properties (for example, avoid `SelectedId`/`SelectedAnotherField`).
- DropDown components must inherit from `DropDownDataGridControl`. When using them, ensure `TValue` is correct (`Guid` for required DTO fields and `Guid?` if the field is optional).
- `ModelFormControl` allows important customizations:
  - `ShowNewAction`, `ShowEditAction`, and `ShowDeleteAction` in `DataGridControl` control which buttons are shown.
  - `PreferAddOverNew` changes the default button text to "Add" (as in [`CityFormPage.razor`](../src/AppProject.Web.General/Pages/CityFormPage.razor) when managing neighborhoods).
  - `PreferOpenOverEdit` displays "Open" instead of "Edit", useful for read-only pages.
  - `PreferExecuteOverSave` renames the main button to "Execute", suitable for processing screens.
  - `PreferCloseOverCancel` applies the closing style to the secondary button.
- For nested relationships, follow the `CityFormPage` example:
  - Load child records via a dedicated client (`ICityClient.GetNeighborhoodsAsync`).
  - When inserting/updating an item, add it to `ChangedNeighborhoodRequests`.
  - When deleting, move the identifier to `DeletedNeighborhoodRequests`.
  - Use specialized dialogs (such as [`NeighborhoodFormPage.razor`](../src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor)) to edit child items.
- Always consult the Radzen component documentation before setting attributes like `Min`/`Max`. For decimal values use expressions like `@(0.01m)` in `RadzenNumeric` to ensure the value is treated as `decimal`, and avoid redundant validators when the property already has `[Required]` in the model.

### Full flow for the General module records
1. **Country**
   - Search: [`CountrySummaryPage.razor`](../src/AppProject.Web.General/Pages/CountrySummaryPage.razor) calls `ICountrySummaryClient` and, after confirming deletions, uses `ICountryClient.DeleteAsync`.
   - Form: [`CountryFormPage.razor`](../src/AppProject.Web.General/Pages/CountryFormPage.razor) opens inside `DialogService`, reusing the same component to create or edit. Visual validations use `RadzenRequiredValidator` and `RadzenLengthValidator`.
2. **State**
   - Search: [`StateSummaryPage.razor`](../src/AppProject.Web.General/Pages/StateSummaryPage.razor) adds a country filter via [`CountrySummaryDropDownDataGridControl.razor`](../src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor).
   - Form: [`StateFormPage.razor`](../src/AppProject.Web.General/Pages/StateFormPage.razor) requires selecting a country. The `Guid` validation uses `RadzenCustomValidator` to block `Guid.Empty`.
3. **City**
   - Search: [`CitySummaryPage.razor`](../src/AppProject.Web.General/Pages/CitySummaryPage.razor) combines state filters, ordering, and extra columns (`StateName`, `CountryName`).
   - Form: [`CityFormPage.razor`](../src/AppProject.Web.General/Pages/CityFormPage.razor) uses another `DataGridControl` to manage neighborhoods and sets `PreferAddOverNew` to reflect inserting child items. The `ChangedNeighborhoodRequests` and `DeletedNeighborhoodRequests` lists update whenever the user confirms the neighborhood dialog.
4. **Neighborhoods**
   - Child form: [`NeighborhoodFormPage.razor`](../src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor) inherits from `ModelFormPage<Neighborhood>` and returns the object via `CloseDialogAsync`. The component is used both to create and edit records within the city.

### Reusable components
- [`DataGridControl.razor`](../src/AppProject.Web.Framework/Components/DataGridControl.razor) wraps `RadzenDataGrid` with multi-selection, localization, and configurable buttons (`ShowNewAction`, `ShowEditAction`, `ShowDeleteAction`, `PreferAddOverNew`, `PreferOpenOverEdit`).
- [`ModelFormControl.razor`](../src/AppProject.Web.Framework/Components/ModelFormControl.razor) standardizes the form header, includes `GlobalActions` slots, and exposes the `PreferExecuteOverSave` / `PreferCloseOverCancel` toggles.
- [`FieldsetControl.razor`](../src/AppProject.Web.Framework/Components/FieldsetControl.razor) creates collapsible fieldsets with centralized translations.
- [`SearchControl.razor`](../src/AppProject.Web.Framework/Components/SearchControl.razor) provides basic/advanced filters and the automatic alert for `Take`.
- [`DropDownDataGridControl.cs`](../src/AppProject.Web.Framework/Components/DropDownDataGridControl.cs) adjusts the `RadzenDropDownDataGrid` texts and is reused by [`CountrySummaryDropDownDataGridControl.razor`](../src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor) and [`StateSummaryDropDownDataGridControl.razor`](../src/AppProject.Web.Shared/General/Components/StateSummaryDropDownDataGridControl.razor), ensuring the selected item loads even after pagination/filtering.
- [`BusyIndicatorControl.razor`](../src/AppProject.Web.Framework/Components/BusyIndicatorControl.razor) is used by [`AppProjectComponentBase`](../src/AppProject.Web.Framework/Components/AppProjectComponentBase.cs) to display progress dialogs and handle exceptions (including `ApiException` responses from Refit).
- Layout and global preferences:
  - [`LanguageSelector.razor`](../src/AppProject.Web/Layout/LanguageSelector.razor) persists the selected culture using `Blazored.LocalStorage`.
  - [`ThemeToggle.razor`](../src/AppProject.Web/Layout/ThemeToggle.razor) toggles between the themes defined in [`ThemeConstants`](../src/AppProject.Web/Constants/ThemeConstants.cs).
  - [`Login.razor`](../src/AppProject.Web/Layout/Login.razor) summarizes the OIDC authentication flow (login/logout).

### Localization and resources
- Both the API and the frontend consume the resources defined in `AppProject.Resources`. The helper [`StringResource.cs`](../src/AppProject.Resources/StringResource.cs) reads [`Resource.resx`](../src/AppProject.Resources/Resource.resx), [`Resource.pt-BR.resx`](../src/AppProject.Resources/Resource.pt-BR.resx), and [`Resource.es-ES.resx`](../src/AppProject.Resources/Resource.es-ES.resx).
- When adding new screens or messages, include the keys in all three files to keep multilingual support.
- Preserve the comments and grouping (forms separated, validations and menus together) when editing the `.resx` files; each new key must have its own line following the existing pattern to make diffs easier.
- When you need to reserve placeholders for values interpolated later, use `{{Value}}` instead of `{Value}` so the resource parser treats the text as literal until the template replaces it.
- Reusable components consume specific keys (`DataGridControl_NewButton_Text`, `DataGridControl_AddButton_Text`, `ModelFormControl_SaveButton_Text`, `ModelFormControl_ExecuteButton_Text`, among others). Adjust these keys to customize labels without changing the code.

### Lazy loading, navigation, and bootstrap
- [`AppProject.Web/App.razor`](../src/AppProject.Web/App.razor) loads assemblies on demand. Routes starting with `general` load `AppProject.Web.General.dll`. For new modules, replicate the logic by adding the route prefix and the assembly.
- [`WebBootstrap.ConfigureRefit`](../src/AppProject.Web/Bootstraps/WebBootstrap.cs) registers all HTTP clients. Update `GetApiClientAssemblies()` with the new assemblies so every interface is registered automatically.
- [`AppProjectConstants`](../src/AppProject.Web/Constants/AppProjectConstants.cs) defines the name displayed in the header and the storage prefixes; adjust it when customizing the template.

### Menu and permissions
- [`NavMenu.razor`](../src/AppProject.Web/Layout/NavMenu.razor) queries `IPermissionClient` and displays the General module items only for users with `PermissionType.System_ManageSettings`.
- When creating new modules, add the corresponding menu items and evaluate which permissions they require.
