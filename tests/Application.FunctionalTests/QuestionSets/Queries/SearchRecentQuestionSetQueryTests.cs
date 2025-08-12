using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class SearchRecentQuestionSetQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnRecentQuestionSetsSortedByLastAccess()
    {
        var userId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS1",
            CreatedBy = userId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS2",
            CreatedBy = userId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var qs3 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS3",
            CreatedBy = userId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(qs1);
        await AddAsync(qs2);
        await AddAsync(qs3);

        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = qs1.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-30)
        });
        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = qs2.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-10)
        });
        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = qs3.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-20)
        });

        var query = new SearchRecentQuestionSetQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        var orderedItems = result.Items.ToList();
        orderedItems.First().Id.Should().Be(qs2.Id);
        orderedItems[1].Id.Should().Be(qs3.Id);
        orderedItems.Last().Id.Should().Be(qs1.Id);
    }

    //normal
    [Test]
    public async Task ShouldHandleDifferentVisibilityModes()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);

        var publicQs = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public QS",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        var privateQs = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Private QS",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        var onlyClassQs = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "OnlyClass QS",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.OnlyClass
        };
        await AddAsync(publicQs);
        await AddAsync(privateQs);
        await AddAsync(onlyClassQs);

        var userId = await RunAsDefaultUserAsync();

        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = privateQs.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        var class1 = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Test Class",
            CreatedBy = ownerId
        };
        await AddAsync(class1);
        await AddAsync(new ClassUser
        {
            ClassId = class1.Id,
            UserId = userId,
            ShareMode = ClassShareMode.Student
        });
        await AddAsync(new ClassQuestionSet
        {
            ClassId = class1.Id,
            QuestionSetId = onlyClassQs.Id
        });

        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = publicQs.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-10)
        });
        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = privateQs.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-5)
        });
        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = onlyClassQs.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-15)
        });

        var query = new SearchRecentQuestionSetQuery
        {
            PageSize = 5,
            PageNumber = 1
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        var orderedItems = result.Items.ToList();
        orderedItems.First().Id.Should().Be(privateQs.Id);
        orderedItems[1].Id.Should().Be(publicQs.Id);
        orderedItems.Last().Id.Should().Be(onlyClassQs.Id);
    }

    //normal
    [Test]
    public async Task ShouldApplyPagination()
    {
        var userId = await RunAsDefaultUserAsync();
        for (int i = 0; i < 10; i++)
        {
            var qs = new QuestionSet
            {
                Id = Guid.NewGuid(),
                Name = $"QS{i}",
                CreatedBy = userId,
                VisibilityMode = QuestionSetVisibilityMode.Public
            };
            await AddAsync(qs);
            await AddAsync(new UserQuestionSetAccessHistory
            {
                UserId = userId,
                QuestionSetId = qs.Id,
                LastAccess = DateTimeOffset.UtcNow.AddMinutes(-i)
            });
        }

        var query = new SearchRecentQuestionSetQuery
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
    public async Task ShouldCalculateCompletedQuestionCount()
    {
        var userId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS1",
            CreatedBy = userId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            QuestionCount = 3
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS2",
            CreatedBy = userId,
            VisibilityMode = QuestionSetVisibilityMode.Public,
            QuestionCount = 2
        };
        await AddAsync(qs1);
        await AddAsync(qs2);

        var q1_1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = qs1.Id,
            QuestionText = "Q1.1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        var q1_2 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = qs1.Id,
            QuestionText = "Q1.2",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        var q1_3 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = qs1.Id,
            QuestionText = "Q1.3",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1_1);
        await AddAsync(q1_2);
        await AddAsync(q1_3);

        var q2_1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = qs2.Id,
            QuestionText = "Q2.1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        var q2_2 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = qs2.Id,
            QuestionText = "Q2.2",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q2_1);
        await AddAsync(q2_2);

        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = qs1.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-10)
        });
        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = qs2.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-5)
        });

        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = q1_1.Id,
            IsCorrect = true
        });
        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = q1_2.Id,
            IsCorrect = true
        });
        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = q2_1.Id,
            IsCorrect = true
        });

        var query = new SearchRecentQuestionSetQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(item => item.Id == qs1.Id && item.CompletedQuestionCount == 2);
        result.Items.Should().Contain(item => item.Id == qs2.Id && item.CompletedQuestionCount == 1);
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenNoRecentAccessHistory()
    {
        await RunAsDefaultUserAsync();
        var query = new SearchRecentQuestionSetQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new SearchRecentQuestionSetQuery();

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    //abnormal
    [Test]
    [TestCase(0)]
    [TestCase(101)]
    public async Task ShouldThrowError_WhenPageSizeIsInvalid(int pageSize)
    {
        await RunAsDefaultUserAsync();
        var query = new SearchRecentQuestionSetQuery
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
        var query = new SearchRecentQuestionSetQuery
        {
            PageNumber = pageNumber
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
}
