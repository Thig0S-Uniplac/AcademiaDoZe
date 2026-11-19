using AcademiaDoZe.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorAcessoItem : ObservableObject
{
    public int ColaboradorId { get; }
    public string Nome { get; }
    public string Cpf { get; }
    public string SituacaoAcesso => EstaNaAcademia ? "Presente" : "Ausente";
    public string Acao => EstaNaAcademia ? "Registrar saída" : "Registrar entrada";

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

    public ColaboradorAcessoItem(ColaboradorDto colaborador, bool estaNaAcademia)
    {
        ColaboradorId = colaborador.Id;
        Nome = colaborador.Nome;
        Cpf = colaborador.Cpf;
        EstaNaAcademia = estaNaAcademia;
    }
}
