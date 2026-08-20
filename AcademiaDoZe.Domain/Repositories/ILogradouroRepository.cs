using AcademiaDoZe.Domain.Entities; //Thiago Kovalski
using AcademiaDoZe.Domain.ValueObjects; //Thiago Kovalski
namespace AcademiaDoZe.Domain.Repositories;

public interface ILogradouroRepository : IRepository<Logradouro>
{
    // Métodos específicos do domínio
    Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default);
    Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default);
    Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default);
}