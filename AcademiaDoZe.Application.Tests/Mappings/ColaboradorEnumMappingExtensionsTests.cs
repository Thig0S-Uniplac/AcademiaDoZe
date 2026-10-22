using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Tests.Mappings;

public class ColaboradorEnumMappingExtensionsTests
{
    [Theory]
    [InlineData(AppColaboradorTipo.Administrador, ColaboradorTipo.Administrador)]
    [InlineData(AppColaboradorTipo.Atendente, ColaboradorTipo.Atendente)]
    [InlineData(AppColaboradorTipo.Instrutor, ColaboradorTipo.Instrutor)]
    public void ToDomain_maps_colaborador_tipo_by_name(AppColaboradorTipo appValue, ColaboradorTipo expected)
    {
        Assert.Equal(expected, appValue.ToDomain());
    }

    [Theory]
    [InlineData(ColaboradorTipo.Administrador, AppColaboradorTipo.Administrador)]
    [InlineData(ColaboradorTipo.Atendente, AppColaboradorTipo.Atendente)]
    [InlineData(ColaboradorTipo.Instrutor, AppColaboradorTipo.Instrutor)]
    public void ToApplication_maps_colaborador_tipo_by_name(ColaboradorTipo domainValue, AppColaboradorTipo expected)
    {
        Assert.Equal(expected, domainValue.ToApplication());
    }

    [Theory]
    [InlineData(AppColaboradorVinculo.CLT, ColaboradorVinculo.CLT)]
    [InlineData(AppColaboradorVinculo.Estagio, ColaboradorVinculo.Estagio)]
    public void ToDomain_maps_vinculo_by_name(AppColaboradorVinculo appValue, ColaboradorVinculo expected)
    {
        Assert.Equal(expected, appValue.ToDomain());
    }

    [Theory]
    [InlineData(ColaboradorVinculo.CLT, AppColaboradorVinculo.CLT)]
    [InlineData(ColaboradorVinculo.Estagio, AppColaboradorVinculo.Estagio)]
    public void ToApplication_maps_vinculo_by_name(ColaboradorVinculo domainValue, AppColaboradorVinculo expected)
    {
        Assert.Equal(expected, domainValue.ToApplication());
    }

    [Fact]
    public void ToDomain_rejects_undefined_vinculo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((AppColaboradorVinculo)999).ToDomain());
    }

    [Fact]
    public void ToApplication_rejects_undefined_vinculo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ColaboradorVinculo)999).ToApplication());
    }
}
