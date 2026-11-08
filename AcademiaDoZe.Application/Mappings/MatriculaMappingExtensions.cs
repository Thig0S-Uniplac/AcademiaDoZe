// Thiago Kovalski
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDto(this Matricula matricula, AlunoDto alunoDto)
    {
        ArgumentNullException.ThrowIfNull(matricula);
        ArgumentNullException.ThrowIfNull(alunoDto);
        return new MatriculaDto
        {
            Id = matricula.Id,
            AlunoMatricula = alunoDto,
            Plano = matricula.Plano.ToApplication(),
            DataInicio = matricula.DataInicio,
            DataFim = matricula.DataFim,
            Objetivo = matricula.Objetivo,
            RestricoesMedicas = matricula.RestricoesMedicas.ToApplication(),
            ObservacoesRestricoes = matricula.ObservacoesRestricoes,
            LaudoMedico = matricula.LaudoMedico != null ? new ArquivoDto { Conteudo = matricula.LaudoMedico.Conteudo } : null
        };
    }
    public static Matricula ToEntity(this MatriculaDto matriculaDto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);
        ArgumentNullException.ThrowIfNull(aluno);
        Arquivo? laudo = null;
        if (matriculaDto.LaudoMedico != null)
        {
            if (matriculaDto.LaudoMedico.Conteudo is not { Length: > 0 })
                throw new InvalidOperationException("O laudo médico não pode estar vazio.");
            var laudoResult = Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo);
            if (laudoResult.IsFailure)
                throw new InvalidOperationException($"Laudo médico inválido: {string.Join(", ", laudoResult.Notifications.Select(n => n.Mensagem))}");
            laudo = laudoResult.Value;
        }
        var result = Matricula.Criar(
        matriculaDto.Id,
        aluno,
        matriculaDto.Plano.ToDomain(),
        matriculaDto.DataInicio,
        matriculaDto.Objetivo,
        matriculaDto.RestricoesMedicas.ToDomain(),
        laudo,
        matriculaDto.ObservacoesRestricoes ?? string.Empty
        );
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter Matrícula: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
    public static Matricula UpdateFromDto(this Matricula matricula, MatriculaDto matriculaDto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(matricula);
        ArgumentNullException.ThrowIfNull(matriculaDto);
        ArgumentNullException.ThrowIfNull(aluno);
        if (matriculaDto.AlunoMatricula == null || matriculaDto.AlunoMatricula.Id != matricula.AlunoId || aluno.Id != matricula.AlunoId)
            throw new InvalidOperationException("Não é permitido alterar o aluno de uma matrícula existente.");
        Arquivo? laudo = null;
        if (matriculaDto.LaudoMedico != null)
        {
            if (matriculaDto.LaudoMedico.Conteudo is not { Length: > 0 })
                throw new InvalidOperationException("O laudo médico não pode estar vazio.");
            var laudoResult = Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo);
            if (laudoResult.IsFailure)
                throw new InvalidOperationException($"Laudo médico inválido: {string.Join(", ", laudoResult.Notifications.Select(n => n.Mensagem))}");
            laudo = laudoResult.Value;
        }
        var result = Matricula.Criar(
        matricula.Id,
        aluno,
        matriculaDto.Plano.ToDomain(),
        matriculaDto.DataInicio,
        matriculaDto.Objetivo,
        matriculaDto.RestricoesMedicas.ToDomain(),
        laudo,
        matriculaDto.ObservacoesRestricoes ?? string.Empty
        );
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao atualizar Matrícula: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
}
