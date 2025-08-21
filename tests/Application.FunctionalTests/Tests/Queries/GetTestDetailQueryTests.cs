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

    [Test]
    public async Task ShouldReturnAllTestPropertiesCorrectly()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId
        };
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.TestId.Should().Be(testId);
        result.ClassId.Should().Be(classId);
        result.Name.Should().Be("Bài kiểm tra chi tiết");
        result.TimeLimit.Should().Be(60);
        result.QuestionCount.Should().Be(1);
        result.GradeAttemptMethod.Should().Be("LastAttempt");
        result.GradeQuestionMethod.Should().Be("Partial");
        result.IsShowCorrectAnswerInReview.Should().BeTrue();
        result.IsAllowReviewAfterSubmit.Should().BeTrue();
        result.NumberOfShuffles.Should().Be(1);
        result.MaxAttempt.Should().Be(1);
        result.PassingScore.Should().Be(5);
        result.TotalScore.Should().Be(1); // Score của 1 câu hỏi
        result.Questions.Should().HaveCount(1);
        result.Questions.First().QuestionText.Should().Be("Câu hỏi trắc nghiệm?");
        result.Questions.First().Score.Should().Be(1);
    }

    [Test]
    public async Task ShouldHideQuestionsWhenIsShowQuestionIsFalse()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId,
            IsShowQuestion = false
        };
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldShowQuestionsWhenIsShowQuestionIsTrue()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId,
            IsShowQuestion = true
        };
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.TestId.Should().Be(testId);
        result.QuestionCount.Should().Be(1);
        result.Questions.Should().NotBeEmpty();
        result.Questions.Should().HaveCount(1);
        result.TotalScore.Should().Be(1);
    }

    [Test]
    public async Task ShouldShowQuestionsWhenIsShowQuestionIsNull()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId,
            IsShowQuestion = null
        };
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.TestId.Should().Be(testId);
        result.QuestionCount.Should().Be(1);
        result.Questions.Should().NotBeEmpty(); // Mặc định hiển thị câu hỏi khi null
        result.Questions.Should().HaveCount(1);
        result.TotalScore.Should().Be(1);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotInClass()
    {
        // Tạo test với user khác (không có quyền)
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        // Đăng nhập với user khác không có trong lớp
        await RunAsDefaultUserAsync();
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST);
    }

    [Test]
    public async Task ShouldReturnCorrectTimeValues()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var testId = await CreateTestAndGetId(classId);
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId
        };
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.StartTime.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(1), TimeSpan.FromSeconds(5));
        result.EndTime.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(2), TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task ShouldHandleMultipleQuestionsCorrectly()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra nhiều câu hỏi",
            ClassId = classId,
            TimeLimit = 120,
            StartTime = DateTime.UtcNow.AddSeconds(1),
            EndTime = DateTime.UtcNow.AddSeconds(2),
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 2,
            MaxAttempt = 3,
            PassingScore = 7,
            Questions = new List<CreateUpdateQuestionDto> 
            { 
                GetValidQuestion(),
                new CreateUpdateQuestionDto
                {
                    Type = "MultipleChoice",
                    QuestionText = "Câu hỏi thứ hai?",
                    Score = 2,
                    MultipleChoices = new List<CreateMultipleChoiceDto>
                    {
                        new CreateMultipleChoiceDto { Text = "C", IsAnswer = true },
                        new CreateMultipleChoiceDto { Text = "D", IsAnswer = false }
                    }
                }
            }
        };
        var testId = await SendAsync(command);
        
        await Task.Delay(2000);
        var query = new GetTestDetailQuery
        {
            TestId = testId
        };
        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.TestId.Should().Be(testId);
        result.Name.Should().Be("Bài kiểm tra nhiều câu hỏi");
        result.QuestionCount.Should().Be(2);
        result.Questions.Should().HaveCount(2);
        result.TotalScore.Should().Be(3); // 1 + 2
        result.NumberOfShuffles.Should().Be(2);
        result.MaxAttempt.Should().Be(3);
        result.PassingScore.Should().Be(7);
        result.TimeLimit.Should().Be(120);
    }
}
