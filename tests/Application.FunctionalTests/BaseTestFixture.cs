using CleanArchitectureBase.Application.FunctionalTests;

namespace CleanArchitectureBase.Application.Command.UnitTests;

using static Testing;

[TestFixture]
public abstract class BaseTestFixture
{
    [SetUp]
    public virtual async Task TestSetUp()
    {
        await ResetState();
    }
}
