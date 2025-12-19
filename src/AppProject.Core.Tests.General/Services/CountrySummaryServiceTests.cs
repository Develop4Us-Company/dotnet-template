using System;
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
public class CountrySummaryServiceTests
{
    private Faker faker = null!;
    private Mock<IDatabaseRepository> databaseRepositoryMock = null!;
    private CountrySummaryService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.faker = new Faker();
        this.databaseRepositoryMock = new Mock<IDatabaseRepository>();
        this.service = new CountrySummaryService(this.databaseRepositoryMock.Object);
    }

    [Test]
    public async Task GetSummaries_WithTrimmedSearchText_FiltersByNameOrCodeAsync()
    {
        var targetCountry = new TbCountry
        {
            Id = Guid.NewGuid(),
            Name = "Brazil",
            Code = "BR"
        };

        var dataset = new List<TbCountry>
        {
            targetCountry,
            new () { Id = Guid.NewGuid(), Name = "Argentina", Code = "AR" },
            new () { Id = Guid.NewGuid(), Name = "Chile", Code = "CL" }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbCountry, CountrySummary>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(country => new CountrySummary
                {
                    Id = country.Id,
                    Name = country.Name
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<CountrySummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new SearchRequest { SearchText = "  Bra " });

        response.Summaries.Count.ShouldBe(1);
        var summary = response.Summaries.Single();
        summary.Id.ShouldBe(targetCountry.Id);
    }

    [Test]
    public async Task GetSummaries_WithTakeValue_RespectsOrderingAndLimitAsync()
    {
        var dataset = new List<TbCountry>
        {
            new () { Id = Guid.NewGuid(), Name = "United States", Code = "US" },
            new () { Id = Guid.NewGuid(), Name = "Brazil", Code = "BR" },
            new () { Id = Guid.NewGuid(), Name = "Canada", Code = "CA" }
        };

        this.databaseRepositoryMock
            .Setup(x => x.GetByConditionAsync<TbCountry, CountrySummary>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>, CancellationToken>((query, _) =>
            {
                var filtered = query(dataset.AsQueryable()).ToList();
                var summaries = filtered.Select(country => new CountrySummary
                {
                    Id = country.Id,
                    Name = country.Name
                }).ToList();

                return Task.FromResult<IReadOnlyCollection<CountrySummary>>(summaries);
            });

        var response = await this.service.GetSummariesAsync(new SearchRequest { Take = 2 });

        response.Summaries.Count.ShouldBe(2);
        response.Summaries.First().Name.ShouldBe("Brazil");
        response.Summaries.Last().Name.ShouldBe("Canada");
    }

    [Test]
    public async Task GetSummary_WhenCountryDoesNotExist_ThrowsEntityNotFoundAsync()
    {
        this.databaseRepositoryMock
            .Setup(x => x.GetFirstOrDefaultAsync<TbCountry, CountrySummary>(
                It.IsAny<Func<IQueryable<TbCountry>, IQueryable<TbCountry>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountrySummary?)null);

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
