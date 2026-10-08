using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Application.Mappings;
public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType appDatabaseType) =>
        appDatabaseType switch
        {
            AppDatabaseType.Sqlite => DatabaseType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(appDatabaseType), appDatabaseType, "Banco de dados não suportado.")
        };

    public static AppDatabaseType ToApplication(this DatabaseType databaseType) =>
        databaseType switch
        {
            DatabaseType.Sqlite => AppDatabaseType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Banco de dados não suportado.")
        };
}