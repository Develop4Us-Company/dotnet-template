using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Exceptions;
using AppProject.Models;
using Bogus;
using Moq;
using Shouldly;

namespace AppProject.Core.Tests.General.Services;

[TestFixture]
public class CitySummaryServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private CitySummaryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.service = new CitySummaryService(this.databaseRepositoryMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithStateId_ReturnsCitiesForStateAsync()
    {
        var stateId = Guid.NewGuid();
        var dataset = new List<TbCity>
        {
            new () { Id = Guid.NewGuid(), Name = "Rio de Janeiro", StateId = stateId },
            new () { Id = Guid.NewGuid(), Name = "Niterói", StateId = stateId },
            new () { Id = Guid.NewGuid(), Name = "Lisbon", StateId = Guid.NewGuid() }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbCity, CitySummary>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbCity>, IQueryable<TbCity>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(city => new CitySummary
                {
                    Id = city.Id,
                    Name = city.Name,
                    StateId = city.StateId,
                    CountryId = Guid.NewGuid(),
                    StateName = this.faker.Address.State(),
                    CountryName = this.faker.Address.Country()
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<CitySummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new CitySummarySearchRequest { StateId = stateId });

        response.Summaries.Count.ShouldBe(2);
        response.Summaries.All(summary => summary.StateId == stateId).ShouldBe(true);
    }

    [Test]
    public async Task GetSummaries_WithSearchText_FiltersByNameOrCodeAsync()
    {
        var dataset = new List<TbCity>
        {
            new () { Id = Guid.NewGuid(), Name = "São Paulo", Code = "SAO", StateId = Guid.NewGuid() },
            new () { Id = Guid.NewGuid(), Name = "Campinas", Code = "CPQ", StateId = Guid.NewGuid() }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbCity, CitySummary>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbCity>, IQueryable<TbCity>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(city => new CitySummary
                {
                    Id = city.Id,
                    Name = city.Name,
                    StateId = city.StateId,
                    CountryId = Guid.NewGuid(),
                    StateName = this.faker.Address.State(),
                    CountryName = this.faker.Address.Country()
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<CitySummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new CitySummarySearchRequest { SearchText = " São " });

        response.Summaries.Count.ShouldBe(1);
        var summary = response.Summaries.Single();
        summary.Name.ShouldBe("São Paulo");
    }

    [Test]
    public async Task GetSummary_WhenCityDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCity, CitySummary>(
                It.IsAny<Func<IQueryable<TbCity>, IQueryable<TbCity>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CitySummary?)null);

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
