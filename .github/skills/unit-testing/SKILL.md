---
name: unit-testing
description: Step-by-step instructions for creating unit tests in the AppProject .NET template using NUnit, Moq, Shouldly, and Bogus. Covers testing CRUD services, summary services, permission validation, duplicate checks, and exception handling. Use when the user wants to write unit tests for backend services.
metadata:
  author: appproject
  version: "1.0"
---

# Unit Testing

Follow these patterns to create unit tests for backend services. The tests use **NUnit**, **Moq**, **Shouldly**, and **Bogus**.

## Test Project Location

Tests live in `src/AppProject.Core.Tests.<Module>/Services/`

## Test Class Structure

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.<Module>;
using AppProject.Core.Models.<Module>;
using AppProject.Core.Services.Auth;
using AppProject.Core.Services.<Module>;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Bogus;
using Moq;
using Shouldly;

namespace AppProject.Core.Tests.<Module>.Services;

[TestFixture]
public class <EntityName>ServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private <EntityName>Service service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();

        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.<PermissionName>,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.service = new <EntityName>Service(
            this.databaseRepositoryMock.Object,
            this.permissionServiceMock.Object);
    }

    // Tests go here...

    private static async Task<AppException> AssertAppExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
            Assert.Fail("Expected AppException was not thrown.");
            throw new InvalidOperationException();
        }
        catch (AppException ex)
        {
            return ex;
        }
    }
}
```

## Required Test Scenarios for CRUD Services

### 1. GetEntity — When Entity Exists

```csharp
[Test]
public async Task GetEntity_WhenEntityExists_ReturnsEntityAsync()
{
    var id = Guid.NewGuid();
    var expected = new <EntityName>
    {
        Id = id,
        Name = this.faker.Commerce.ProductName()
    };

    this.databaseRepositoryMock
        .Setup(x => x.GetFirstOrDefaultAsync<Tb<EntityName>, <EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id });

    response.Entity.ShouldBe(expected);
    this.permissionServiceMock.Verify(
        p => p.ValidateCurrentUserPermissionAsync(
            PermissionType.<PermissionName>,
            null,
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

### 2. GetEntity — When Entity Does Not Exist

```csharp
[Test]
public async Task GetEntity_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync()
{
    this.databaseRepositoryMock
        .Setup(x => x.GetFirstOrDefaultAsync<Tb<EntityName>, <EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((<EntityName>?)null);

    var exception = await AssertAppExceptionAsync(() => this.service.GetEntityAsync(
        new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

    exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
}
```

### 3. PostEntity — When Duplicate Exists

```csharp
[Test]
public async Task PostEntity_WhenNameAlreadyExists_ThrowsDuplicateNameAsync()
{
    var entity = new <EntityName>
    {
        Id = Guid.NewGuid(),
        Name = this.faker.Commerce.ProductName()
    };

    this.databaseRepositoryMock
        .Setup(x => x.HasAnyAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
        new CreateOrUpdateRequest<<EntityName>> { Entity = entity }));

    exception.ExceptionCode.ShouldBe(ExceptionCode.<Module>_<EntityName>_DuplicateName);
}
```

### 4. PostEntity — When Valid

```csharp
[Test]
public async Task PostEntity_WhenValid_PersistsEntityAsync()
{
    var id = Guid.NewGuid();
    var entity = new <EntityName>
    {
        Id = id,
        Name = this.faker.Commerce.ProductName()
    };

    Tb<EntityName>? inserted = null;

    this.databaseRepositoryMock
        .Setup(x => x.HasAnyAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    this.databaseRepositoryMock
        .Setup(x => x.InsertAndSaveAsync(
            It.IsAny<Tb<EntityName>>(),
            It.IsAny<CancellationToken>()))
        .Callback<Tb<EntityName>, CancellationToken>((e, _) => inserted = e)
        .Returns(Task.CompletedTask);

    var response = await this.service.PostEntityAsync(new CreateOrUpdateRequest<<EntityName>> { Entity = entity });

    response.Id.ShouldBe(id);
    inserted.ShouldNotBe(null);
    inserted!.Name.ShouldBe(entity.Name);

    this.databaseRepositoryMock.Verify(
        x => x.InsertAndSaveAsync(
            It.Is<Tb<EntityName>>(tb => tb.Id == id),
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

### 5. PutEntity — When Duplicate Exists

```csharp
[Test]
public async Task PutEntity_WhenNameAlreadyExists_ThrowsDuplicateNameAsync()
{
    var entity = new <EntityName>
    {
        Id = Guid.NewGuid(),
        Name = this.faker.Commerce.ProductName()
    };

    this.databaseRepositoryMock
        .Setup(x => x.HasAnyAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
        new CreateOrUpdateRequest<<EntityName>> { Entity = entity }));

    exception.ExceptionCode.ShouldBe(ExceptionCode.<Module>_<EntityName>_DuplicateName);
}
```

### 6. PutEntity — When Entity Does Not Exist

```csharp
[Test]
public async Task PutEntity_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync()
{
    var entity = new <EntityName>
    {
        Id = Guid.NewGuid(),
        Name = this.faker.Commerce.ProductName()
    };

    this.databaseRepositoryMock
        .Setup(x => x.HasAnyAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    this.databaseRepositoryMock
        .Setup(x => x.GetFirstOrDefaultAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((Tb<EntityName>?)null);

    var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
        new CreateOrUpdateRequest<<EntityName>> { Entity = entity }));

    exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
}
```

### 7. DeleteEntity — When Entity Does Not Exist

```csharp
[Test]
public async Task DeleteEntity_WhenEntityDoesNotExist_ThrowsEntityNotFoundAsync()
{
    this.databaseRepositoryMock
        .Setup(x => x.GetFirstOrDefaultAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((Tb<EntityName>?)null);

    var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
        new DeleteRequest<Guid> { Id = Guid.NewGuid() }));

    exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
}
```

### 8. DeleteEntity — When Entity Exists

```csharp
[Test]
public async Task DeleteEntity_WhenEntityExists_RemovesEntityAsync()
{
    var id = Guid.NewGuid();
    var tbEntity = new Tb<EntityName> { Id = id };

    this.databaseRepositoryMock
        .Setup(x => x.GetFirstOrDefaultAsync<Tb<EntityName>>(
            It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(tbEntity);

    this.databaseRepositoryMock
        .Setup(x => x.DeleteAndSaveAsync(tbEntity, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    await this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = id });

    this.databaseRepositoryMock.Verify(
        x => x.DeleteAndSaveAsync(tbEntity, It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## Summary Service Tests

```csharp
[TestFixture]
public class <EntityName>SummaryServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private <EntityName>SummaryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.service = new <EntityName>SummaryService(this.databaseRepositoryMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithSearchText_ReturnsFilteredResultsAsync()
    {
        var summaries = new List<<EntityName>Summary>
        {
            new() { Id = Guid.NewGuid(), Name = this.faker.Commerce.ProductName() }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<Tb<EntityName>, <EntityName>Summary>(
                It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        var response = await this.service.GetSummariesAsync(
            new SearchRequest { SearchText = "test" });

        response.Summaries.ShouldBe(summaries);
    }

    [Test]
    public async Task GetSummary_WhenNotFound_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<Tb<EntityName>, <EntityName>Summary>(
                It.IsAny<Func<IQueryable<Tb<EntityName>>, IQueryable<Tb<EntityName>>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((<EntityName>Summary?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetSummaryAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    private static async Task<AppException> AssertAppExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
            Assert.Fail("Expected AppException was not thrown.");
            throw new InvalidOperationException();
        }
        catch (AppException ex)
        {
            return ex;
        }
    }
}
```

## Best Practices

1. **Always follow Arrange/Act/Assert** pattern
2. **Use `Bogus`** for generating test data — `this.faker.Commerce.ProductName()`, `this.faker.Address.Country()`, etc.
3. **Use `Shouldly`** for readable assertions — `response.Entity.ShouldBe(expected)`
4. **Configure permission mock** to return `Task.CompletedTask` in `SetUp`
5. **Verify mock calls** with `Times.Once` to ensure methods are called exactly as expected
6. **Test both happy paths and exception flows**
7. **Use `AssertAppExceptionAsync`** helper to verify exception codes
8. **Each test class** should have its own `AssertAppExceptionAsync` method
9. **Use `Callback`** to capture entities passed to repository methods for assertions

## Running Tests

```bash
cd src
dotnet test AppProject.slnx
```

Or run specific test project:
```bash
dotnet test AppProject.Core.Tests.<Module>
```
