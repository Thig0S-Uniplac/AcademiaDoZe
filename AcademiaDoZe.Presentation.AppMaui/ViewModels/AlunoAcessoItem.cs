using AcademiaDoZe.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoAcessoItem : ObservableObject
{
    public int AlunoId { get; }
    public string Nome { get; }
    public string Cpf { get; }
    public bool PossuiMatriculaAtiva { get; }
    public string SituacaoMatricula => PossuiMatriculaAtiva ? "Matriculado" : "Sem matrícula";
    public string SituacaoAcesso => EstaNaAcademia ? "Na academia" : "Fora da academia";
    public string Acao => EstaNaAcademia
        ? "Registrar saída"
        : PossuiMatriculaAtiva ? "Registrar entrada" : "Entrada bloqueada";

    private bool _estaNaAcademia;
    public bool EstaNaAcademia
    {
        get => _estaNaAcademia;
        set
        {
            if (SetProperty(ref _estaNaAcademia, value))
            {
                OnPropertyChanged(nameof(SituacaoAcesso));
                OnPropertyChanged(nameof(Acao));
            }
        }
    }

    public AlunoAcessoItem(AlunoDto aluno, bool possuiMatriculaAtiva, bool estaNaAcademia)
    {
        AlunoId = aluno.Id;
        Nome = aluno.Nome;
        Cpf = aluno.Cpf;
        PossuiMatriculaAtiva = possuiMatriculaAtiva;
        EstaNaAcademia = estaNaAcademia;
    }
}
