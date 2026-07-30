namespace AcademiaDoZe.Domain.ValueObjects;

public class Cpf
{
    public string Numero { get; private set; }

    public Cpf(string numero)
    {
        Numero = numero;
    }

}
