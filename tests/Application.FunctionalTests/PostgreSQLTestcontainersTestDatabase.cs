using System.Data.Common;
using CleanArchitectureBase.Application.FunctionalTests;
using CleanArchitectureBase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace CleanArchitectureBase.Application.Command.UnitTests;

public class PostgreSQLTestcontainersTestDatabase : ITestDatabase
{
    private const string DefaultDatabase = "CleanArchitectureTestDb";
    private readonly PostgreSqlContainer _container;
    private DbConnection _connection = null!;
    private string _connectionString = null!;
    private Respawner _respawner = null!;

    public PostgreSQLTestcontainersTestDatabase()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("CleanArchitectureTestDb")
            .WithUsername("admin")
            .WithPassword("password")
            .WithPortBinding(5432, 5432) // Map thẳng port ra ngoài
            .WithAutoRemove(true)
            .Build();
    }

    public async Task InitialiseAsync()
    {
        await _container.StartAsync();
        await _container.ExecScriptAsync($"CREATE DATABASE {DefaultDatabase}");

        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = DefaultDatabase
        };

        _connectionString = builder.ConnectionString;

        _connection = new NpgsqlConnection(_connectionString);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .ConfigureWarnings(warnings =>
                warnings.Log(RelationalEventId
                    .MigrationsNotApplied)) //JasonTaylordev: PendingModelChangesWarning, tuy nhiên mã sự kiện này chỉ có ở EF của .NET 9
            .Options;

        var context = new ApplicationDbContext(options);

        await context.Database.MigrateAsync();

        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
        await _connection.CloseAsync();
    }

    public DbConnection GetConnection()
    {
        return _connection;
    }

    public string GetConnectionString()
    {
        return _connectionString;
    }

    public async Task ResetAsync()
    {
        await _connection.OpenAsync();
        await _respawner.ResetAsync(_connection);
        await _connection.CloseAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
