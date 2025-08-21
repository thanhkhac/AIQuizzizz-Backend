using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Application.Tests.Dto;
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
public class StartAttemptTestCommandTests : BaseTestFixture
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
            Name = "Bài kiểm tra để làm",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddSeconds(1), // Đã bắt đầu
            EndTime = DateTime.UtcNow.AddDays(1),    // Chưa kết thúc
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 1,
            MaxAttempt = 2,
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
    public async Task ShouldStartAttemptSuccessfully()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        var startCommand = new StartAttemptTestCommand
        {
            TestId = testId
        };
        await Task.Delay(2000);
        var result = await SendAsync(startCommand);
        
        result.Should().NotBeNull();
        result.AttemptId.Should().NotBeEmpty();
        result.Questions.Should().NotBeEmpty();
        result.TimeLimit.Should().BeGreaterThan(0);
        result.TimeStart.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }


    [Test]
    public async Task ShouldThrowErrorWhenTestNotFound()
    {
        await RunAsDefaultUserAsync();
        var startCommand = new StartAttemptTestCommand
        {
            TestId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(startCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestNotStarted()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        
        // Tạo test chưa bắt đầu
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra chưa bắt đầu",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddDays(1), // Chưa bắt đầu
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
        var testId = await SendAsync(command);
        
        var startCommand = new StartAttemptTestCommand
        {
            TestId = testId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(startCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_YET_TIME_TO_OPEN_TEST);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestEnded()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        
        // Tạo test đã kết thúc
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra đã kết thúc",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddSeconds(1), // Đã bắt đầu
            EndTime = DateTime.UtcNow.AddSeconds(2),   // Đã kết thúc
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 1,
            MaxAttempt = 1,
            PassingScore = 5,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        var testId = await SendAsync(command);
        
        await Task.Delay(5000);
        
        var startCommand = new StartAttemptTestCommand
        {
            TestId = testId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(startCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenExceedMaxAttempts()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        await Task.Delay(2000);

        // Bắt đầu lần thứ nhất
        var startCommand1 = new StartAttemptTestCommand { TestId = testId };
        await SendAsync(startCommand1);
        
        // Bắt đầu lần thứ hai
        var startCommand2 = new StartAttemptTestCommand { TestId = testId };
        await SendAsync(startCommand2);
        
        // Bắt đầu lần thứ ba (vượt quá MaxAttempt = 2)
        var startCommand3 = new StartAttemptTestCommand { TestId = testId };
        var ex = await FluentActions.Invoking(() => SendAsync(startCommand3)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.MAX_ATTEMPT_IN_THIS_TEST);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        Logout();
        
        var startCommand = new StartAttemptTestCommand
        {
            TestId = testId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(startCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
