using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;
//thiago kovalski
public class Senha
{
    public string Valor { get; private set; }

    private Senha(string valor)
    {
        Valor = valor;
    }
    public static Result<Senha> Criar(string senha)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(senha))
            notifications.Add(new Notification("Senha", "SENHA_OBRIGATORIO"));

        if (notifications.Count != 0)
            return Result<Senha>.Failure(notifications);

        return Result<Senha>.Success(new Senha(senha));

    }
}
