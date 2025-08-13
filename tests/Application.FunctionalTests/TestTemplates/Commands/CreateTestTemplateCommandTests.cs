using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Commands;

using static Testing;

public class CreateTestTemplateCommandTests : BaseTestFixture
{
    private static CreateUpdateQuestionDto ValidShortText() => new CreateUpdateQuestionDto
    {
        Type = "ShortText",
        QuestionText = "Câu hỏi hợp lệ",
        Score = 1,
        ShortAnswer = "Đáp án"
    };

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateTestTemplateCommand
        {
            Name = "",
            Questions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireNameNotTooLong()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateTestTemplateCommand
        {
            Name = new string('a', 201),
            Questions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireAtLeastOneQuestion()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateTestTemplateCommand
        {
            Name = "Test Template",
            Questions = new List<CreateUpdateQuestionDto>()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireNotTooManyQuestions()
    {
        await RunAsDefaultUserAsync();

        var questions = new List<CreateUpdateQuestionDto>();
        for (int i = 0; i < 101; i++)
        {
            questions.Add(ValidShortText());
        }

        var command = new CreateTestTemplateCommand
        {
            Name = "Test Template",
            Questions = questions
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldCreateTestTemplateSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateTestTemplateCommand
        {
            Name = "New Test Template",
            Description = "Test Description",
            Questions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var testTemplateId = await SendAsync(command);

        var newTestTemplate = (await QueryListAsync<TestTemplate>(x => x.Where(tt => tt.Id == testTemplateId))).FirstOrDefault();
        newTestTemplate.Should().NotBeNull();
        newTestTemplate!.Name.Should().Be(command.Name);
        newTestTemplate.Description.Should().Be(command.Description);
        newTestTemplate.CreatedBy.Should().Be(userId);

        var testTemplateUser = (await QueryListAsync<TestTemplateUser>(x => x.Where(ttu => ttu.TestTemplateId == testTemplateId && ttu.UserId == userId))).FirstOrDefault();
        testTemplateUser.Should().NotBeNull();
        testTemplateUser!.ShareMode.Should().Be(TestTemplateUserShareMode.Owner);

        var questions = await QueryListAsync<Question>(x => x.Where(q => q.CreatedBy == userId));
        questions.Should().HaveCount(1);
        questions.First().QuestionText.Should().Be(ValidShortText().QuestionText);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new CreateTestTemplateCommand
        {
            Name = "New Test Template",
            Questions = new List<CreateUpdateQuestionDto> { ValidShortText() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
