using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users;
using CleanArchitectureBase.Application.Users.Common;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CleanArchitectureBase.Application.UnitTests.Authentication;

[TestFixture]
public class LoginCommandHandlerTests 
{
    private Mock<IIdentityService> _identityServiceMock;
    private Mock<IApplicationDbContext> _dbContextMock;
    private LoginCommandCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new LoginCommandCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    [Test]
    public async Task Handle_ValidCredentials_ReturnsTokenDto()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "password" };
        var expectedResult = new TokenDto { AccessToken = "access", RefreshToken = "refresh", ExpireMin = 60, HasPassword = true };
        _identityServiceMock.Setup(x => x.TryLoginAsync(command.Email, command.Password))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _identityServiceMock.Verify(x => x.TryLoginAsync(command.Email, command.Password), Times.Once);
    }

    [Test]
    public void Handle_InvalidCredentials_ThrowsException()
    {
        // Arrange
        var command = new LoginCommand { Email = "wrong@example.com", Password = "wrong" };
        _identityServiceMock.Setup(x => x.TryLoginAsync(command.Email, command.Password))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
    }
} 
