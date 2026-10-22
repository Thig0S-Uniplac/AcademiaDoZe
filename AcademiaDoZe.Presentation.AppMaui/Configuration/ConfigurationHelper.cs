using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
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

        // Configura a fábrica de repositórios com a string de conexão e tipo de banco
        services.AddSingleton(repoConfig);

        // Assina mensagens de alteração de banco de dados para atualizar o RepositoryConfig diretamente
        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(repoConfig, (r, m) =>
        {
            var (novaConnStr, novoDbType) = ObterConfiguracaoAtual();

            r.ConnectionString = novaConnStr;
            r.DatabaseType = novoDbType.ToInfrastructure();
        });

        // Configura os serviços da camada de aplicação
        services.AddApplicationServices();
    }

    /// <summary>
    /// Obtém a connection string SQLite ativa e o tipo de banco a partir das Preferences do usuário.
    /// </summary>
    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        var databaseTypeStr = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());

        if (!Enum.TryParse<AppDatabaseType>(databaseTypeStr, out var databaseType))
        {
            databaseType = AppDatabaseType.Sqlite;
        }

        string connectionString;

            var defaultDbPath = DeviceInfo.Platform == DevicePlatform.WinUI
                ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");

            var dbPath = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultDbPath));

            if (string.IsNullOrWhiteSpace(dbPath))
                dbPath = defaultDbPath;

            var complemento = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
            connectionString = $"Data Source={dbPath};{complemento}";
       

        return (connectionString, databaseType);
    }
}
