using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class JoinClassByCodeCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireCode()
    {
        await RunAsDefaultUserAsync();

        var command = new JoinClassByCodeCommand
        {
            Code = ""
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenCodeNotFoundOrExpired()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new JoinClassByCodeCommand
        {
            Code = "INVALIDCODE"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_CODE_NOT_FOUND);
    }


    [Test]
    public async Task ShouldThrowErrorWhenStudentAlreadyInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class" };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });

        var inviteCode = new ClassInvitation
        {
            ClassId = classEntity.Id,
            Code = "VALIDCODE",
            TimeStart = DateTime.UtcNow.AddMinutes(-10),
            TimeEnd = DateTime.UtcNow.AddMinutes(10),
            IsDeleted = false
        };
        await AddAsync(inviteCode);

        var command = new JoinClassByCodeCommand
        {
            Code = inviteCode.Code
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.STUDENT_ALREADY_EXISTS_IN_CLASS);
    }

    [Test]
    public async Task ShouldJoinClassSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class" };
        await AddAsync(classEntity);

        var inviteCode = new ClassInvitation
        {
            ClassId = classEntity.Id,
            Code = "VALIDCODE",
            TimeStart = DateTime.UtcNow.AddMinutes(-10),
            TimeEnd = DateTime.UtcNow.AddMinutes(10),
            IsDeleted = false
        };
        await AddAsync(inviteCode);

        var command = new JoinClassByCodeCommand
        {
            Code = inviteCode.Code
        };

        await SendAsync(command);

        var classUser = (await QueryListAsync<ClassUser>(x => x.Where(cu => cu.ClassId == classEntity.Id && cu.UserId == userId))).FirstOrDefault();
        classUser.Should().NotBeNull();
        classUser!.ShareMode.Should().Be(ClassShareMode.Student);

        var classInvitationUser = (await QueryListAsync<ClassInvitationUser>(x => x.Where(ci => ci.ClassInvitationId == inviteCode.Id && ci.UserId == userId))).FirstOrDefault();
        classInvitationUser.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new JoinClassByCodeCommand
        {
            Code = "SOMECODE"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
