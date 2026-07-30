namespace AcademiaDoZe.Domain.ValueObjects;

public class Endereco
{
    public Logradouro Logradouro { get; private set; }
    public string Numero { get; private set; }
    public string Complemento { get; private set; }

    public Endereco(Logradouro logradouro, string numero, string complemento)
    {
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
    }
}
