using CleanArchitectureBase.Application.Common.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace CleanArchitectureBase.Application.UnitTests.Common;

[TestFixture]
public class DisplayNameHelperTests
{
    [TestCase("alice@example.com", "a***@example.com")]
    [TestCase("Nguyen Van A", "Nguyen Van A")]
    [TestCase("", "")]
    public void MaskIfEmail_MasksOnlyEmails(string input, string expected)
    {
        DisplayNameHelper.MaskIfEmail(input).Should().Be(expected);
    }
}
