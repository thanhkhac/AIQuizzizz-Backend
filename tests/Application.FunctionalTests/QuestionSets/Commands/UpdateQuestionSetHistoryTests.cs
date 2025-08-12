using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Commands;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;

using static Testing;

public class UpdateQuestionSetHistoryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldUpdateHistory_WhenValidRequest()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        var question2 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q2",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(question1);
        await AddAsync(question2);

        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = question1.Id,
            IsCorrect = false
        });

        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = questionSet.Id,
            Questions = new List<QuestionHistoryUpdate>
            {
                new()
                {
                    QuestionId = question1.Id,
                    IsCorrect = true
                },
                new()
                {
                    QuestionId = question2.Id,
                    IsCorrect = false
                }
            }
        };

        await SendAsync(command);

        var historyQ1 = await FindAsync<UserQuestionSetHistory>(userId, question1.Id);
        historyQ1.Should().NotBeNull();
        historyQ1!.IsCorrect.Should().BeTrue();

        var historyQ2 = await FindAsync<UserQuestionSetHistory>(userId, question2.Id);
        historyQ2.Should().NotBeNull();
        historyQ2!.IsCorrect.Should().BeFalse();
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = Guid.NewGuid(),
            Questions = new List<QuestionHistoryUpdate>
            {
                new()
                {
                    QuestionId = Guid.NewGuid(),
                    IsCorrect = true
                }
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotViewQuestionSet()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(question);

        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = questionSet.Id,
            Questions = new List<QuestionHistoryUpdate>
            {
                new()
                {
                    QuestionId = question.Id,
                    IsCorrect = true
                }
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(questionSet);
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = "{}",
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(question);


        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = questionSet.Id,
            Questions = new List<QuestionHistoryUpdate>
            {
                new()
                {
                    QuestionId = question.Id,
                    IsCorrect = true
                }
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = Guid.Empty,
            Questions = new List<QuestionHistoryUpdate>
            {
                new()
                {
                    QuestionId = Guid.NewGuid(),
                    IsCorrect = true
                }
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionsListIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = GetUserId()!.Value,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = questionSet.Id,
            Questions = new List<QuestionHistoryUpdate>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
}
