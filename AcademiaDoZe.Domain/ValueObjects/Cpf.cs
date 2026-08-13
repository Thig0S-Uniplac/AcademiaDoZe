using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Domain.Common;
using AcademiaDoZe.Domain.Services;

//thiago kovalski
namespace AcademiaDoZe.Domain.ValueObjects;

public class Cpf
{
    public string Numero { get; private set; }

    private Cpf(string numero)
    {
        Numero = numero;
    }
    public static Result<Cpf> Criar(string cpf)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(cpf))
        {
            notifications.Add(new Notification("Cpf", "CPF_OBRIGATORIO"));
        }
        else
        {
            // Limpa caracteres não numéricos (pontos, traços e espaços)
            var textoLimpo = NormalizadoService.LimparEDigitos(cpf);
            cpf = textoLimpo;
        }

        if (notifications.Count != 0)
            return Result<Cpf>.Failure(notifications);

        return Result<Cpf>.Success(new Cpf(cpf));
    }
}
