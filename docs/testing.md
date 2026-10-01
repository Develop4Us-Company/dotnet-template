# Tests

The unit tests live in projects such as `src/AppProject.Core.Tests.<Module>` (for example, `src/AppProject.Core.Tests.General`) and rely on **NUnit**, **Moq**, **Shouldly**, and **Bogus**. They validate both positive scenarios and expected exceptions.

- [`CountryServiceTests.cs`](../src/AppProject.Core.Tests.General/Services/CountryServiceTests.cs): covers reading, inserting, updating, and deleting countries, and validates duplicates and authorization.
- [`StateServiceTests.cs`](../src/AppProject.Core.Tests.General/Services/StateServiceTests.cs): ensures validation of duplicate names per country and the CRUD behavior for states.
- [`CityServiceTests.cs`](../src/AppProject.Core.Tests.General/Services/CityServiceTests.cs): exercises the nested neighborhood logic, duplicates, and relationships during `Post`, `Put`, and `Delete`.
- [`CountrySummaryServiceTests.cs`](../src/AppProject.Core.Tests.General/Services/CountrySummaryServiceTests.cs): tests text filters and the handling of missing entities.
- [`StateSummaryServiceTests.cs`](../src/AppProject.Core.Tests.General/Services/StateSummaryServiceTests.cs): evaluates filters by `CountryId` and individual retrieval.
- [`CitySummaryServiceTests.cs`](../src/AppProject.Core.Tests.General/Services/CitySummaryServiceTests.cs): ensures filters by `StateId` and `SearchText` work and that the proper exceptions are thrown.

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
