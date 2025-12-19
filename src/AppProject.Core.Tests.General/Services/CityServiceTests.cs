using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Core.Services.Auth;
using AppProject.Core.Services.General;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Bogus;
using Moq;
using Shouldly;

namespace AppProject.Core.Tests.General.Services;

[TestFixture]
public class CityServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private CityService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.permissionServiceMock = new Mock<IPermissionService>();

        this.permissionServiceMock
            .Setup(p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.System_ManageSettings,
                It.IsAny<PermissionContext?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.service = new CityService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetEntity_WhenCityExists_ReturnsEntityAsync()
    {
        var id = Guid.NewGuid();
        var expectedCity = new City
        {
            Id = id,
            Name = this.faker.Address.City(),
            Code = this.faker.Address.ZipCode(),
            StateId = Guid.NewGuid()
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity, City>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCity);

        var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id });

        response.Entity.ShouldBe(expectedCity);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.System_ManageSettings,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetEntity_WhenCityDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity, City>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetEntityAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task PostEntity_WhenCityNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var city = this.CreateCity();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_City_DuplicateName);
    }

    [Test]
    public async Task PostEntity_WhenNeighborhoodNamesRepeats_ThrowsDuplicateNeighborhoodAsync()
    {
        var city = this.CreateCity();
        var duplicateName = this.faker.Address.CityPrefix();

        city.ChangedNeighborhoodRequests = new List<CreateOrUpdateRequest<Neighborhood>>
        {
            new () { Entity = new Neighborhood { Id = Guid.Empty, Name = duplicateName } },
            new () { Entity = new Neighborhood { Id = Guid.Empty, Name = duplicateName } }
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_City_Neighborhood_DuplicateName);
    }

    [Test]
    public async Task PostEntity_WhenNeighborhoodAlreadyExists_ThrowsDuplicateNeighborhoodAsync()
    {
        var city = this.CreateCity();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .SetupSequence(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_City_Neighborhood_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenCityNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var city = this.CreateCity();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_City_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenNeighborhoodNamesRepeats_ThrowsDuplicateNeighborhoodAsync()
    {
        var city = this.CreateCity();
        var duplicateName = this.faker.Address.CityPrefix();

        city.ChangedNeighborhoodRequests = new List<CreateOrUpdateRequest<Neighborhood>>
        {
            new () { Entity = new Neighborhood { Id = Guid.Empty, Name = duplicateName } },
            new () { Entity = new Neighborhood { Id = Guid.Empty, Name = duplicateName } }
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_City_Neighborhood_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenNeighborhoodAlreadyExists_ThrowsDuplicateNeighborhoodAsync()
    {
        var city = this.CreateCity();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .SetupSequence(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TbCity { Id = city.Id!.Value });

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_City_Neighborhood_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenCityDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var city = this.CreateCity();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbNeighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbCity?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<City> { Entity = city }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenCityDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbCity?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
            new DeleteRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenCityExists_RemovesCityAsync()
    {
        var entity = new TbCity { Id = Guid.NewGuid() };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        this.databaseRepositoryMock
            .Setup(x => x.DeleteAndSaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = entity.Id });

        this.databaseRepositoryMock.Verify(
            x => x.DeleteAndSaveAsync(
                entity,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetNeighborhoodEntities_WhenRequested_ReturnsNeighborhoodsAsync()
    {
        var cityId = Guid.NewGuid();
        var neighborhoods = new List<Neighborhood>
        {
            new () { Id = Guid.NewGuid(), Name = this.faker.Address.CityPrefix() },
            new () { Id = Guid.NewGuid(), Name = this.faker.Address.CityPrefix() }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbNeighborhood, Neighborhood>(
                It.IsAny<Func<IQueryable<TbNeighborhood>, IQueryable<TbNeighborhood>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(neighborhoods);

        var response = await this.service.GetNeighborhoodEntitiesAsync(new GetByParentIdRequest<Guid>
        {
            ParentId = cityId
        });

        response.Entities.ShouldBe(neighborhoods);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.System_ManageSettings,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
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

    private City CreateCity()
    {
        return new City
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Address.City(),
            Code = this.faker.Address.ZipCode(),
            StateId = Guid.NewGuid(),
            ChangedNeighborhoodRequests = new List<CreateOrUpdateRequest<Neighborhood>>
            {
                new () { Entity = new Neighborhood { Id = Guid.NewGuid(), Name = this.faker.Address.CityPrefix() } },
                new () { Entity = new Neighborhood { Id = Guid.NewGuid(), Name = this.faker.Address.CitySuffix() } }
            },
            DeletedNeighborhoodRequests = new List<DeleteRequest<Guid>>()
        };
    }
}
