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

namespace CleanArchitectureBase.Application.Command.UnitTests.Tests.Commands;

[TestFixture]
public class DeleteTestCommandTests : BaseTestFixture
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
            Name = "Bài kiểm tra để xóa",
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
    public async Task ShouldDeleteTestSuccessfully()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        var deleteCommand = new DeleteTestCommand
        {
            TestId = testId
        };

        await SendAsync(deleteCommand);
        
        var test = (await QueryListAsync<Test>(x => x.Where(y => y.Id == testId))).FirstOrDefault();
        test.Should().NotBeNull();
        test!.IsDeleted.Should().BeTrue();
    }

    [Test]
    public async Task ShouldRequireValidTestId()
    {
        await RunAsUserWithPlanAsync();
        var deleteCommand = new DeleteTestCommand
        {
            TestId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(deleteCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestNotFound()
    {
        await RunAsUserWithPlanAsync();
        var deleteCommand = new DeleteTestCommand
        {
            TestId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(deleteCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        Logout();
        
        var deleteCommand = new DeleteTestCommand
        {
            TestId = testId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(deleteCommand)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
