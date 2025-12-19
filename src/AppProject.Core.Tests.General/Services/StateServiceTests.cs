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
public class StateServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private Mock<IPermissionService> permissionServiceMock = null!;
    private StateService service = null!;

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

        this.service = new StateService(this.databaseRepositoryMock.Object, this.permissionServiceMock.Object);
    }

    [Test]
    public async Task GetEntity_WhenStateExists_ReturnsEntityAsync()
    {
        var id = Guid.NewGuid();
        var expectedState = new State
        {
            Id = id,
            Name = this.faker.Address.State(),
            Code = this.faker.Address.StateAbbr(),
            CountryId = Guid.NewGuid()
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbState, State>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedState);

        var response = await this.service.GetEntityAsync(new GetByIdRequest<Guid> { Id = id });

        response.Entity.ShouldBe(expectedState);
        this.permissionServiceMock.Verify(
            p => p.ValidateCurrentUserPermissionAsync(
                PermissionType.System_ManageSettings,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetEntity_WhenStateDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbState, State>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((State?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.GetEntityAsync(
            new GetByIdRequest<Guid> { Id = Guid.NewGuid() }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task PostEntity_WhenStateNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var state = this.CreateState();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PostEntityAsync(
            new CreateOrUpdateRequest<State> { Entity = state }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_State_DuplicateName);
    }

    [Test]
    public async Task PostEntity_WhenValid_PersistsStateAsync()
    {
        var state = this.CreateState();
        var insertedState = default(TbState);

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.InsertAndSaveAsync(
                It.IsAny<TbState>(),
                It.IsAny<CancellationToken>()))
            .Callback<TbState, CancellationToken>((entity, _) => insertedState = entity)
            .Returns(Task.CompletedTask);

        var response = await this.service.PostEntityAsync(new CreateOrUpdateRequest<State> { Entity = state });

        response.Id.ShouldBe(state.Id!.Value);
        insertedState.ShouldNotBe(null);
        insertedState!.CountryId.ShouldBe(state.CountryId);

        this.databaseRepositoryMock.Verify(
            x => x.InsertAndSaveAsync(
                It.Is<TbState>(tb => tb.Id == state.Id && tb.CountryId == state.CountryId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PutEntity_WhenStateNameAlreadyExists_ThrowsDuplicateNameAsync()
    {
        var state = this.CreateState();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<State> { Entity = state }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.General_State_DuplicateName);
    }

    [Test]
    public async Task PutEntity_WhenStateDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var state = this.CreateState();

        this.databaseRepositoryMock
            .Setup(x => x.HasAnyAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbState?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.PutEntityAsync(
            new CreateOrUpdateRequest<State> { Entity = state }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenStateDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        var id = Guid.NewGuid();

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TbState?)null);

        var exception = await AssertAppExceptionAsync(() => this.service.DeleteEntityAsync(
            new DeleteRequest<Guid> { Id = id }));

        exception.ExceptionCode.ShouldBe(ExceptionCode.EntityNotFound);
    }

    [Test]
    public async Task DeleteEntity_WhenStateExists_RemovesStateAsync()
    {
        var entity = new TbState { Id = Guid.NewGuid() };

        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbState>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
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

    private State CreateState()
    {
        return new State
        {
            Id = Guid.NewGuid(),
            Name = this.faker.Address.State(),
            Code = this.faker.Address.StateAbbr(),
            CountryId = Guid.NewGuid()
        };
    }
}
