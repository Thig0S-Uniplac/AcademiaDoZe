using System;
using AcademiaDoZe.Domain.Entities.Base;

namespace AcademiaDoZe.Domain.Entities
{
    public class AcessoAluno : Entity
    {
        public Guid AlunoId { get; private set; }
        public Aluno Aluno { get; private set; } = null!;
        public DateTime DataHoraAcesso { get; private set; }
        public bool Liberado { get; private set; }

        public AcessoAluno(Guid alunoId, DateTime dataHoraAcesso, bool liberado)
        {
            AlunoId = alunoId;
            DataHoraAcesso = dataHoraAcesso;
            Liberado = liberado;
        }
    }
}