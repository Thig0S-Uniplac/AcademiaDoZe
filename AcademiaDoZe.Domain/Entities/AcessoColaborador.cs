using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity, IAggregateRoot
{
    public int ColaboradorId { get; private set; }
    public DateTime DataHora { get; private set; }
    private AcessoColaborador(int id, int colaboradorId, DateTime dataHora) : base(id)
    {
        ColaboradorId = colaboradorId;
        DataHora = dataHora;
    }
    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateTime dataHora)
    {
        var notifications = new List<Notification>();
        if (colaborador == null)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_INVALIDO"));
        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);
        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador!.Id, dataHora));
    }
}
// Dependem da persistência:
// Validar se já não ultrapassa o limite de: 8 horas se for ctl, 6 horas se for estágio.
// Na saída, mostrar o tempo que permaneceu na academia, devendo ser somado todos os registros do dia.