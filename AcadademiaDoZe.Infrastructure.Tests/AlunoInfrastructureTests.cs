using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infraestruture.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcadademiaDoZe.Infrastructure.Tests;
//thiago kovalski
public class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
    }

    internal static async Task<Aluno> CriarEInserirAlunoAsync(AlunoRepository alunoRepo, LogradouroRepository logradouroRepo)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value!;

        var alunoResult = Aluno.Criar(
            id: 0,
            nome: "Aluno Teste " + Guid.NewGuid().ToString("N")[..5],
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(2000, 10, 10),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "100",
            complemento: "Apto 101",
            senha: "SenhaAluno123",
            foto: foto
        );

        if (alunoResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Aluno: {string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await alunoRepo.Adicionar(alunoResult.Value!);
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        Assert.NotNull(aluno);
        Assert.True(aluno.Id > 0);

        var obtido = await _alunoRepo.ObterPorId(aluno.Id);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(aluno.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(aluno.Nome, obtido.Nome);
        Assert.Equal(aluno.Email.Valor, obtido.Email.Valor);
        Assert.NotNull(obtido.Endereco);
        Assert.Equal(aluno.Endereco.LogradouroId, obtido.Endereco.LogradouroId);
    }

    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var todos = await _alunoRepo.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value!;

        var aluno = await _alunoRepo.Adicionar(Aluno.Criar(
            0, "Aluno Original", GerarCpf(), new DateOnly(2000, 1, 1),
            GerarTelefone(), GerarEmail(), logradouro, "100", "Bloco A",
            "Senha123", foto).Value!);

        var novoNome = "Aluno Editado " + Guid.NewGuid().ToString("N")[..5];
        var alunoAtualizado = Aluno.Criar(
            id: aluno.Id,
            nome: novoNome,
            cpf: aluno.Cpf.Valor,
            dataNascimento: aluno.DataNascimento,
            telefone: aluno.Telefone.Valor,
            email: aluno.Email.Valor,
            endereco: logradouro,
            numero: "200",
            complemento: "Bloco B",
            senha: aluno.Senha.Valor,
            foto: aluno.Foto
        ).Value!;

        var resultado = await _alunoRepo.Atualizar(alunoAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal(novoNome, resultado.Nome);

        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(novoNome, noBanco.Nome);
    }

    [Fact]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var removido = await _alunoRepo.Remover(aluno.Id);

        Assert.True(removido);
        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Aluno_CpfJaExiste_ValidacaoCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var existe = await _alunoRepo.CpfJaExiste(aluno.Cpf);
        Assert.True(existe);

        var existeIgnorandoId = await _alunoRepo.CpfJaExiste(aluno.Cpf, aluno.Id);
        Assert.False(existeIgnorandoId);
    }

    [Fact]
    public async Task Aluno_TrocarSenha_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var novaSenha = Senha.Criar("NovaSenhaAluno123").Value!;

        var alterou = await _alunoRepo.TrocarSenha(aluno.Id, novaSenha);

        Assert.True(alterou);
        var atualizado = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(atualizado);
        Assert.Equal("NovaSenhaAluno123", atualizado.Senha.Valor);
    }
}