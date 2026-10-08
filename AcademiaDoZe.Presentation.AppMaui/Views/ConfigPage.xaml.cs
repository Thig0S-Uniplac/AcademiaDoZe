using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
    }

    #region Tema

    private void CarregarTema()
    {
        // uso de expressão switch para carregar o índice selecionado
        TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch { "light" => 0, "dark" => 1, _ => 2, };
    }

    private async void OnSalvarTemaClicked(object? sender, EventArgs e)
    {
        string selectedTheme = TemaPicker.SelectedIndex switch { 0 => "light", 1 => "dark", _ => "system" };

        Preferences.Set("Tema", selectedTheme);

        // Disparar mensagem para aplicar o tema imediatamente (App.xaml.cs está escutando)
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));

        await DisplayAlertAsync("Sucesso", "Tema salvo com sucesso!", "OK");

        await Shell.Current.GoToAsync("//dashboard");
    }

    #endregion

    #region Banco de Dados

    private void CarregarBanco()
    {
        var defaultSqlitePath = Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
        SqliteCaminhoEntry.Text = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultSqlitePath));
    }

    private async void OnSalvarBdClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
        {
            await DisplayAlertAsync("Validação", "Informe o caminho do arquivo do banco SQLite.", "OK");
            return;
        }

        var caminho = SqliteCaminhoEntry.Text.Trim();
        Preferences.Set("Sqlite_Caminho", caminho);
        Preferences.Set("SqliteCaminho", caminho);
        Preferences.Set("DatabaseType", AppDatabaseType.Sqlite.ToString());

        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));

        await DisplayAlertAsync("Sucesso", "Configurações do banco SQLite salvas com sucesso!", "OK");

        await Shell.Current.GoToAsync("//dashboard");
    }

    #endregion

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard");
    }

    // Ao fechar a página, desinscreve o mensageiro para evitar vazamentos de memória (memory leaks)
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
