
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Domain.Common;
using AcademiaDoZe.Domain.Entities.Base;
using AcademiaDoZe.Domain.Enums;
//thiago kovaslki

namespace AcademiaDoZe.Domain.Entities
{
    public class Matricula : Entity
    {
        public int AlunoId { get; private set; }
        public Aluno Aluno { get; private set; } = null!;
        public MatriculaPlano Plano { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal Valor { get; private set; }
        public MatriculaRestricoes Restricoes { get; private set; }

        private Matricula(
            int alunoId,
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
        public static Result<Matricula> Criar(
        int id,
        int alunoId,
        MatriculaPlano plano,
        DateTime dataInicio,
        DateTime dataFim,
        decimal valor,
        MatriculaRestricoes restricoes)
        {
            var notifications = new List<Notification>();

            if (alunoId <= 0)
                notifications.Add(new Notification("AlunoId", "ALUNO_ID_OBRIGATORIO"));

            if (!Enum.IsDefined(plano))
                notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));

            if (!Enum.IsDefined(restricoes))
                notifications.Add(new Notification("Restricoes", "RESTRICOES_INVALIDO"));

            if (valor <= 0)
                notifications.Add(new Notification("Valor", "VALOR_MATRICULA_INVALIDO"));

            if (dataInicio == default)
                notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIA"));

            if (dataFim == default)
                notifications.Add(new Notification("DataFim", "DATA_FIM_OBRIGATORIA"));
            else if (dataFim <= dataInicio)
                notifications.Add(new Notification("DataFim", "DATA_FIM_MENOR_OU_IGUAL_INICIO"));

            if (notifications.Count != 0)
                return Result<Matricula>.Failure(notifications);

            // Criação e retorno do objeto
            var matricula = new Matricula(alunoId, plano, dataInicio, dataFim, valor, restricoes);

            return Result<Matricula>.Success(matricula);
        }
    }
}