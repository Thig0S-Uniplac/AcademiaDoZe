namespace AcademiaDoZe.Domain.ValueObjects;

public class Senha
{
    public string Valor { get; private set; }

    public Senha(string valor)
    {
        Valor = valor;
    }
}
