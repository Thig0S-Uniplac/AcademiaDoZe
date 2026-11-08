using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Tests.Mappings;

public class MatriculaMappingExtensionsTests
{
    [Fact]
    public void UpdateFromDto_aplica_plano_mensal_limpa_restricoes_e_remove_laudo()
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.Plano = AppMatriculaPlano.Mensal;
        dto.RestricoesMedicas = AppMatriculaRestricoes.None;
        dto.LaudoMedico = null;
        dto.ObservacoesRestricoes = null;

        var atualizado = existente.UpdateFromDto(dto, aluno);

        Assert.Equal(MatriculaPlano.Mensal, atualizado.Plano);
        Assert.Equal(dto.DataInicio.AddMonths(1), atualizado.DataFim);
        Assert.Equal(MatriculaRestricoes.None, atualizado.RestricoesMedicas);
        Assert.Null(atualizado.LaudoMedico);
        Assert.Equal(string.Empty, atualizado.ObservacoesRestricoes);
    }

    [Fact]
    public void UpdateFromDto_substitui_laudo()
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.LaudoMedico = new ArquivoDto { Conteudo = [4, 5, 6] };

        var atualizado = existente.UpdateFromDto(dto, aluno);

        Assert.Equal(dto.LaudoMedico.Conteudo, atualizado.LaudoMedico!.Conteudo);
    }

    [Fact]
    public void UpdateFromDto_rejeita_remocao_de_laudo_com_restricoes()
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.LaudoMedico = null;

        Assert.Throws<InvalidOperationException>(() => existente.UpdateFromDto(dto, aluno));
    }

    [Fact]
    public void UpdateFromDto_rejeita_troca_de_aluno()
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.AlunoMatricula.Id = 2;

        Assert.Throws<InvalidOperationException>(() => existente.UpdateFromDto(dto, aluno));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Mapeamento_rejeita_laudo_acima_de_15_MB(bool atualizar)
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.LaudoMedico = new ArquivoDto { Conteudo = new byte[15 * 1024 * 1024 + 1] };

        Assert.Throws<InvalidOperationException>(() => atualizar
            ? existente.UpdateFromDto(dto, aluno)
            : dto.ToEntity(aluno));
    }

    [Fact]
    public void UpdateFromDto_rejeita_data_de_inicio_vazia()
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.DataInicio = default;

        Assert.Throws<InvalidOperationException>(() => existente.UpdateFromDto(dto, aluno));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Mapeamento_rejeita_laudo_vazio(bool atualizar)
    {
        var aluno = CriarAluno();
        var existente = CriarMatricula(aluno);
        var dto = existente.ToDto(aluno.ToDto());
        dto.LaudoMedico = new ArquivoDto { Conteudo = [] };

        Assert.Throws<InvalidOperationException>(() => atualizar
            ? existente.UpdateFromDto(dto, aluno)
            : dto.ToEntity(aluno));
    }

    [Theory]
    [InlineData(AppMatriculaPlano.Mensal, 1)]
    [InlineData(AppMatriculaPlano.Trimestral, 3)]
    [InlineData(AppMatriculaPlano.Semestral, 6)]
    [InlineData(AppMatriculaPlano.Anual, 12)]
    public void ToEntity_calcula_vigencia_pelo_plano_e_ignora_data_fim_do_formulario(AppMatriculaPlano plano, int meses)
    {
        var aluno = CriarAluno();
        var dto = CriarMatricula(aluno).ToDto(aluno.ToDto());
        dto.Plano = plano;
        dto.DataFim = default;

        var criada = dto.ToEntity(aluno);

        Assert.Equal(dto.DataInicio.AddMonths(meses), criada.DataFim);
        Assert.Equal(plano, criada.ToDto(aluno.ToDto()).Plano);
    }

    private static Aluno CriarAluno()
    {
        var endereco = Logradouro.Criar(1, "12345-678", "Rua Teste", "Centro", "Cidade", "SP", "Brasil").Value!;
        return Aluno.Criar(1, "Aluno Teste", "529.982.247-25", new DateOnly(1990, 1, 1),
            "11912345678", "aluno@example.com", endereco, "10", string.Empty, "Abcdef1",
            Arquivo.Criar([1, 2, 3]).Value!).Value!;
    }

    private static Matricula CriarMatricula(Aluno aluno) => Matricula.Criar(1, aluno,
        MatriculaPlano.Anual, new DateOnly(2026, 1, 31), "Condicionamento",
        MatriculaRestricoes.Diabetes, Arquivo.Criar([1, 2, 3]).Value!, "Cuidados").Value!;
}
