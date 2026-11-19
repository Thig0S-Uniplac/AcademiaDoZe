using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoAcessoPage : ContentPage
{
    public AlunoAcessoPage(AlunoAcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AlunoAcessoViewModel viewModel)
            await viewModel.LoadAlunosCommand.ExecuteAsync(null);
    }
}
