using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class SearchOwnAndSharedQuestionSetQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnOwnQuestionSets_WhenFilterByCreatedByMe()
    {
        var otherUser = await RunAsUserAsync("other@local", "Other1234!", []);
        var questionSet3 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Other User Set",
            CreatedBy = otherUser
        };

        await AddAsync(questionSet3);

        await AddAsync(new QuestionSetUser
        {
            UserId = otherUser,
            QuestionSetId = questionSet3.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var userId = await RunAsDefaultUserAsync();
        var questionSet1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "My Question Set 1",
            CreatedBy = userId
        };
        var questionSet2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "My Question Set 2",
            CreatedBy = userId
        };
        await AddAsync(questionSet1);
        await AddAsync(questionSet2);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet1.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet2.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            FilterBy = "CreatedByMe"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(qs => qs.Id == questionSet1.Id);
        result.Items.Should().Contain(qs => qs.Id == questionSet2.Id);
        result.Items.Should().NotContain(qs => qs.Id == questionSet3.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnSharedQuestionSets_WhenFilterByShareWithMe()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Shared Set 1",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        var questionSet2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Shared Set 2",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet1);
        await AddAsync(questionSet2);
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet1.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet2.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var userId = await RunAsDefaultUserAsync();
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet1.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet2.Id,
            ShareMode = QuestionSetUserShareMode.Editable
        });


        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = questionSet1.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-10)
        });
        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = questionSet2.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-5)
        });

        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            FilterBy = "ShareWithMe"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(qs => qs.Id == questionSet1.Id);
        result.Items.Should().Contain(qs => qs.Id == questionSet2.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnBothOwnAndSharedQuestionSets_WhenNoFilter()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "My Own Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet1);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet1.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var otherOwnerId = await RunAsUserAsync("other@local", "Other1234!", []);
        var questionSet2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Shared Set",
            CreatedBy = otherOwnerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet2);
        await AddAsync(new QuestionSetUser
        {
            UserId = otherOwnerId,
            QuestionSetId = questionSet2.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet2.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        await AddAsync(new UserQuestionSetAccessHistory
        {
            UserId = userId,
            QuestionSetId = questionSet2.Id,
            LastAccess = DateTimeOffset.UtcNow.AddMinutes(-10)
        });

        await RunAsDefaultUserAsync();

        var query = new SearchOwnAndSharedQuestionSetQuery();
        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(qs => qs.Id == questionSet1.Id);
        result.Items.Should().Contain(qs => qs.Id == questionSet2.Id);
    }

    //normal
    [Test]
    public async Task ShouldFilterByName()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Math Quiz",
            CreatedBy = userId
        };
        var questionSet2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Science Test",
            CreatedBy = userId
        };
        await AddAsync(questionSet1);
        await AddAsync(questionSet2);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet1.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet2.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            Name = "Math"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(qs => qs.Id == questionSet1.Id);
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
                Name = $"Question Set {i}",
                CreatedBy = userId
            };
            await AddAsync(qs);
            await AddAsync(new QuestionSetUser
            {
                UserId = userId,
                QuestionSetId = qs.Id,
                ShareMode = QuestionSetUserShareMode.Owner
            });
        }

        var query = new SearchOwnAndSharedQuestionSetQuery
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
    public async Task ShouldSortByRecentAccess()
    {
        var userId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS1",
            CreatedBy = userId
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS2",
            CreatedBy = userId
        };
        await AddAsync(qs1);
        await AddAsync(qs2);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = qs1.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = qs2.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

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

        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            SortBy = "RecentAccess"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.First().Id.Should().Be(qs2.Id);
        result.Items.Last().Id.Should().Be(qs1.Id);
    }

    //normal
    [Test]
    public async Task ShouldSortByNewest()
    {
        var userId = await RunAsDefaultUserAsync();
        var qs1 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS1",
            CreatedBy = userId,
            Created = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        var qs2 = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "QS2",
            CreatedBy = userId,
            Created = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        await AddAsync(qs1);
        await AddAsync(qs2);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = qs1.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = qs2.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            SortBy = "Newest"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.First().Id.Should().Be(qs2.Id);
        result.Items.Last().Id.Should().Be(qs1.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenNoMatchingQuestionSets()
    {
        await RunAsDefaultUserAsync();
        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            Name = "NonExistent"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new SearchOwnAndSharedQuestionSetQuery();

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
        var query = new SearchOwnAndSharedQuestionSetQuery
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
        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            PageNumber = pageNumber
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenFilterByIsInvalid()
    {
        await RunAsDefaultUserAsync();
        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            FilterBy = "InvalidFilter"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenSortByIsInvalid()
    {
        await RunAsDefaultUserAsync();
        var query = new SearchOwnAndSharedQuestionSetQuery
        {
            SortBy = "InvalidSort"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
}
