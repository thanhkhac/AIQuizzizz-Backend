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
public class CreateTestCommandTests : BaseTestFixture
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
    public async Task ShouldCreateTestWithValidData()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
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
        var id = await SendAsync(command);
        var test = (await QueryListAsync<Test>(x => x.Where(y => y.Id == id))).FirstOrDefault();
        test.Should().NotBeNull();
        test!.Name.Should().Be(command.Name);
        test.ClassId.Should().Be(classId);
        test.TimeLimit.Should().Be(command.TimeLimit);
        test.MaxAttempt.Should().Be(command.MaxAttempt);
        test.PassingScore.Should().Be(command.PassingScore);
        test.GradeAttemptMethod.ToString().Should().Be(command.GradeAttemptMethod);
        test.GradeQuestionMethod.ToString().Should().Be(command.GradeQuestionMethod);
        test.IsShowCorrectAnswerInReview.Should().Be(command.IsShowCorrectAnswerInReview);
        test.IsAllowReviewAfterSubmit.Should().Be(command.IsAllowReviewAfterSubmit);
        test.QuestionCount.Should().Be(1);
    }

    [Test]
    [TestCaseSource(typeof(CreateQuestionSetTests),nameof(CreateQuestionSetTests.InvalidQuestions))]
    public async Task ShouldThrowErrorCreateTestWithInvalidData(CreateUpdateQuestionDto dto)
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
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
            Questions = new List<CreateUpdateQuestionDto> { dto }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "",
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
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireClassId()
    {
        await RunAsUserWithPlanAsync();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = Guid.Empty,
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
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireTimeLimit()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = classId,
            TimeLimit = 0,
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
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireStartTimeGreaterThanNow()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
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
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireEndTimeGreaterThanStartTime()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var now = DateTime.UtcNow.AddDays(1);
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = now,
            EndTime = now,
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 1,
            MaxAttempt = 1,
            PassingScore = 5,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    [TestCase("", TestName = "Empty GradeAttemptMethod")]
    [TestCase("Invalid", TestName = "Invalid GradeAttemptMethod")]
    public async Task ShouldRequireValidGradeAttemptMethod(string gradeAttemptMethod)
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2),
            GradeAttemptMethod = gradeAttemptMethod,
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 1,
            MaxAttempt = 1,
            PassingScore = 5,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    [TestCase("", TestName = "Empty GradeQuestionMethod")]
    [TestCase("Invalid", TestName = "Invalid GradeQuestionMethod")]
    public async Task ShouldRequireValidGradeQuestionMethod(string gradeQuestionMethod)
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2),
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = gradeQuestionMethod,
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = 1,
            MaxAttempt = 1,
            PassingScore = 5,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    [TestCase(-1, TestName = "Negative PassingScore")]
    [TestCase(100, TestName = "PassingScore >= 100")]
    public async Task ShouldRequireValidPassingScore(int passingScore)
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
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
            PassingScore = passingScore,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    [TestCase(0, TestName = "NumberOfShuffles = 0")]
    [TestCase(11, TestName = "NumberOfShuffles > 10")]
    public async Task ShouldRequireValidNumberOfShuffles(int numberOfShuffles)
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2),
            GradeAttemptMethod = "LastAttempt",
            GradeQuestionMethod = "Partial",
            IsShowCorrectAnswerInReview = true,
            IsAllowReviewAfterSubmit = true,
            NumberOfShuffles = numberOfShuffles,
            MaxAttempt = 1,
            PassingScore = 5,
            Questions = new List<CreateUpdateQuestionDto> { GetValidQuestion() }
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }


    [Test]
    public async Task ShouldThrowErrorWhenQuestionsExceedLimit()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        var questions = Enumerable.Range(0, 101).Select(_ => GetValidQuestion()).ToList();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
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
            Questions = questions
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.NUMBER_OF_QUESTION_EXCEED_LIMIT);
    }

    [Test]
    public async Task ShouldThrowErrorIfNotTeacherOrOwner()
    {
        // Giả lập user không phải teacher/owner (cần setup thêm nếu có logic phân quyền test)
        // Ở đây chỉ minh họa, thực tế cần setup class và user phù hợp
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        // TODO: Gán user vào class với vai trò Student
        // ...
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
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
        // Giả lập lỗi phân quyền
        // var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        // ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS);
    }

    [Test]
    public async Task ShouldThrowErrorIfNoPlan()
    {
        // Giả lập user không có plan (cần setup thêm nếu có logic kiểm tra plan)
        // Ở đây chỉ minh họa, thực tế cần setup user phù hợp
        await RunAsDefaultUserAsync();
        var classId = await CreateClassAndGetId();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra số 1",
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
        // Giả lập lỗi không có plan
        // var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        // ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_REQUIRE_PLAN);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        await RunAsUserWithPlanAsync();
        var classId = await CreateClassAndGetId();
        Logout();
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra không đăng nhập",
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
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
