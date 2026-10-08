using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) = ObterConfiguracaoAtual();

        var repoConfig = new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        };

        services.AddSingleton(repoConfig);

        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(repoConfig, (config, message) =>
        {
            var (newConnectionString, newDatabaseType) = ObterConfiguracaoAtual();

            config.ConnectionString = newConnectionString;
            config.DatabaseType = newDatabaseType.ToInfrastructure();
        });

        services.AddApplicationServices();
    }

    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        var databaseType = AppDatabaseType.Sqlite;
        var defaultDbPath = Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
        var dbPath = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultDbPath));
        if (string.IsNullOrWhiteSpace(dbPath))
            dbPath = defaultDbPath;

        return (DbProvider.CreateSqliteConnectionString(dbPath), databaseType);
    }
}
