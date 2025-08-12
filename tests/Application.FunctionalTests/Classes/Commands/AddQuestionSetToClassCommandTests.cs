using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class AddQuestionSetToClassCommandTests : BaseTestFixture
{


    [Test]
    public async Task ShouldRequireValidQuestionSetId()
    {
        var userId = await RunAsDefaultUserAsync();

        var classEntity = new Class { Name = "Test Class" };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = classEntity.Id,
            QuestionSetId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = Guid.NewGuid(),
            QuestionSetId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_NOTFOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserIsNotLecturerOrOwner()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var questionSet = new QuestionSet { Name = "Test Question Set", CreatedBy = ownerId, VisibilityMode = QuestionSetVisibilityMode.Public };
        await AddAsync(questionSet);

        await RunAsDefaultUserAsync(); 

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = classEntity.Id,
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS);
    }

    [Test]
    public async Task ShouldThrowErrorWhenQuestionSetNotFound()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = classEntity.Id,
            QuestionSetId = Guid.NewGuid() 
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenQuestionSetAlreadyInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var questionSet = new QuestionSet { Name = "Test Question Set", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        await AddAsync(questionSet);

        await AddAsync(new ClassQuestionSet { ClassId = classEntity.Id, QuestionSetId = questionSet.Id });

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = classEntity.Id,
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_ALREADY_IN_CLASS);
    }

    [Test]
    public async Task ShouldThrowErrorWhenNotHavePermissionToAddPrivateQuestionSet()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var privateQuestionSet = new QuestionSet { Name = "Private Question Set", CreatedBy = Guid.NewGuid(), VisibilityMode = QuestionSetVisibilityMode.Private };
        await AddAsync(privateQuestionSet);

        await RunAsDefaultUserAsync(); 

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = classEntity.Id,
            QuestionSetId = privateQuestionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS);
    }

    [Test]
    public async Task ShouldAddQuestionSetToClassSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var questionSet = new QuestionSet { Name = "Test Question Set", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        await AddAsync(questionSet);

        var command = new AddQuestionSetToClassCommand
        {
            ClassId = classEntity.Id,
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(command);

        result.Should().Be(questionSet.Id);

        var classQuestionSet = await FindAsync<ClassQuestionSet>(classEntity.Id, questionSet.Id);
        classQuestionSet.Should().NotBeNull();
        classQuestionSet!.ClassId.Should().Be(classEntity.Id);
        classQuestionSet.QuestionSetId.Should().Be(questionSet.Id);
    }
}
