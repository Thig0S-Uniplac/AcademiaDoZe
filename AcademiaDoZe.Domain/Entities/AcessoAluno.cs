using System;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Domain.Common;
using AcademiaDoZe.Domain.Entities.Base;
//thiago kovaslki

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoAluno : Entity
    {
        public int AlunoId { get; private set; }
        public Aluno Aluno { get; private set; } = null!;
        public DateTime DataHoraAcesso { get; private set; }
        public bool Liberado { get; private set; }

        private AcessoAluno(int alunoId, DateTime dataHoraAcesso, bool liberado)
        {
            AlunoId = alunoId;
            DataHoraAcesso = dataHoraAcesso;
            Liberado = liberado;
        }
        public static Result<AcessoAluno> Criar(int id, int alunoId, DateTime dataHoraAcesso, bool liberado)
        {
            var notifications = new List<Notification>();

            // Validações
            if (alunoId <= 0)
                notifications.Add(new Notification("AlunoId", "ALUNO_ID_OBRIGATORIO"));

            if (dataHoraAcesso == default)
                notifications.Add(new Notification("DataHoraAcesso", "DATA_HORA_ACESSO_OBRIGATORIO"));
            else if (dataHoraAcesso > DateTime.Now)
                notifications.Add(new Notification("DataHoraAcesso", "DATA_HORA_ACESSO_FUTURA_INVALIDA"));

            if (notifications.Count != 0)
                return Result<AcessoAluno>.Failure(notifications);

            // Criação e retorno do objeto
            var acesso = new AcessoAluno(alunoId, dataHoraAcesso, liberado);

            return Result<AcessoAluno>.Success(acesso);
        }
    }
}