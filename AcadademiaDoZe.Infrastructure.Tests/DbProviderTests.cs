using AcademiaDoZe.Infraestruture.Exceptions;
using AcademiaDoZe.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace AcadademiaDoZe.Infrastructure.Tests;

public class DbProviderTests
{
    [Fact]
    public async Task CreateConnection_CreatesSqliteFileAndEnablesForeignKeys()
    {
        var root = Path.Combine(Path.GetTempPath(), $"academia-do-ze-{Guid.NewGuid():N}");
        var databasePath = Path.Combine(root, "nested folder", "database;file.db");

        try
        {
            var connectionString = DbProvider.CreateSqliteConnectionString(databasePath);
            Assert.Equal(databasePath, new SqliteConnectionStringBuilder(connectionString).DataSource);

            await using (var connection = DbProvider.CreateConnection(connectionString))
            {
                await connection.OpenAsync();

                Assert.True(File.Exists(databasePath));
                await using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys;";
                Assert.Equal(1, Convert.ToInt32(await command.ExecuteScalarAsync()));
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CreateSqliteConnectionString_RejectsEmptyPath()
    {
        var exception = Assert.Throws<InfrastructureException>(() => DbProvider.CreateSqliteConnectionString(" "));

        Assert.Equal("CAMINHO_BANCO_VAZIO", exception.ErrorCode);
    }
}
