// Thiago Kovalski
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;
namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaEnumMappingExtensions
{
    public static MatriculaPlano ToDomain(this AppMatriculaPlano appPlano)
    {
        return appPlano switch
        {
            AppMatriculaPlano.Mensal => MatriculaPlano.Mensal,
            AppMatriculaPlano.Trimestral => MatriculaPlano.Trimestral,
            AppMatriculaPlano.Semestral => MatriculaPlano.Semestral,
            AppMatriculaPlano.Anual => MatriculaPlano.Anual,
            _ => throw new ArgumentOutOfRangeException(nameof(appPlano), appPlano, "Plano inválido.")
        };
    }
    public static AppMatriculaPlano ToApplication(this MatriculaPlano domainPlano)
    {
        return domainPlano switch
        {
            MatriculaPlano.Mensal => AppMatriculaPlano.Mensal,
            MatriculaPlano.Trimestral => AppMatriculaPlano.Trimestral,
            MatriculaPlano.Semestral => AppMatriculaPlano.Semestral,
            MatriculaPlano.Anual => AppMatriculaPlano.Anual,
            _ => throw new ArgumentOutOfRangeException(nameof(domainPlano), domainPlano, "Plano inválido.")
        };
    }
    public static MatriculaRestricoes ToDomain(this AppMatriculaRestricoes appRestricoes)
    {
        return (MatriculaRestricoes)appRestricoes;
    }
    public static AppMatriculaRestricoes ToApplication(this MatriculaRestricoes domainRestricoes)
    {
        return (AppMatriculaRestricoes)domainRestricoes;
    }
}
