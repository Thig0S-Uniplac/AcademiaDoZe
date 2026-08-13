using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Domain.Common;
using AcademiaDoZe.Domain.Entities.Base;
//thiago kovaslki
namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoColaborador : Entity
    {
        public int ColaboradorId { get; private set; }
        public Colaborador Colaborador { get; private set; } = null!;
        public DateTime DataHoraAcesso { get; private set; }
        public bool Liberado { get; private set; }

        private AcessoColaborador(int colaboradorId, DateTime dataHoraAcesso, bool liberado)
        {
            ColaboradorId = colaboradorId;
            DataHoraAcesso = dataHoraAcesso;
            Liberado = liberado;
        }
        public static Result<AcessoColaborador> Criar(int id, int colaboradorId, DateTime dataHoraAcesso, bool liberado)
        {
            var notifications = new List<Notification>();

            if (dataHoraAcesso == default)
                notifications.Add(new Notification("DataHoraAcesso", "DATA_HORA_ACESSO_OBRIGATORIO"));
            else if (dataHoraAcesso > DateTime.Now)
                notifications.Add(new Notification("DataHoraAcesso", "DATA_HORA_ACESSO_FUTURA_INVALIDA"));

            if (notifications.Count != 0)
                return Result<AcessoColaborador>.Failure(notifications);

            // Criação e retorno do objeto
            var acesso = new AcessoColaborador(colaboradorId, dataHoraAcesso, liberado);

            return Result<AcessoColaborador>.Success(acesso);
        }
    }
}