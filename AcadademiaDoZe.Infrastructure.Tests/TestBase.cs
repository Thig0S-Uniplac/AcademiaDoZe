using AcademiaDoZe.Infrastructure.Data;
using Microsoft.Data.Sqlite;
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]
namespace AcadademiaDoZe.Infrastructure.Tests;
// thiago kovalski

public abstract class TestBase : IDisposable
{
    // Alterne o SGBD alvo dos testes trocando apenas a constante abaixo:
    private const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;
    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }
    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        ConnectionString = DatabaseType switch
        {
            DatabaseType.Sqlite => "Data Source=db_academia_do_ze.db;",
            _ => throw new ArgumentOutOfRangeException(
                nameof(DatabaseType),
                DatabaseType,
                "SGBD não suportado para testes."
            )
        };

        CleanupDatabase();
    }

    public void Dispose()
    {
        CleanupDatabase();
        GC.SuppressFinalize(this);
    }

    private void CleanupDatabase()
    {
        if (DatabaseType != DatabaseType.Sqlite)
        {
            return;
        }

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = DbInitializer.ObterScript(DatabaseType) + """

            DELETE FROM tb_acesso;
            DELETE FROM tb_matricula;
            DELETE FROM tb_colaborador;
            DELETE FROM tb_aluno;
            DELETE FROM tb_logradouro;
            """;
        command.ExecuteNonQuery();
    }

    #region Geradores de dados aleatórios
    private static int _counter = 10000;
    private static int _cpfCounter = 10000;
    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + Interlocked.Increment(ref _counter)).ToString("D8")[..8];
    protected static string GerarCpf()
    {
        var baseCpf = Interlocked.Increment(ref _cpfCounter).ToString("D9");
        var firstDigit = CalcularDigitoCpf(baseCpf);
        var secondDigit = CalcularDigitoCpf(baseCpf + firstDigit);
        return baseCpf + firstDigit + secondDigit;
    }
    private static int CalcularDigitoCpf(string cpfParcial)
    {
        var soma = 0;
        var multiplicador = cpfParcial.Length + 1;
        foreach (var digito in cpfParcial)
            soma += (digito - '0') * multiplicador--;
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";
    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    #endregion
}