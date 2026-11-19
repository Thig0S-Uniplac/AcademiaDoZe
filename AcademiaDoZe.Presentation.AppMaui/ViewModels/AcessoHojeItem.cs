namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class AcessoHojeItem
{
    public int Id { get; }
    public string Nome { get; }
    public DateTime DataHora { get; }
    public string TipoAcesso { get; }

    public AcessoHojeItem(int id, string nome, DateTime dataHora, string tipoAcesso)
    {
        Id = id;
        Nome = nome;
        DataHora = dataHora;
        TipoAcesso = tipoAcesso;
    }

    public static IEnumerable<AcessoHojeItem> Criar(
        IEnumerable<(int Id, int PessoaId, DateTime DataHora)> registros,
        IReadOnlyDictionary<int, string> nomesPorPessoa)
    {
        var quantidadePorPessoa = new Dictionary<int, int>();
        var itens = new List<AcessoHojeItem>();

        foreach (var registro in registros.OrderBy(registro => registro.DataHora).ThenBy(registro => registro.Id))
        {
            if (!nomesPorPessoa.TryGetValue(registro.PessoaId, out var nome))
                throw new InvalidOperationException($"Pessoa associada ao acesso {registro.Id} não foi encontrada.");

            var quantidadeAnterior = quantidadePorPessoa.GetValueOrDefault(registro.PessoaId);
            var tipoAcesso = quantidadeAnterior % 2 == 0 ? "Entrada" : "Saída";
            quantidadePorPessoa[registro.PessoaId] = quantidadeAnterior + 1;
            itens.Add(new AcessoHojeItem(registro.Id, nome, registro.DataHora, tipoAcesso));
        }

        return itens.OrderByDescending(item => item.DataHora).ThenByDescending(item => item.Id);
    }
}
