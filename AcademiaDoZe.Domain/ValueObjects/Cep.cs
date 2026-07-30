namespace AcademiaDoZe.Domain.ValueObjects;

public class Cep
{
    public string Numero { get; private set;}

    public Cep(string numero)
    {
        Numero = numero;
    }

}
