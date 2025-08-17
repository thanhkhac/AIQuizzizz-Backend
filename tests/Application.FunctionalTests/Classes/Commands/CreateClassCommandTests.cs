using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands
{
    using static Testing;

    public class CreateClassCommandTests : BaseTestFixture
    {
        public static IEnumerable<TestCaseData> InvalidCommands()
        {
            // Missing Name
            yield return new TestCaseData(new CreateClassCommand
            {
                Name = "",
                Topic = "Test Topic"
            }).SetName("Invalid: Name is empty");

            // Name only spaces
            yield return new TestCaseData(new CreateClassCommand
            {
                Name = "   ",
                Topic = "Test Topic"
            }).SetName("Invalid: Name is white space");

            // Name too long
            yield return new TestCaseData(new CreateClassCommand
            {
                Name = new string('A', 201),
                Topic = "Test Topic"
            }).SetName("Invalid: Name is too long");
            
            yield return new TestCaseData(new CreateClassCommand
            {
                Name = new string('A', 10),
                Topic = new string('A', 201),
            }).SetName("Invalid: Topic is too long");
            
            yield return new TestCaseData(new CreateClassCommand
            {
                Name = "Valid Name",
                Topic = "   "
            }).SetName("Invalid: Topic is white space but exceeds allowed length after trim if any");
        }

        [TestCaseSource(nameof(InvalidCommands))]
        public async Task ShouldFailValidation(CreateClassCommand command)
        {
            var userId = await RunAsDefaultUserAsync();

            var ex = await FluentActions.Invoking(() => SendAsync(command))
                .Should().ThrowAsync<ErrorCodeException>();

            ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
        }

        [TestCase(null, TestName = "Topic is null")]
        [TestCase("Test Topic", TestName = "Topic is not null")]
        public async Task ShouldCreateClassSuccessfully(string? topic)
        {
            var userId = await RunAsDefaultUserAsync();

            var command = new CreateClassCommand
            {
                Name = "New Test Class",
                Topic = topic
            };

            var classId = await SendAsync(command);

            var newClass = await FindAsync<Class>(classId);
            newClass.Should().NotBeNull();
            newClass!.Name.Should().Be(command.Name);
            newClass.Topic.Should().Be(topic);
            newClass.CreatedBy.Should().Be(userId);

            var classUser = await FindAsync<ClassUser>(classId, userId);
            classUser.Should().NotBeNull();
            classUser!.ShareMode.Should().Be(ClassShareMode.Owner);
        }

        [Test]
        public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
        {
            var command = new CreateClassCommand
            {
                Name = "New Test Class",
                Topic = "Test Topic"
            };

            var ex = await FluentActions.Invoking(() => SendAsync(command))
                .Should().ThrowAsync<ErrorCodeException>();

            ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
        }
    }
}
