using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;
//thiago kovaslki

public class Aluno : Pessoa
{
    public Arquivo Foto { get; private set; }
    public string RestricoesMedicas { get; private set; }

    private Aluno(
        int id,
        string nome,
        Cpf cpf,
        Email email,
        Telefone telefone,
        DateOnly dataNascimento,
        Endereco endereco,
        Arquivo foto,
        Senha senha,
        string restricoesMedicas)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
        Foto = foto;
        RestricoesMedicas = restricoesMedicas;
    }
    public static Result<Aluno> Criar(
        int id,
        string nome,
        string cpf,
        DateOnly dataNascimento,
        string telefone,
        string email,
        Logradouro endereco,
        string numero,
        string complemento,
        string senha,
        Arquivo foto,
        string restricoesMedicas)
    {
        var notifications = new List<Notification>();

        // Validações e normalizações
        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

        // Normalização de campo opcional
        restricoesMedicas = NormalizadoService.LimparEspacos(restricoesMedicas);

        // Instanciação e validação via Value Objects
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure) notifications.AddRange(senhaResult.Notifications);

        var enderecoResult = Endereco.Criar(endereco, numero, complemento);
        if (enderecoResult.IsFailure) notifications.AddRange(enderecoResult.Notifications);

        if (notifications.Count != 0)
            return Result<Aluno>.Failure(notifications);

        // criação e retorno do objeto
        var aluno = new Aluno(
            id,
            nome,
            cpfResult.Value!,
            emailResult.Value!,
            telefoneResult.Value!,
            dataNascimento,
            enderecoResult.Value!,
            foto,
            senhaResult.Value!,
            restricoesMedicas);

        return Result<Aluno>.Success(aluno);
    }
}