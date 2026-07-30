using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Aluno : Pessoa
{
    public Arquivo Foto { get; private set; }
    public string RestricoesMedicas { get; private set; }

    public Aluno(
        string nome,
        Cpf cpf,
        Email email,
        Telefone telefone,
        DateTime dataNascimento,
        Endereco endereco,
        Arquivo foto,
        string restricoesMedicas)
        : base(nome, cpf, email, telefone, dataNascimento, endereco)
    {
        Foto = foto;
        RestricoesMedicas = restricoesMedicas;
    }
}