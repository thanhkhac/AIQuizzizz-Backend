using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Domain.Constants;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CleanArchitectureBase.Application.UnitTests.Authentication;

[TestFixture]
public class RegisterUserCommandHandlerTests 
{
    private Mock<IIdentityService> _identityServiceMock;
    private RegisterUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new RegisterUserCommandHandler(_identityServiceMock.Object);
    }

    [Test]
    public async Task Handle_ValidRequest_ReturnsUserId()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "test@example.com", Password = "password" };
        var userId = System.Guid.NewGuid();
        var result = (Result.Success(), userId);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act
        var returnedUserId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        returnedUserId.Should().Be(userId.ToString());
        _identityServiceMock.Verify(x => x.CreateUserAsync(command.Email, command.Password), Times.Once);
    }

    [Test]
    public void Handle_EmailBanned_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "banned@example.com", Password = "password" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.ACCOUNT_EMAIL_BANNED, new[] { "This email address has been banned" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.ACCOUNT_EMAIL_BANNED);
        ex.Errors[ErrorCodes.ACCOUNT_EMAIL_BANNED].Should().Contain("This email address has been banned");
    }

    [Test]
    public void Handle_DuplicateEmail_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "existing@example.com", Password = "password" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.IDENTITY_DUPLICATE_EMAIL, new[] { "Email already exists" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.IDENTITY_DUPLICATE_EMAIL);
        ex.Errors[ErrorCodes.IDENTITY_DUPLICATE_EMAIL].Should().Contain("Email already exists");
    }

    [Test]
    public void Handle_ServerInternalError_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "test@example.com", Password = "password" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.COMMON_SERVER_INTERNAL_ERROR, new[] { "Internal server error occurred" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.COMMON_SERVER_INTERNAL_ERROR);
    }

    [Test]
    public void Handle_InvalidCredentials_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "invalid@example.com", Password = "weak" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.ACCOUNT_INVALID_CREDENTIALS, new[] { "Invalid credentials provided" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.ACCOUNT_INVALID_CREDENTIALS);
    }


    [Test]
    public void Handle_EmailNotVerified_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "unverified@example.com", Password = "password" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.ACCOUNT_EMAIL_NOT_VERIFIED, new[] { "Email address not verified" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.ACCOUNT_EMAIL_NOT_VERIFIED);
    }

    [Test]
    public void Handle_AccountBanned_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "banneduser@example.com", Password = "password" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.ACCOUNT_BANNED, new[] { "Account has been banned" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.ACCOUNT_BANNED);
    }

    [Test]
    public void Handle_AccountNotFound_ThrowsErrorCodeException()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "notfound@example.com", Password = "password" };
        var errors = new Dictionary<string, string[]> 
        { 
            { ErrorCodes.ACCOUNT_NOTFOUND, new[] { "Account not found" } } 
        };
        var result = (Result.Failure(errors), System.Guid.Empty);
        
        _identityServiceMock.Setup(x => x.CreateUserAsync(command.Email, command.Password))
            .ReturnsAsync(result);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ErrorCodeException>(() => _handler.Handle(command, CancellationToken.None));
        ex.Errors.Should().ContainKey(ErrorCodes.ACCOUNT_NOTFOUND);
    }

}
