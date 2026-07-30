using AcademiaDoZe.Domain.Entities.Base;

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoColaborador : Entity
    {
        public Guid ColaboradorId { get; private set; }
        public Colaborador Colaborador { get; private set; } = null!;
        public DateTime DataHoraAcesso { get; private set; }
        public bool Liberado { get; private set; }

        public AcessoColaborador(Guid colaboradorId, DateTime dataHoraAcesso, bool liberado)
        {
            ColaboradorId = colaboradorId;
            DataHoraAcesso = dataHoraAcesso;
            Liberado = liberado;
        }
    }
}