using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
public class CountryServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private CountryService service = null!;

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

        this.service = new CountryService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetEntity_WhenCountryExists_ReturnsEntityAsync()
    {
        var id = Guid.NewGuid();
        var expectedCountry = new Country
        {
            Id = id,
            Name = this.faker.Address.Country(),
            Code = this.faker.Address.CountryCode()
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCountry, Country>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCountry);

        var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id });

        response.Entity.ShouldBe(expectedCountry);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.System_ManageSettings,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetEntity_WhenCountryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCountry, Country>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetEntityAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task PostEntity_WhenCountryNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Address.Country(),
            Code = this.faker.Address.CountryCode()
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<Country> { Entity = country }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_Country_DuplicateName);
    }

    [Test]
    public async Task PostEntity_WhenValid_PersistsCountryAsync()
    {
        var id = Guid.NewGuid();
        var country = new Country
        {
            Id = id,
            Name = this.faker.Address.Country(),
            Code = this.faker.Address.CountryCode()
        };

        TbCountry? insertedCountry = null;

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.InsertAndSaveAsync(
                It.IsAny<TbCountry>(),
                It.IsAny<CancellationToken>()))
            .Callback<TbCountry, CancellationToken>((entity, _) => insertedCountry = entity)
            .Returns(Task.CompletedTask);

        var response = await this.service.PostEntityAsync(new CreateOrUpdateRequest<Country> { Entity = country });

        response.Id.ShouldBe(id);
        insertedCountry.ShouldNotBe(null);
        insertedCountry!.Name.ShouldBe(country.Name);

        this.databaseRepositoryMock.Verify(
            x => x.InsertAndSaveAsync(
                It.Is<TbCountry>(tb => tb.Id == id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PutEntity_WhenCountryNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Address.Country(),
            Code = this.faker.Address.CountryCode()
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<Country> { Entity = country }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_Country_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenCountryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Address.Country(),
            Code = this.faker.Address.CountryCode()
        };

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbCountry?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<Country> { Entity = country }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenCountryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var id = Guid.NewGuid();

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbCountry?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
            new DeleteRequest<Guid> { Id = id }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenCountryExists_RemovesCountryAsync()
    {
        var id = Guid.NewGuid();
        var entity = new TbCountry { Id = id };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCountry>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        this.databaseRepositoryMock
            .Setup(x => x.DeleteAndSaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await this.service.DeleteEntityAsync(new DeleteRequest<Guid> { Id = id });

        this.databaseRepositoryMock.Verify(
            x => x.DeleteAndSaveAsync(
                entity,
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
}
