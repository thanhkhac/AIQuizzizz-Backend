using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class GetClassesHaveQuestionSetQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnClassesWithQuestionSetStatus_WhenUserIsOwnerOrTeacher()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var class1 = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Class 1",
            CreatedBy = userId
        };
        var class2 = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Class 2",
            CreatedBy = userId
        };
        var class3 = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Class 3",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(class1);
        await AddAsync(class2);
        await AddAsync(class3);

        await AddAsync(new ClassUser
        {
            ClassId = class1.Id,
            UserId = userId,
            ShareMode = ClassShareMode.Owner
        });
        await AddAsync(new ClassUser
        {
            ClassId = class2.Id,
            UserId = userId,
            ShareMode = ClassShareMode.Teacher
        });
        await AddAsync(new ClassUser
        {
            ClassId = class3.Id,
            UserId = userId,
            ShareMode = ClassShareMode.Student
        });
        await AddAsync(new ClassQuestionSet
        {
            ClassId = class1.Id,
            QuestionSetId = questionSet.Id
        });
        var query = new GetClassesHaveQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(dto => dto.ClassId == class1.Id && dto.ClassName == "Class 1" && dto.IsAdded == true);
        result.Should().Contain(dto => dto.ClassId == class2.Id && dto.ClassName == "Class 2" && dto.IsAdded == false);
        result.Should().NotContain(dto => dto.ClassId == class3.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenUserHasNoClasses()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });


        var query = new GetClassesHaveQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenUserIsNotOwnerOrTeacherOfAnyClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var class1 = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Class 1",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(class1);
        await AddAsync(new ClassUser
        {
            ClassId = class1.Id,
            UserId = userId,
            ShareMode = ClassShareMode.Student
        });
        var query = new GetClassesHaveQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetClassesHaveQuestionSetQuery
        {
            QuestionSetId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var query = new GetClassesHaveQuestionSetQuery
        {
            QuestionSetId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(questionSet);

        var query = new GetClassesHaveQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
