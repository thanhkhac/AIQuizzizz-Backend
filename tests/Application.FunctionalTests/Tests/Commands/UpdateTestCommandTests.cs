using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Tests.Commands;

[TestFixture]
public class UpdateTestCommandTests : BaseTestFixture
{
    private async Task<Guid> CreateClassAndGetId()
    {
        var classCommand = new CreateClassCommand
        {
            Name = "Lớp kiểm tra cho test",
            Topic = "Test"
        };
        return await SendAsync(classCommand);
    }

    private async Task<Guid> CreateTestAndGetId(Guid classId)
    {
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra ban đầu",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2),
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 1,
            MaxAttempt = 1,
            PassingScore = 5,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        return await SendAsync(command);
    }

    private CreateUpdateQuestionDto GetValidQuestion()
    {
        return new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            QuestionText = "Câu hỏi trắc nghiệm?",
            Score = 1,
            MultipleChoices = new List<CreateMultipleChoiceDto>
            {
                new CreateMultipleChoiceDto { Text = "A", IsAnswer = true },
                new CreateMultipleChoiceDto { Text = "B", IsAnswer = false }
            }
        };
    }

    [Test]
    public async Task ShouldUpdateTestWithValidData()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        var updateCommand = new UpdateTestCommand
        {
            TestId = testId,
            Name = "Bài kiểm tra đã cập nhật",
            TimeLimit = 90,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            GradeAttemptMethod = "HighestScore",
            GradeQuestionMethod = "AllOrNothing",
            IsShowCorrectAnswerInReview = false,
            IsAllowReviewAfterSubmit = false,
            MaxAttempt = 2,
            NumberOfShuffles = 2,
            PassingScore = 7,
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { GetValidQuestion() },
            DeleteQuestionIds = new List<Guid>()
        };

        var updatedTestId = await SendAsync(updateCommand);
        var test = (await QueryListAsync<Test>(x => x.Where(y => y.Id == updatedTestId))).FirstOrDefault();
        
        test.Should().NotBeNull();
        test!.Name.Should().Be(updateCommand.Name);
        test.TimeLimit.Should().Be(updateCommand.TimeLimit);
        test.MaxAttempt.Should().Be(updateCommand.MaxAttempt);
        test.PassingScore.Should().Be(updateCommand.PassingScore);
        test.GradeAttemptMethod.ToString().Should().Be(updateCommand.GradeAttemptMethod);
        test.GradeQuestionMethod.ToString().Should().Be(updateCommand.GradeQuestionMethod);
        test.IsShowCorrectAnswerInReview.Should().Be(updateCommand.IsShowCorrectAnswerInReview);
        test.IsAllowReviewAfterSubmit.Should().Be(updateCommand.IsAllowReviewAfterSubmit);
    }

    [Test]
    public async Task ShouldRequireValidTestId()
    {
        await RunAsUserWithPlanAsync();
        var updateCommand = new UpdateTestCommand
        {
            TestId = Guid.Empty,
            Name = "Bài kiểm tra đã cập nhật",
            TimeLimit = 90,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            GradeAttemptMethod = "HighestScore",
            GradeQuestionMethod = "AllOrNothing",
            IsShowCorrectAnswerInReview = false,
            IsAllowReviewAfterSubmit = false,
            MaxAttempt = 2,
            NumberOfShuffles = 2,
            PassingScore = 7,
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { GetValidQuestion() },
            DeleteQuestionIds = new List<Guid>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(updateCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        var updateCommand = new UpdateTestCommand
        {
            TestId = testId,
            Name = "",
            TimeLimit = 90,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            GradeAttemptMethod = "HighestScore",
            GradeQuestionMethod = "AllOrNothing",
            IsShowCorrectAnswerInReview = false,
            IsAllowReviewAfterSubmit = false,
            MaxAttempt = 2,
            NumberOfShuffles = 2,
            PassingScore = 7,
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { GetValidQuestion() },
            DeleteQuestionIds = new List<Guid>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(updateCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestNotFound()
    {
        await RunAsUserWithPlanAsync();
        var updateCommand = new UpdateTestCommand
        {
            TestId = Guid.NewGuid(),
            Name = "Bài kiểm tra không tồn tại",
            TimeLimit = 90,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            GradeAttemptMethod = "HighestScore",
            GradeQuestionMethod = "AllOrNothing",
            IsShowCorrectAnswerInReview = false,
            IsAllowReviewAfterSubmit = false,
            MaxAttempt = 2,
            NumberOfShuffles = 2,
            PassingScore = 7,
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { GetValidQuestion() },
            DeleteQuestionIds = new List<Guid>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(updateCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_NOT_FOUND);
    }

    [Test]
    [TestCaseSource(typeof(CreateQuestionSetTests), nameof(CreateQuestionSetTests.InvalidQuestions))]
    public async Task ShouldThrowErrorWithInvalidQuestions(CreateUpdateQuestionDto invalidQuestion)
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        var updateCommand = new UpdateTestCommand
        {
            TestId = testId,
            Name = "Bài kiểm tra đã cập nhật",
            TimeLimit = 90,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            GradeAttemptMethod = "HighestScore",
            GradeQuestionMethod = "AllOrNothing",
            IsShowCorrectAnswerInReview = false,
            IsAllowReviewAfterSubmit = false,
            MaxAttempt = 2,
            NumberOfShuffles = 2,
            PassingScore = 7,
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { invalidQuestion },
            DeleteQuestionIds = new List<Guid>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(updateCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        Logout();
        
        var updateCommand = new UpdateTestCommand
        {
            TestId = testId,
            Name = "Bài kiểm tra đã cập nhật",
            TimeLimit = 90,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            GradeAttemptMethod = "HighestScore",
            GradeQuestionMethod = "AllOrNothing",
            IsShowCorrectAnswerInReview = false,
            IsAllowReviewAfterSubmit = false,
            MaxAttempt = 2,
            NumberOfShuffles = 2,
            PassingScore = 7,
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { GetValidQuestion() },
            DeleteQuestionIds = new List<Guid>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(updateCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
