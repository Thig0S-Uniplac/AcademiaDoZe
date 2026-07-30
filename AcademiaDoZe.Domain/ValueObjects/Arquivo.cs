namespace AcademiaDoZe.Domain.ValueObjects;

public class Arquivo
{
    public string Nome { get; private set; }
    public string Extensao { get; private set; }
    public byte[] Bytes { get; private set; }

    public Arquivo(string nome, string extensao, byte[] bytes)
    {
        Nome = nome;
        Extensao = extensao;
        Bytes = bytes;
    }
}
