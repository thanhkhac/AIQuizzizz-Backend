using CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;
using CleanArchitectureBase.Application.Command.UnitTests.TestDataUltils;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Commands;

using static Testing;

public class UpdateTestTemplateCommandTests : BaseTestFixture
{
    private static CreateUpdateQuestionDto ValidShortText() => new CreateUpdateQuestionDto
    {
        Type = "ShortText",
        QuestionText = "Câu hỏi hợp lệ",
        Score = 1,
        ShortAnswer = "Đáp án"
    };

    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = Guid.Empty,
            Name = "Updated Name",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireName()
    {
        var userId = await RunAsDefaultUserAsync();

        var testTemplate = new TestTemplate { Name = "Test Template" };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = "",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireNameNotTooLong()
    {
        var userId = await RunAsDefaultUserAsync();

        var testTemplate = new TestTemplate { Name = "Test Template" };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = new string('a', 201),
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireAtLeastOneQuestion()
    {
        var userId = await RunAsDefaultUserAsync();

        var testTemplate = new TestTemplate { Name = "Test Template" };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = "Updated Name",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireNotTooManyQuestions()
    {
        var userId = await RunAsDefaultUserAsync();

        var testTemplate = new TestTemplate { Name = "Test Template" };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var questions = new List<CreateUpdateQuestionDto>();
        for (int i = 0; i < 101; i++)
        {
            questions.Add(ValidShortText());
        }

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = "Updated Name",
            CreateUpdateQuestions = questions
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = Guid.NewGuid(),
            Name = "Updated Name",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }
    
    [Test]
    [TestCaseSource(typeof(CreateQuestionSetTests),nameof(CreateQuestionSetTests.InvalidQuestions))]
    public async Task ShouldThrowErrorInvalidQuestion(CreateUpdateQuestionDto invalidQuestion)
    {
        await RunAsDefaultUserAsync();
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = new string('a', 100),
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { invalidQuestion }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToEditTestTemplate()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without edit permission

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = "Updated Name",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    }

    [Test]
    public async Task ShouldUpdateTestTemplateSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var testTemplate = new TestTemplate { Name = "Original Name", Description = "Original Description", CreatedBy = userId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var existingQuestion = new Question { QuestionText = "Existing Q", Type = QuestionType.ShortText, Score = 1, DataJson = QuestionJsonTestData.ShortTextDataJson };
        await AddAsync(existingQuestion);
        await AddAsync(new TestTemplateQuestion { TestTemplateId = testTemplate.Id, QuestionId = existingQuestion.Id });

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = "Updated Name",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                new() { QuestionId = existingQuestion.Id, Type = "ShortText", QuestionText = "Updated Existing Q", Score = 2, ShortAnswer = "Updated A" },
                ValidShortText() // Add new question
            },
            DeleteQuestionIds = new List<Guid>()
        };

        var result = await SendAsync(command);

        result.Should().Be(testTemplate.Id);

        var updatedTestTemplate = (await QueryListAsync<TestTemplate>(x => x.Where(tt => tt.Id == testTemplate.Id))).FirstOrDefault();
        updatedTestTemplate.Should().NotBeNull();
        updatedTestTemplate!.Name.Should().Be("Updated Name");

        var questions = await QueryListAsync<Question>(x => x.Where(q => q.CreatedBy == userId && q.IsDeleted == false));
        questions.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public async Task ShouldDeleteQuestionsSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var testTemplate = new TestTemplate { Name = "Original Name", CreatedBy = userId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var questionToDelete = new Question { QuestionText = "Q to Delete", Type = QuestionType.ShortText, Score = 1, DataJson = QuestionJsonTestData.ShortTextDataJson };
        var questionToKeep = new Question { QuestionText = "Q to Keep", Type = QuestionType.ShortText, Score = 1, DataJson = QuestionJsonTestData.ShortTextDataJson };
        await AddAsync(questionToDelete);
        await AddAsync(questionToKeep);

        await AddAsync(new TestTemplateQuestion { TestTemplateId = testTemplate.Id, QuestionId = questionToDelete.Id });
        await AddAsync(new TestTemplateQuestion { TestTemplateId = testTemplate.Id, QuestionId = questionToKeep.Id });

        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            Name = "Updated Name",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto>
            {
                new() { QuestionId = questionToKeep.Id, Type = "ShortText", QuestionText = "Updated Q to Keep", Score = 2, ShortAnswer = "Updated A" }
            },
            DeleteQuestionIds = new List<Guid> { questionToDelete.Id }
        };

        var result = await SendAsync(command);

        result.Should().Be(testTemplate.Id);

        var questions = await QueryListAsync<Question>(x => x.Where(q => q.CreatedBy == userId && q.IsDeleted == false));
        questions.Should().HaveCountGreaterThan(0); 

        var testTemplateQuestions = await QueryListAsync<TestTemplateQuestion>(x => x.Include(x => x.Question).Where(ttq => ttq.TestTemplateId == testTemplate.Id && ttq.Question!.IsDeleted == false));
        testTemplateQuestions.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new UpdateTestTemplateCommand
        {
            TestTemplateId = Guid.NewGuid(),
            Name = "Test",
            CreateUpdateQuestions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
