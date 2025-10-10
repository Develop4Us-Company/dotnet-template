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
public class StateSummaryServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private StateSummaryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.service = new StateSummaryService(this.databaseRepositoryMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithCountryId_ReturnsStatesForCountryAsync()
    {
        var countryId = Guid.NewGuid();
        var dataset = new List<TbState>
        {
            new () { Id = Guid.NewGuid(), Name = "California", CountryId = countryId },
            new () { Id = Guid.NewGuid(), Name = "Texas", CountryId = countryId },
            new () { Id = Guid.NewGuid(), Name = "Quebec", CountryId = Guid.NewGuid() }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbState, StateSummary>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbState>, IQueryable<TbState>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(state => new StateSummary
                {
                    Id = state.Id,
                    Name = state.Name,
                    CountryId = state.CountryId,
                    CountryName = this.faker.Address.Country()
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<StateSummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new StateSummarySearchRequest { CountryId = countryId });

        response.Summaries.Count.ShouldBe(2);
        response.Summaries.All(summary => summary.CountryId == countryId).ShouldBe(true);
    }

    [Test]
    public async Task GetSummaries_WithSearchText_FiltersByNameOrCodeAsync()
    {
        var countryId = Guid.NewGuid();
        var dataset = new List<TbState>
        {
            new () { Id = Guid.NewGuid(), Name = "Rio de Janeiro", Code = "RJ", CountryId = countryId },
            new () { Id = Guid.NewGuid(), Name = "São Paulo", Code = "SP", CountryId = countryId }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbState, StateSummary>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbState>, IQueryable<TbState>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(state => new StateSummary
                {
                    Id = state.Id,
                    Name = state.Name,
                    CountryId = state.CountryId,
                    CountryName = this.faker.Address.Country()
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<StateSummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new StateSummarySearchRequest { SearchText = "  RJ " });

        response.Summaries.Count.ShouldBe(1);
        var summary = response.Summaries.Single();
        summary.Name.ShouldBe("Rio de Janeiro");
    }

    [Test]
    public async Task GetSummary_WhenStateDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbState, StateSummary>(
                It.IsAny<Func<IQueryable<TbState>, IQueryable<TbState>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((StateSummary?)null);

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
