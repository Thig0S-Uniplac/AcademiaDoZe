// Thiago Kovalski
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infraestruture.Exceptions;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infraestruture.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const int TipoAluno = 0;

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT
        ac.id_acesso, ac.pessoa_id, ac.data_hora,
        a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email,
        a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
        l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso ac
        INNER JOIN tb_aluno a ON a.id_aluno = ac.pessoa_id
        INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
        WHERE ac.pessoa_tipo = 0";

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} AND ac.id_acesso = @Id", cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSO_ALUNO", $"Erro ao obter acesso de aluno por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} ORDER BY ac.data_hora DESC", cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken)) acessos.Add(Map(reader));
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_ALUNO", $"Erro ao obter acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string insert = "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)";
            await using var command = await CreateCommandAsync(FormatInsertQuery(insert), cancellationToken);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter o ID do acesso inserido.", cancellationToken);
            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", $"Erro ao adicionar acesso do aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com ID {entity.Id} para atualização.");
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso de aluno ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso de aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filters = new List<string>();
            if (alunoId.HasValue) filters.Add("ac.pessoa_id = @AlunoId");
            if (inicio.HasValue) filters.Add("ac.data_hora >= @Inicio");
            if (fim.HasValue) filters.Add("ac.data_hora < DATEADD(day, 1, @Fim)");
            var query = $"{BaseSelectQuery}{(filters.Count > 0 ? " AND " + string.Join(" AND ", filters) : string.Empty)} ORDER BY ac.data_hora DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken)) acessos.Add(Map(reader));
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_ALUNO_PERIODO", $"Erro ao obter acessos de alunos no período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} AND ac.pessoa_id = @AlunoId ORDER BY ac.data_hora DESC", cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO_ALUNO", $"Erro ao obter último acesso do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "SELECT COUNT(*) FROM tb_acesso WHERE pessoa_tipo = @PessoaTipo AND pessoa_id = @AlunoId AND CAST(data_hora AS DATE) = CAST(GETDATE() AS DATE)";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            var quantidade = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return quantidade % 2 == 1;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_PRESENCA_ALUNO", $"Erro ao verificar presença do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            var hora = GetDateHourExpression("ac.data_hora");
            var mesExpression = GetDateMonthExpression("ac.data_hora");
            var query = $"SELECT {hora} AS hora, COUNT(*) AS quantidade FROM tb_acesso ac WHERE ac.pessoa_tipo = @PessoaTipo AND {mesExpression} = @Mes GROUP BY {hora} ORDER BY quantidade DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var resultado = new Dictionary<TimeOnly, int>();
            while (await reader.ReadAsync(cancellationToken)) resultado[new TimeOnly(reader.GetInt32Value("hora"), 0)] = reader.GetInt32Value("quantidade");
            return resultado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORARIO_PROCURADO_ALUNO", $"Erro ao obter horários mais procurados: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = @"
                WITH acessos AS (
                    SELECT pessoa_id, data_hora,
                           ROW_NUMBER() OVER (PARTITION BY pessoa_id, CAST(data_hora AS DATE) ORDER BY data_hora) AS numero
                    FROM tb_acesso
                    WHERE pessoa_tipo = @PessoaTipo AND MONTH(data_hora) = @Mes
                ), pares AS (
                    SELECT entrada.pessoa_id, DATEDIFF(SECOND, entrada.data_hora, saida.data_hora) AS segundos
                    FROM acessos entrada
                    INNER JOIN acessos saida ON saida.pessoa_id = entrada.pessoa_id AND saida.numero = entrada.numero + 1
                    WHERE entrada.numero % 2 = 1
                )
                SELECT AVG(CAST(segundos AS FLOAT)) AS media_segundos FROM pares WHERE segundos >= 0";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var resultado = new Dictionary<int, TimeSpan>();
            if (await reader.ReadAsync(cancellationToken) && !reader.IsDBNull(reader.GetOrdinal("media_segundos")))
                resultado[mes] = TimeSpan.FromSeconds(Convert.ToDouble(reader["media_segundos"]));
            return resultado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_PERMANENCIA_ALUNO", $"Erro ao obter permanência média dos alunos: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = @"
                SELECT a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email,
                       a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
                       l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
                FROM tb_aluno a
                INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
                WHERE NOT EXISTS (
                    SELECT 1 FROM tb_acesso ac
                    WHERE ac.pessoa_tipo = @PessoaTipo AND ac.pessoa_id = a.id_aluno
                      AND ac.data_hora >= DATEADD(day, -@Dias, GETDATE()))
                ORDER BY a.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", TipoAluno, DbType.Int32);
            command.AddParameter("@Dias", dias, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var alunos = new List<Aluno>();
            while (await reader.ReadAsync(cancellationToken)) alunos.Add(AlunoRepository.Map(reader));
            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ALUNOS_SEM_ACESSO", $"Erro ao obter alunos sem acesso recente: {ex.Message}", ex);
        }
    }

    public static AcessoAluno Map(DbDataReader reader)
    {
        try
        {
            var aluno = AlunoRepository.Map(reader);
            var result = AcessoAluno.Criar(reader.GetInt32Value("id_acesso"), aluno, reader.GetDateTimeValue("data_hora"));
            if (result.IsFailure)
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso de aluno: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", $"Erro ao mapear acesso de aluno: {ex.Message}", ex);
        }
    }
}