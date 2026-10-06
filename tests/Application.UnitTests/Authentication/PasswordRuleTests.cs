using CleanArchitectureBase.Application.Users;
using FluentAssertions;
using NUnit.Framework;

namespace CleanArchitectureBase.Application.UnitTests.Authentication;

[TestFixture]
public class PasswordRuleTests
{
    [TestCase("abc12345", true)]
    [TestCase("Abcdef12", true)]
    [TestCase("abc123", false)]
    [TestCase("abcdefgh", false)]
    [TestCase("12345678", false)]
    public void RegisterValidator_EnforcesUnifiedPasswordRule(string password, bool valid)
    {
        var validator = new RegisterUserCommandValidator();
        var result = validator.Validate(new RegisterUserCommand { Email = "a@b.com", Password = password });
        result.IsValid.Should().Be(valid);
    }

    [Test]
    public void ChangePasswordValidator_RejectsShortPassword()
    {
        var validator = new ChangePasswordCommandValidator();
        validator.Validate(new ChangePasswordCommand { CurrentPassword = "x", NewPassword = "abc123" }).IsValid.Should().BeFalse();
        validator.Validate(new ChangePasswordCommand { CurrentPassword = "x", NewPassword = "abc12345" }).IsValid.Should().BeTrue();
    }
}
