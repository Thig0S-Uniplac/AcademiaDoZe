namespace AcademiaDoZe.Domain.ValueObjects;

public class Logradouro
{
    public string Nome { get; private set; }
    public string Bairro { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public Cep Cep { get; private set; }

    public Logradouro(string nome, string bairro, string cidade, string estado, Cep cep)
    {
        Nome = nome;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
    }
}
