using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Tests.Mappings;

public class MatriculaEnumMappingExtensionsTests
{
    [Theory]
    [InlineData(AppMatriculaPlano.Mensal, MatriculaPlano.Mensal)]
    [InlineData(AppMatriculaPlano.Trimestral, MatriculaPlano.Trimestral)]
    [InlineData(AppMatriculaPlano.Semestral, MatriculaPlano.Semestral)]
    [InlineData(AppMatriculaPlano.Anual, MatriculaPlano.Anual)]
    public void Plano_converte_nos_dois_sentidos(AppMatriculaPlano app, MatriculaPlano domain)
    {
        Assert.Equal(domain, app.ToDomain());
        Assert.Equal(app, domain.ToApplication());
    }

    [Fact]
    public void Plano_rejeita_valores_invalidos()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((AppMatriculaPlano)99).ToDomain());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((MatriculaPlano)0).ToApplication());
    }

    [Fact]
    public void Restricoes_preservam_selecao_multipla_nos_dois_sentidos()
    {
        var app = AppMatriculaRestricoes.Diabetes | AppMatriculaRestricoes.Alergias;
        var domain = MatriculaRestricoes.Diabetes | MatriculaRestricoes.Alergias;
        Assert.Equal(domain, app.ToDomain());
        Assert.Equal(app, domain.ToApplication());
    }
}
