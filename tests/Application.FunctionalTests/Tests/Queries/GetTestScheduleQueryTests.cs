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
public class GetTestScheduleQueryTests : BaseTestFixture
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

    private async Task<Guid> CreateTestAndGetId(Guid classId, DateTime startTime)
    {
        var command = new CreateTestCommand
        {
            Name = "Bài kiểm tra lịch trình",
            ClassId = classId,
            TimeLimit = 60,
            StartTime = startTime,
            EndTime = startTime.AddDays(1),
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
    public async Task ShouldGetTestScheduleSuccessfully()
    {
        await RunAsDefaultUserAsync();
        var classId = await CreateClassAndGetId();
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        
        // Tạo test trong tháng hiện tại
        var testStartTime = new DateTime(currentYear, currentMonth, 15, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, testStartTime);
        
        var query = new GetTestScheduleQuery
        {
            Month = currentMonth,
            Year = currentYear
        };

        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(0);
        
        var firstSchedule = result.First();
        firstSchedule.Date.Should().Be(testStartTime.Date);
        firstSchedule.TestSchedules.Should().NotBeEmpty();
        firstSchedule.TestSchedules.Count.Should().Be(1);
        
        var testSchedule = firstSchedule.TestSchedules.First();
        testSchedule.TestName.Should().Be("Bài kiểm tra lịch trình");
        testSchedule.ClassId.Should().Be(classId);
        testSchedule.TimeStart.Should().Be(testStartTime);
    }

    [Test]
    public async Task ShouldReturnEmptyListWhenNoTestsInMonth()
    {
        await RunAsDefaultUserAsync();
        var classId = await CreateClassAndGetId();
        
        // Tạo test ở tháng khác
        var otherMonth = DateTime.UtcNow.AddMonths(1);
        var testStartTime = new DateTime(otherMonth.Year, otherMonth.Month, 15, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, testStartTime);
        
        var query = new GetTestScheduleQuery
        {
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year
        };

        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldGroupTestsByDate()
    {
        await RunAsDefaultUserAsync();
        var classId = await CreateClassAndGetId();
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        
        // Tạo 2 test cùng ngày
        var testDate = new DateTime(currentYear, currentMonth, 15, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, testDate);
        await CreateTestAndGetId(classId, testDate.AddHours(2));
        
        // Tạo 1 test ngày khác
        var otherDate = new DateTime(currentYear, currentMonth, 20, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, otherDate);
        
        var query = new GetTestScheduleQuery
        {
            Month = currentMonth,
            Year = currentYear
        };

        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.Count.Should().Be(2); // 2 ngày khác nhau
        
        var firstDay = result.First(x => x.Date == testDate.Date);
        firstDay.TestSchedules.Count.Should().Be(2); // 2 test cùng ngày
        
        var secondDay = result.First(x => x.Date == otherDate.Date);
        secondDay.TestSchedules.Count.Should().Be(1); // 1 test ngày khác
    }

    [Test]
    public async Task ShouldReturnCorrectStatusForTests()
    {
        await RunAsDefaultUserAsync();
        var classId = await CreateClassAndGetId();
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        
        // Test chưa bắt đầu (Upcoming)
        var upcomingTestTime = DateTime.UtcNow.AddDays(1);
        var upcomingTest = new DateTime(currentYear, currentMonth, upcomingTestTime.Day, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, upcomingTest);
        
        // Test đang diễn ra (Active)
        var activeTestTime = DateTime.UtcNow.AddHours(-1);
        var activeTest = new DateTime(currentYear, currentMonth, activeTestTime.Day, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, activeTest);
        
        // Test đã kết thúc (Completed)
        var completedTestTime = DateTime.UtcNow.AddDays(-2);
        var completedTest = new DateTime(currentYear, currentMonth, completedTestTime.Day, 10, 0, 0, DateTimeKind.Utc);
        await CreateTestAndGetId(classId, completedTest);
        
        var query = new GetTestScheduleQuery
        {
            Month = currentMonth,
            Year = currentYear
        };

        var result = await SendAsync(query);
        
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        
        // Kiểm tra status của các test
        var allTests = result.SelectMany(x => x.TestSchedules).ToList();
        allTests.Should().Contain(x => x.Status == "Upcoming");
        allTests.Should().Contain(x => x.Status == "Active");
        allTests.Should().Contain(x => x.Status == "Completed");
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        await RunAsDefaultUserAsync();
        Logout();
        
        var query = new GetTestScheduleQuery
        {
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
