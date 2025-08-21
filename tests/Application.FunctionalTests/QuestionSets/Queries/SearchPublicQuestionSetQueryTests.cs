using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class SearchPublicQuestionSetQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnPublicQuestionSets()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public Set 1",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public Set 2",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs3 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Private Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(qs1);
        await AddAsync(qs2);
        await AddAsync(qs3);

        var query = new SearchPublicQuestionSetQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(qs => qs.Id == qs1.Id);
        result.Items.Should().Contain(qs => qs.Id == qs2.Id);
        result.Items.Should().NotContain(qs => qs.Id == qs3.Id);
    }

    //normal
    [Test]
    public async Task ShouldFilterByName()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Math Public",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Science Public",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(qs1);
        await AddAsync(qs2);

        var query = new SearchPublicQuestionSetQuery
        {
            Name = "Math"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(qs => qs.Id == qs1.Id);
    }

    //normal
    [Test]
    public async Task ShouldFilterByTagIds()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public Set 1",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public Set 2",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(qs1);
        await AddAsync(qs2);

        var tag1 = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "Tag1"
        };
        var tag2 = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "Tag2"
        };
        await AddAsync(tag1);
        await AddAsync(tag2);

        await AddAsync(new QuestionSetTag
        {
            QuestionSetId = qs1.Id,
            TagId = tag1.Id
        });
        await AddAsync(new QuestionSetTag
        {
            QuestionSetId = qs2.Id,
            TagId = tag2.Id
        });

        var query = new SearchPublicQuestionSetQuery
        {
            TagIds = new List<Guid>
            {
                tag1.Id
            }
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(qs => qs.Id == qs1.Id);
    }

    //normal
    [Test]
    public async Task ShouldFilterByNameAndTagIds()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Math Public",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Science Public",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(qs1);
        await AddAsync(qs2);

        var tag1 = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "MathTag"
        };
        var tag2 = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "ScienceTag"
        };
        await AddAsync(tag1);
        await AddAsync(tag2);

        await AddAsync(new QuestionSetTag
        {
            QuestionSetId = qs1.Id,
            TagId = tag1.Id
        });
        await AddAsync(new QuestionSetTag
        {
            QuestionSetId = qs2.Id,
            TagId = tag2.Id
        });

        var query = new SearchPublicQuestionSetQuery
        {
            Name = "Math",
            TagIds = new List<Guid>
            {
                tag1.Id
            }
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(qs => qs.Id == qs1.Id);
    }

    //normal
    [Test]
    public async Task ShouldApplyPagination()
    {
        var ownerId = await RunAsDefaultUserAsync();
        for (int i = 0; i < 10; i++)
        {
            var qs = new QuestionSet
            {
                Id = Guid.NewGuid(),
                Name = $"Public Set {i}",
                CreatedBy = ownerId,
                VisibilityMode = QuestionSetVisibilityMode.Public
            };
            await AddAsync(qs);
        }

        var query = new SearchPublicQuestionSetQuery
        {
            PageNumber = 2,
            PageSize = 5
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.TotalCount.Should().Be(10);
    }

    //normal
    [Test]
    public async Task ShouldSortByNewest()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS1",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            Created = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS2",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            Created = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        await AddAsync(qs1);
        await AddAsync(qs2);

        var query = new SearchPublicQuestionSetQuery
        {
            SortBy = "Newest"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.First().Id.Should().Be(qs2.Id);
        result.Items.Last().Id.Should().Be(qs1.Id);
    }

    private double CalculateBayesianAverage(int ratingCount, double ratingAverage, double globalAverage, int m = 5)
    {
        if ((ratingCount + m) == 0) return 0;
        return (ratingCount / (double)(ratingCount + m)) * ratingAverage +
               (m / (double)(ratingCount + m)) * globalAverage;
    }

    //normal
    [Test]
    public async Task ShouldSortByRating()
    {
        var ownerId = await RunAsDefaultUserAsync();

        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS1",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            RatingCount = 10,
            RatingSum = 40,
            RatingAverage = 4.0
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS2",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            RatingCount = 20,
            RatingSum = 60,
            RatingAverage = 3.0
        };
        var qs3 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS3",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            RatingCount = 5,
            RatingSum = 25,
            RatingAverage = 5.0
        };
        var qs4 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS4",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            RatingCount = 0,
            RatingSum = 0,
            RatingAverage = 0.0
        };

        await AddAsync(qs1);
        await AddAsync(qs2);
        await AddAsync(qs3);
        await AddAsync(qs4);

        var ratedQuestionSets = new List<QuestionSet>
        {
            qs1,
            qs2,
            qs3
        };
        double globalAverage = ratedQuestionSets.Sum(q => q.RatingSum) / (double)ratedQuestionSets.Sum(q => q.RatingCount);

        var bayesianQs1 = CalculateBayesianAverage(qs1.RatingCount, qs1.RatingAverage, globalAverage);
        var bayesianQs2 = CalculateBayesianAverage(qs2.RatingCount, qs2.RatingAverage, globalAverage);
        var bayesianQs3 = CalculateBayesianAverage(qs3.RatingCount, qs3.RatingAverage, globalAverage);
        var bayesianQs4 = CalculateBayesianAverage(qs4.RatingCount, qs4.RatingAverage, globalAverage);
        var expectedOrder = new List<(Guid Id, double BayesianScore)>
        {
            (qs1.Id, bayesianQs1),
            (qs2.Id, bayesianQs2),
            (qs3.Id, bayesianQs3),
            (qs4.Id, bayesianQs4)
        }.OrderByDescending(x => x.BayesianScore).ToList();

        var query = new SearchPublicQuestionSetQuery
        {
            SortBy = "Rating"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCountGreaterThan(0);
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenNoMatchingPublicQuestionSets()
    {
        await RunAsDefaultUserAsync();
        var query = new SearchPublicQuestionSetQuery
        {
            Name = "NonExistent"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    //abnormal
    [Test]
    [TestCase(0)]
    [TestCase(101)]
    public async Task ShouldThrowError_WhenPageSizeIsInvalid(int pageSize)
    {
        await RunAsDefaultUserAsync();
        var query = new SearchPublicQuestionSetQuery
        {
            PageSize = pageSize
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public async Task ShouldThrowError_WhenPageNumberIsInvalid(int pageNumber)
    {
        await RunAsDefaultUserAsync();
        var query = new SearchPublicQuestionSetQuery
        {
            PageNumber = pageNumber
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenSortByIsInvalid()
    {
        await RunAsDefaultUserAsync();
        var query = new SearchPublicQuestionSetQuery
        {
            SortBy = "InvalidSort"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
}
