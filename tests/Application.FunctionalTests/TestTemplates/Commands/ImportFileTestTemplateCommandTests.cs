using System.Text;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Commands;

using static Testing;

public class ImportFileTestTemplateCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireFileData()
    {
        await RunAsDefaultUserAsync();

        var command = new ImportFileTestTemplateCommand
        {
            FileData = null
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireFileDataStreamNotEmpty()
    {
        await RunAsDefaultUserAsync();

        var command = new ImportFileTestTemplateCommand
        {
            FileData = new FileStreamData { FileName = "test.xlsx", Data = new MemoryStream() }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenFileFormatIsInvalid()
    {
        await RunAsDefaultUserAsync();

        var command = new ImportFileTestTemplateCommand
        {
            FileData = new FileStreamData { FileName = "test.txt", Data = new MemoryStream(Encoding.UTF8.GetBytes("dummy data")) }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.ERROR_FORMAT_FILE);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new ImportFileTestTemplateCommand
        {
            FileData = new FileStreamData { FileName = "test.xlsx", Data = new MemoryStream(Encoding.UTF8.GetBytes("dummy data")) }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
