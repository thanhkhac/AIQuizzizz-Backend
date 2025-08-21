using CleanArchitectureBase.Application.Classes;
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

namespace CleanArchitectureBase.Application.Command.UnitTests.Tests.Queries;

[TestFixture]
public class GetTestDetailQueryTests : BaseTestFixture
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
            Name = "Bài kiểm tra chi tiết",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddSeconds(1), // Đã bắt đầu
            EndTime = DateTime.UtcNow.AddSeconds(2),    // Chưa kết thúc
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
    public async Task ShouldGetTestDetailSuccessfully()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        var query = new GetTestDetailQuery
        {
            TestId = testId
        };
        await Task.Delay(2000);
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.TestId.Should().Be(testId);
        result.Name.Should().Be("Bài kiểm tra chi tiết");
        result.ClassId.Should().Be(classId);
        result.TimeLimit.Should().Be(60);
        result.QuestionCount.Should().Be(1);
        result.Questions.Should().NotBeEmpty();
        result.Questions.Count.Should().Be(1);
    }

    [Test]
    public async Task ShouldRequireValidTestId()
    {
        await RunAsDefaultUserAsync();
        var query = new GetTestDetailQuery
        {
            TestId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetTestDetailQuery
        {
            TestId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_NOT_FOUND);
    }


    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
  
        
        var query = new GetTestDetailQuery
        {
            TestId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
