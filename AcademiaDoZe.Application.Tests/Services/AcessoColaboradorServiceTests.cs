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

public class AcessoColaboradorServiceTests
{
    [Fact]
    public async Task Registro_de_acesso_alterna_entrada_e_saida_do_mesmo_colaborador()
    {
        var connectionString = $"Data Source=AcademiaColaboradorAcesso_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await DbInitializer.InicializarAsync(connectionString, DatabaseType.Sqlite);

        var logradouroRepository = new LogradouroRepository(connectionString, DatabaseType.Sqlite);
        var colaboradorRepository = new ColaboradorRepository(connectionString, DatabaseType.Sqlite);
        var logradouro = await logradouroRepository.Adicionar(
            Logradouro.Criar(0, "12345-678", "Rua Teste", "Centro", "Cidade", "SC", "Brasil").Value!);
        var colaborador = Colaborador.Criar(
            0,
            "Colaborador Teste",
            "529.982.247-25",
            new DateOnly(1990, 1, 1),
            "(49) 99123-4567",
            $"{Guid.NewGuid():N}@example.com",
            logradouro,
            "10",
            string.Empty,
            "Senha123",
            Arquivo.Criar([1, 2, 3]).Value!,
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Instrutor,
            ColaboradorVinculo.CLT).Value!;
        colaborador = await colaboradorRepository.Adicionar(colaborador);

        var service = new AcessoColaboradorService(
            () => new AcessoColaboradorRepository(connectionString, DatabaseType.Sqlite),
            () => new ColaboradorRepository(connectionString, DatabaseType.Sqlite));
        var horarioEntrada = DateTime.Today.AddHours(23).AddMinutes(30);

        await service.AdicionarAsync(new AcessoColaboradorDto
        {
            ColaboradorId = colaborador.Id,
            DataHora = horarioEntrada
        });
        Assert.True(await service.EstaNaAcademiaAsync(colaborador.Id));

        await service.AdicionarAsync(new AcessoColaboradorDto
        {
            ColaboradorId = colaborador.Id,
            DataHora = horarioEntrada.AddMinutes(15)
        });
        Assert.False(await service.EstaNaAcademiaAsync(colaborador.Id));

        var data = DateOnly.FromDateTime(horarioEntrada);
        var acessos = (await service.ObterPorColaboradorPeriodoAsync(colaborador.Id, data, data)).ToList();
        Assert.Equal(2, acessos.Count);
        Assert.All(acessos, acesso => Assert.Equal(colaborador.Id, acesso.ColaboradorId));
    }
}
