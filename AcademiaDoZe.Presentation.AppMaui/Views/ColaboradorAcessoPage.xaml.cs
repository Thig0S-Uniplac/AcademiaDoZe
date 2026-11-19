using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorAcessoPage : ContentPage
{
    public ColaboradorAcessoPage(ColaboradorAcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorAcessoViewModel viewModel)
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
    }
}
