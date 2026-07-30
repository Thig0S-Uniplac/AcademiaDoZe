
using AcademiaDoZe.Domain.Entities.Base;
using AcademiaDoZe.Domain.Enums;
//thiago kovaslki

namespace AcademiaDoZe.Domain.Entities
{
    public class Matricula : Entity
    {
        public Guid AlunoId { get; private set; }
        public Aluno Aluno { get; private set; } = null!;
        public MatriculaPlano Plano { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal Valor { get; private set; }
        public MatriculaRestricoes Restricoes { get; private set; }

        public Matricula(
            Guid alunoId,
            MatriculaPlano plano,
            DateTime dataInicio,
            DateTime dataFim,
            decimal valor,
            MatriculaRestricoes restricoes)
        {
            AlunoId = alunoId;
            Plano = plano;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Valor = valor;
            Restricoes = restricoes;
        }
    }
}