using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Queries;

using static Testing;

public class GetInviteStudentCodeQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new GetInviteStudentCodeQuery
        {
            ClassId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new GetInviteStudentCodeQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
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

        var userId = await RunAsDefaultUserAsync(); // Run as a non-owner/lecturer user

        var query = new GetInviteStudentCodeQuery
        {
            ClassId = classEntity.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS);
    }

    [Test]
    public async Task ShouldReturnNullWhenNoActiveInviteCode()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        // Add an expired invite code
        await AddAsync(new ClassInvitation
        {
            ClassId = classEntity.Id,
            Code = "EXPIREDCODE",
            TimeStart = DateTime.UtcNow.AddDays(-2),
            TimeEnd = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        });

        var query = new GetInviteStudentCodeQuery
        {
            ClassId = classEntity.Id
        };

        var result = await SendAsync(query);

        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnActiveInviteCode()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var activeCode = "ACTIVECODE123";
        await AddAsync(new ClassInvitation
        {
            ClassId = classEntity.Id,
            Code = activeCode,
            TimeStart = DateTime.UtcNow.AddDays(-1),
            TimeEnd = DateTime.UtcNow.AddDays(1),
            IsDeleted = false
        });

        var query = new GetInviteStudentCodeQuery
        {
            ClassId = classEntity.Id
        };

        var result = await SendAsync(query);

        result.Should().Be(activeCode);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetInviteStudentCodeQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
