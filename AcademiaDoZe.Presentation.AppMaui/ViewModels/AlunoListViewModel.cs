using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "Id", "Email"];

    public ObservableCollection<AlunoDto> Alunos { get; } = [];

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task AddAlunoAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("aluno");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao abrir o cadastro do aluno: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditAlunoAsync(AlunoDto aluno)
    {
        if (aluno is null)
            return;

        try
        {
            await Shell.Current.GoToAsync($"aluno?Id={aluno.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao abrir o aluno para edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        IsRefreshing = true;
        await LoadAlunosAsync();
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunos = await _alunoService.ObterTodosAsync(cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Alunos.Clear();
                foreach (var aluno in alunos)
                    Alunos.Add(aluno);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento dos alunos expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            IEnumerable<AlunoDto> resultados;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _alunoService.ObterTodosAsync(cts.Token);
            }
            else
            {
                var termo = SearchText.Trim();
                switch (SelectedFilterType)
                {
                    case "Nome":
                        resultados = await _alunoService.ObterPorNomeAsync(termo, cts.Token);
                        break;
                    case "CPF":
                        var cpf = new string(termo.Where(char.IsDigit).ToArray());
                        if (cpf.Length != 11)
                        {
                            await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por CPF, informe os 11 dígitos numéricos.", "OK");
                            return;
                        }

                        var alunoPorCpf = await _alunoService.ObterPorCpfAsync(cpf, cts.Token);
                        resultados = alunoPorCpf is null ? [] : [alunoPorCpf];
                        break;
                    case "Id":
                        if (!int.TryParse(termo, out var id) || id <= 0)
                        {
                            await Shell.Current.DisplayAlertAsync("Validação", "Informe um ID inteiro positivo.", "OK");
                            return;
                        }

                        var alunoPorId = await _alunoService.ObterPorIdAsync(id, cts.Token);
                        resultados = alunoPorId is null ? [] : [alunoPorId];
                        break;
                    case "Email":
                        var alunoPorEmail = await _alunoService.ObterPorEmailAsync(termo, cts.Token);
                        resultados = alunoPorEmail is null ? [] : [alunoPorEmail];
                        break;
                    default:
                        throw new InvalidOperationException($"Filtro de aluno não suportado: {SelectedFilterType}.");
                }
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Alunos.Clear();
                foreach (var aluno in resultados)
                    Alunos.Add(aluno);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de alunos expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAlunoAsync(AlunoDto aluno)
    {
        if (aluno is null)
            return;

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Sim",
            "Não");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (await _alunoService.RemoverAsync(aluno.Id, cts.Token))
            {
                Alunos.Remove(aluno);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o aluno.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Não Permitido",
                    "Este aluno não pode ser excluído pois possui registros vinculados no sistema (ex: matrículas ou acessos).",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir aluno: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
