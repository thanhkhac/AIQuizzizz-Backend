using System.Data.Common;

namespace CleanArchitectureBase.Application.Command.UnitTests;

public interface ITestDatabase
{
    Task InitialiseAsync();

    DbConnection GetConnection();

    string GetConnectionString();

    Task ResetAsync();

    Task DisposeAsync();
}
