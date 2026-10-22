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
        DatabaseTypePicker.Items.Clear();

        foreach (var tipo in Enum.GetValues<AppDatabaseType>())
        {
            DatabaseTypePicker.Items.Add(tipo.ToString());
        }

        var bancoAtual = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        DatabaseTypePicker.SelectedItem = bancoAtual;

        AtualizarInterfacePorTipoBanco();
    }

    private void OnDatabaseTypeChanged(object? sender, EventArgs? e)
    {
        AtualizarInterfacePorTipoBanco();
    }

    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType))
        {
            return;
        }

        switch (selectedType)
        {
            case AppDatabaseType.Sqlite:
                SqliteInfoCard.IsVisible = true;
                SqliteContainer.IsVisible = true;
                ServidorBancoGrid.IsVisible = false;
                CredenciaisGrid.IsVisible = false;

                ComplementoLabel.Text = "Complemento (ex: Default Timeout=5;)";
                ComplementoEntry.Placeholder = "Default Timeout=5;";

                var defaultSqlitePath = DeviceInfo.Platform == DevicePlatform.WinUI
                    ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                    : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");

                SqliteCaminhoEntry.Text = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultSqlitePath));
                ComplementoEntry.Text = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
                break;
        }
    }

    private async void OnSalvarBdClicked(object? sender, EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType))
        {
            await DisplayAlertAsync("Aviso", "Selecione um tipo de banco de dados válido.", "OK");
            return;
        }

        switch (selectedType)
        {
            case AppDatabaseType.Sqlite:
                if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
                {
                    await DisplayAlertAsync("Validação", "Informe o caminho do arquivo do banco SQLite.", "OK");
                    return;
                }

                var caminho = SqliteCaminhoEntry.Text.Trim();
                var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;

                Preferences.Set("Sqlite_Caminho", caminho);
                Preferences.Set("Sqlite_Complemento", complemento);

                // Chaves de compatibilidade
                Preferences.Set("SqliteCaminho", caminho);
                Preferences.Set("Complemento", complemento);
                break;

            default:
                await DisplayAlertAsync("Aviso", "O tipo de banco de dados selecionado não é compatível.", "OK");
                return;
        }

        Preferences.Set("DatabaseType", selectedType.ToString());

        // Disparar a mensagem para recarga das credenciais (processada pelo ConfigurationHelper)
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));

        await DisplayAlertAsync("Sucesso", $"Configurações do banco de dados ({selectedType}) salvas com sucesso!", "OK");

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
