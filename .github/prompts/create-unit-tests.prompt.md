---
mode: agent
description: "Create unit tests for a CRUD service using NUnit, Moq, Shouldly, and Bogus"
---

# Create Unit Tests

Create unit tests for an existing CRUD service and its summary service in the AppProject .NET template.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Country`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Which services to test** (CRUD service, summary service, or both)
4. **Any specific business rules** beyond the standard duplicate check

## Instructions

Follow the `unit-testing` skill to create test classes:

### For the CRUD Service

Create `src/AppProject.Core.Tests.<Module>/Services/<EntityName>ServiceTests.cs` with:

1. **Test fixture setup:**
   - `Faker` for test data generation
   - `Mock<IDatabaseRepository>` for repository mocking
   - `Mock<IPermissionService>` configured to return `Task.CompletedTask`
   - Service instance with mocked dependencies

2. **Required test cases:**
   - `GetEntity_WhenEntityExists_ReturnsEntityAsync` — verify entity returned and permission checked
   - `GetEntity_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync` — verify `ExceptionCode.EntityNotFound`
   - `PostEntity_WhenNameAlreadyExists_ThrowsDuplicateNameAsync` — verify duplicate validation
   - `PostEntity_WhenValid_PersistsEntityAsync` — verify insert and returned ID
   - `PutEntity_WhenNameAlreadyExists_ThrowsDuplicateNameAsync` — verify duplicate on update
   - `PutEntity_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync` — verify not found on update
   - `DeleteEntity_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync` — verify not found on delete
   - `DeleteEntity_WhenEntityExists_RemovesEntityAsync` — verify deletion called

3. **Helper method:** `AssertAppExceptionAsync` for exception verification

### For the Summary Service

Create `src/AppProject.Core.Tests.<Module>/Services/<EntityName>SummaryServiceTests.cs` with:

1. **Test fixture setup** (simpler — no permission mock needed)

2. **Required test cases:**
   - `GetSummaries_WithSearchText_ReturnsFilteredResultsAsync`
   - `GetSummary_WhenNotFound_ThrowsEntityNotFoundAsync`
   - Additional filter tests if custom SearchRequest exists

### If the entity has additional business rules, add tests for:
   - FK validation (e.g., parent entity must exist)
   - Nested item validation (e.g., child items belong to parent)
   - Custom validation rules

## Best Practices

- Use `Bogus` Faker for all test data — never hardcode values
- Use `Shouldly` for all assertions
- Verify `Times.Once` for critical repository calls
- Use `Callback` to capture entities for detailed assertions
- Follow Arrange/Act/Assert pattern strictly

## Running

```bash
cd src && dotnet test AppProject.Core.Tests.<Module>
```
