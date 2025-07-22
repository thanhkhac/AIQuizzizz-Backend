using CleanArchitectureBase.Application.FunctionalTests;

namespace CleanArchitectureBase.Application.Command.UnitTests;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync()
    {
        var database = new PostgreSQLTestcontainersTestDatabase();

        await database.InitialiseAsync();

        return database;
    }
}
