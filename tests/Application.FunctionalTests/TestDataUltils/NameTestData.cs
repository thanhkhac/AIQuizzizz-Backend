namespace CleanArchitectureBase.Application.Command.UnitTests.TestDataUltils;

public static class NameTestData
{
    public static IEnumerable<TestCaseData> InvalidNameCases
    {
        get
        {
            yield return new TestCaseData(null).SetName("Name_null"); // lower boundary
            yield return new TestCaseData("").SetName("Name_Empty"); // lower boundary
            yield return new TestCaseData(new string('a', 201)).SetName("Name_TooLong"); // upper boundary exceeded
            yield return new TestCaseData("Valid Name").SetName("Name_Valid"); // optional, để test control
        }
    }
}
