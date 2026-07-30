namespace AcademiaDoZe.Domain.ValueObjects;

public class Email
{
    public string Valor { get; private set; }

    public Email(string valor)
    {
        Valor = valor;
    }

}
