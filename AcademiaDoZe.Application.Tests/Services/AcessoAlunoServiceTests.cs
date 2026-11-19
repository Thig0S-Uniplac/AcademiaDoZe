using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infraestruture.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace AcademiaDoZe.Application.Tests.Services;

public class AcessoAlunoServiceTests
{
    [Fact]
    public async Task Registro_de_acesso_permite_visitante_e_exige_nome_completo_do_matriculado()
    {
        var connectionString = $"Data Source=AcademiaAcesso_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await DbInitializer.InicializarAsync(connectionString, DatabaseType.Sqlite);

        var logradouroRepository = new LogradouroRepository(connectionString, DatabaseType.Sqlite);
        var alunoRepository = new AlunoRepository(connectionString, DatabaseType.Sqlite);
        var matriculaRepository = new MatriculaRepository(connectionString, DatabaseType.Sqlite);
        var acessoRepository = new AcessoAlunoRepository(connectionString, DatabaseType.Sqlite);
        var logradouro = await logradouroRepository.Adicionar(
            Logradouro.Criar(0, "12345-678", "Rua Teste", "Centro", "Cidade", "SC", "Brasil").Value!);

        var visitante = await CriarAlunoAsync(alunoRepository, logradouro, "Visitante", "52998224725");
        var alunoMatriculadoSemNomeCompleto = await CriarAlunoAsync(alunoRepository, logradouro, "Maria", "11144477735");
        var alunoMatriculado = await CriarAlunoAsync(alunoRepository, logradouro, "Maria da Silva", "93541134780");

        await matriculaRepository.Adicionar(CriarMatricula(alunoMatriculadoSemNomeCompleto));
        await matriculaRepository.Adicionar(CriarMatricula(alunoMatriculado));

        var service = new AcessoAlunoService(
            () => new AcessoAlunoRepository(connectionString, DatabaseType.Sqlite),
            () => new AlunoRepository(connectionString, DatabaseType.Sqlite),
            () => new MatriculaRepository(connectionString, DatabaseType.Sqlite));

        var horarioEntrada = DateTime.Today.AddHours(10);
        await service.AdicionarAsync(new AcessoAlunoDto { AlunoId = visitante.Id, DataHora = horarioEntrada });
        Assert.True(await service.EstaNaAcademiaAsync(visitante.Id));

        await service.AdicionarAsync(new AcessoAlunoDto { AlunoId = visitante.Id, DataHora = horarioEntrada.AddHours(1) });
        Assert.False(await service.EstaNaAcademiaAsync(visitante.Id));

        var erroNome = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AdicionarAsync(new AcessoAlunoDto
            {
                AlunoId = alunoMatriculadoSemNomeCompleto.Id,
                DataHora = horarioEntrada
            }));
        Assert.Contains("nome completo", erroNome.Message, StringComparison.OrdinalIgnoreCase);

        await service.AdicionarAsync(new AcessoAlunoDto { AlunoId = alunoMatriculado.Id, DataHora = horarioEntrada });
        Assert.True(await service.EstaNaAcademiaAsync(alunoMatriculado.Id));
        await service.AdicionarAsync(new AcessoAlunoDto { AlunoId = alunoMatriculado.Id, DataHora = horarioEntrada.AddHours(1) });
        Assert.False(await service.EstaNaAcademiaAsync(alunoMatriculado.Id));

        var data = DateOnly.FromDateTime(horarioEntrada);
        var acessos = (await service.ObterPorAlunoPeriodoAsync(visitante.Id, data, data)).ToList();
        Assert.Equal(2, acessos.Count);
        Assert.All(acessos, acesso => Assert.Equal(visitante.Id, acesso.AlunoId));
    }

    private static async Task<Aluno> CriarAlunoAsync(
        AlunoRepository alunoRepository,
        Logradouro logradouro,
        string nome,
        string cpf)
    {
        var aluno = Aluno.Criar(
            0,
            nome,
            cpf,
            new DateOnly(1990, 1, 1),
            "49991234567",
            $"{Guid.NewGuid():N}@example.com",
            logradouro,
            "10",
            string.Empty,
            "Senha123",
            Arquivo.Criar([1, 2, 3]).Value!).Value!;

        return await alunoRepository.Adicionar(aluno);
    }

    private static Matricula CriarMatricula(Aluno aluno) =>
        Matricula.Criar(
            0,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Condicionamento",
            MatriculaRestricoes.None,
            null).Value!;
}
