using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    public ObservableCollection<string> FilterTypes { get; } = ["Todas", "Id", "Id Aluno", "Ativas", "Vencendo em dias"];
    private readonly IMatriculaService _matriculaService;
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }
    private string _selectedFilterType = "Todas";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }
    public ObservableCollection<MatriculaDto> Matriculas { get; } = [];
    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = "Matrículas";
    }
    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("matricula");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }
    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDto matricula)
    {
        try
        {
            if (matricula == null)
                return;
            await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }
        IsRefreshing = true;
        try
        {
            await SearchMatriculasAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            IEnumerable<MatriculaDto> resultados = [];
            // Busca as matriculas de acordo com o filtro
            if (SelectedFilterType == "Todas")
            {
                resultados = await _matriculaService.ObterTodasAsync(cts.Token);
            }
            else if (string.IsNullOrWhiteSpace(SearchText))
            {
                if (SelectedFilterType == "Ativas")
                {
                    resultados = await _matriculaService.ObterAtivasAsync(0, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Informe um valor para o filtro selecionado.", "OK");
                    return;
                }
            }
            else if (SelectedFilterType == "Id")
            {
                if (int.TryParse(SearchText.Trim(), out int id) && id > 0)
                {
                    var matricula = await _matriculaService.ObterPorIdAsync(id, cts.Token);
                    if (matricula != null)
                        resultados = [matricula];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID, informe um número válido.", "OK");
                    return;
                }
            }
            else if (SelectedFilterType == "Id Aluno")
            {
                if (int.TryParse(SearchText.Trim(), out int alunoId) && alunoId > 0)
                {
                    resultados = await _matriculaService.ObterPorAlunoIdAsync(alunoId, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID do Aluno, informe um número válido.", "OK");
                    return;
                }
            }
            else if (SelectedFilterType == "Ativas")
            {
                if (!int.TryParse(SearchText.Trim(), out int alunoId) || alunoId <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Informe um ID de aluno inteiro positivo ou deixe vazio para todas as ativas.", "OK");
                    return;
                }
                resultados = await _matriculaService.ObterAtivasAsync(alunoId, cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Vencendo em dias")
            {
                if (int.TryParse(SearchText.Trim(), out int dias) && dias >= 0)
                {
                    resultados = await _matriculaService.ObterVencendoEmDiasAsync(dias, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por vencimento, informe a quantidade de dias (número inteiro).", "OK");
                    return;
                }
            }
            // Atualiza a coleção na thread principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();
                foreach (var item in resultados)
                {
                    Matriculas.Add(item);
                }
                OnPropertyChanged(nameof(Matriculas));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de matrículas expirou. Tente novamente.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        await SearchMatriculasAsync();
    }
    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto matricula)
    {
        if (IsBusy || matricula == null)
            return;
        bool confirm = await Shell.Current.DisplayAlertAsync(
        "Confirmar Exclusão",
        $"Deseja realmente excluir a matrícula {matricula.Id}?",
        "Sim", "Não");
        if (!confirm)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _matriculaService.RemoverAsync(matricula.Id, cts.Token);
            if (success)
            {
                Matriculas.Remove(matricula);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula excluída com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir a matrícula.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão da matrícula expirou. Atualize a lista antes de tentar novamente.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

}
