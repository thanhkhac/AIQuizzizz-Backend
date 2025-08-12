using System.ComponentModel.DataAnnotations;
using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Queries;

using static Testing;

public class SearchClassQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidPageNumber()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchClassQuery
        {
            PageNumber = 0,
            PageSize = 10
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidPageSize()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchClassQuery
        {
            PageNumber = 1,
            PageSize = 0
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);

        query.PageSize = 101;
        ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidShareMode()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchClassQuery
        {
            PageNumber = 1,
            PageSize = 10,
            ShareMode = "InvalidMode"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldReturnClassesForCurrentUser()
    {
        var userId = await RunAsDefaultUserAsync();
        var userId2 = await RunAsDefaultUserAsync(2);

        var class1 = new Class { Name = "Class A", CreatedBy = userId };
        var class2 = new Class { Name = "Class B", CreatedBy = userId2 }; // Another user's class
        var class3 = new Class { Name = "Class C", CreatedBy = userId };
        await AddAsync(class1);
        await AddAsync(class2);
        await AddAsync(class3);

        await AddAsync(new ClassUser { UserId = userId, ClassId = class1.Id, ShareMode = ClassShareMode.Owner });
        await AddAsync(new ClassUser { UserId = userId2, ClassId = class2.Id, ShareMode = ClassShareMode.Owner });
        await AddAsync(new ClassUser { UserId = userId, ClassId = class3.Id, ShareMode = ClassShareMode.Student });
        await RunAsDefaultUserAsync();
        var query = new SearchClassQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Class A and Class C
        result.Items.Should().Contain(c => c.Name == "Class A");
        result.Items.Should().Contain(c => c.Name == "Class C");
        result.Items.Should().NotContain(c => c.Name == "Class B");
    }

    [Test]
    public async Task ShouldFilterClassesByName()
    {
        var userId = await RunAsDefaultUserAsync();

        var class1 = new Class { Name = "Math Class", CreatedBy = userId };
        var class2 = new Class { Name = "Science Class", CreatedBy = userId };
        var class3 = new Class { Name = "History Class", CreatedBy = userId };
        await AddAsync(class1);
        await AddAsync(class2);
        await AddAsync(class3);

        await AddAsync(new ClassUser { UserId = userId, ClassId = class1.Id, ShareMode = ClassShareMode.Owner });
        await AddAsync(new ClassUser { UserId = userId, ClassId = class2.Id, ShareMode = ClassShareMode.Owner });
        await AddAsync(new ClassUser { UserId = userId, ClassId = class3.Id, ShareMode = ClassShareMode.Owner });

        var query = new SearchClassQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Name = "math"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Math Class");
    }

    [Test]
    public async Task ShouldFilterClassesByShareMode()
    {
        var userId = await RunAsDefaultUserAsync();

        var class1 = new Class { Name = "Owner Class", CreatedBy = userId };
        var class2 = new Class { Name = "Student Class", CreatedBy = Guid.NewGuid() };
        var class3 = new Class { Name = "Teacher Class", CreatedBy = Guid.NewGuid() };
        await AddAsync(class1);
        await AddAsync(class2);
        await AddAsync(class3);

        await AddAsync(new ClassUser { UserId = userId, ClassId = class1.Id, ShareMode = ClassShareMode.Owner });
        await AddAsync(new ClassUser { UserId = userId, ClassId = class2.Id, ShareMode = ClassShareMode.Student });
        await AddAsync(new ClassUser { UserId = userId, ClassId = class3.Id, ShareMode = ClassShareMode.Teacher });

        var query = new SearchClassQuery
        {
            PageNumber = 1,
            PageSize = 10,
            ShareMode = "Student"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Student Class");
    }

    [Test]
    public async Task ShouldReturnPaginatedResults()
    {
        var userId = await RunAsDefaultUserAsync();

        for (int i = 0; i < 15; i++)
        {
            var classEntity = new Class { Name = $"Class {i}", CreatedBy = userId };
            await AddAsync(classEntity);
            await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });
        }

        var query = new SearchClassQuery
        {
            PageNumber = 2,
            PageSize = 5
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new SearchClassQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
