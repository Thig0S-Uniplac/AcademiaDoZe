using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Tests.Mappings;

public class ColaboradorMappingExtensionsTests
{
    [Fact]
    public void ToEntity_maps_default_app_vinculo_clt_to_valid_domain_value()
    {
        var colaborador = CriarDto(AppColaboradorTipo.Atendente, AppColaboradorVinculo.CLT).ToEntity();

        Assert.Equal(ColaboradorVinculo.CLT, colaborador.Vinculo);
    }

    [Fact]
    public void UpdateFromDto_applies_default_app_values_for_tipo_and_vinculo()
    {
        var existente = CriarDto(AppColaboradorTipo.Instrutor, AppColaboradorVinculo.Estagio).ToEntity();
        var alteracao = CriarDto(AppColaboradorTipo.Administrador, AppColaboradorVinculo.CLT);

        var atualizado = existente.UpdateFromDto(alteracao);

        Assert.Equal(ColaboradorTipo.Administrador, atualizado.Tipo);
        Assert.Equal(ColaboradorVinculo.CLT, atualizado.Vinculo);
    }

    private static ColaboradorDto CriarDto(AppColaboradorTipo tipo, AppColaboradorVinculo vinculo) => new()
    {
        Nome = "Colaborador Teste",
        Cpf = "529.982.247-25",
        DataNascimento = new DateOnly(1990, 1, 1),
        Telefone = "11912345678",
        Email = "colaborador@example.com",
        Endereco = new LogradouroDto
        {
            Id = 1,
            Cep = "12345-678",
            Nome = "Rua Teste",
            Bairro = "Centro",
            Cidade = "Cidade",
            Estado = "SP",
            Pais = "Brasil"
        },
        Numero = "10",
        Senha = "Abcdef1",
        Foto = new ArquivoDto { Conteudo = [1, 2, 3] },
        DataAdmissao = new DateOnly(2020, 1, 1),
        Tipo = tipo,
        Vinculo = vinculo
    };
}
