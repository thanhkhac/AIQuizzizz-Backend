using CleanArchitectureBase.Application.Command.UnitTests.TestDataUltils;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class GetTestFromQuestionSetQueryTests : BaseTestFixture
{

    //normal
    [Test]
    public async Task ShouldReturnRandomQuestions_WhenValidRequest()
    {
        var userId = await RunAsUserWithPlanAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId,
            QuestionCount = 5
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.MultipleChoice,
            Score = 1,
            DataJson = QuestionJsonTestData.MultipleChoiceDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q2 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q2",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = QuestionJsonTestData.ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q3 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q3",
            Type = QuestionType.MultipleChoice,
            Score = 1,
            DataJson = QuestionJsonTestData.MultipleChoiceDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);
        await AddAsync(q2);
        await AddAsync(q3);

        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = questionSet.Id,
            NumberOfQuestion = 2,
            QuestionTypes = new List<string>
            {
                "MultipleChoice",
                "ShortText"
            }
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }


    //abnormal
    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public async Task ShouldThrowError_WhenNumberOfQuestionIsZeroOrLess(int numberOfQuestion)
    {
        await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = GetUserId()!.Value
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = GetUserId()!.Value,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = questionSet.Id,
            NumberOfQuestion = numberOfQuestion,
            QuestionTypes = new List<string>
            {
                "ShortText"
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionTypesIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = GetUserId()!.Value
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = GetUserId()!.Value,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = questionSet.Id,
            NumberOfQuestion = 1,
            QuestionTypes = new List<string>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionTypesContainInvalidType()
    {
        await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = GetUserId()!.Value
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = GetUserId()!.Value,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = questionSet.Id,
            NumberOfQuestion = 1,
            QuestionTypes = new List<string>
            {
                "InvalidType"
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = Guid.NewGuid(),
            NumberOfQuestion = 1,
            QuestionTypes = new List<string>
            {
                "ShortText"
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotViewQuestionSet()
    {
        var ownerId = await RunAsUserWithPlanAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var userId = await RunAsUserWithPlanAsync();
        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = questionSet.Id,
            NumberOfQuestion = 1,
            QuestionTypes = new List<string>
            {
                "ShortText"
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET);
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


        var query = new GetTestFromQuestionSetQuery
        {
            QuestionSetId = questionSet.Id,
            NumberOfQuestion = 1,
            QuestionTypes = new List<string>
            {
                "ShortText"
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
