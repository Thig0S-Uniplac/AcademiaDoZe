using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorAcessoViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;
    private string _searchText = string.Empty;

    public ObservableCollection<ColaboradorAcessoItem> Colaboradores { get; } = [];
    public ObservableCollection<ColaboradorAcessoItem> ColaboradoresFiltrados { get; } = [];
    public ObservableCollection<AcessoHojeItem> AcessosHoje { get; } = [];

    private bool _exibindoAcessosHoje;
    public bool ExibindoAcessosHoje
    {
        get => _exibindoAcessosHoje;
        set
        {
            if (SetProperty(ref _exibindoAcessosHoje, value))
                OnPropertyChanged(nameof(ExibindoColaboradores));
        }
    }

    public bool ExibindoColaboradores => !ExibindoAcessosHoje;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                FiltrarColaboradores();
        }
    }

    public ColaboradorAcessoViewModel(
        IColaboradorService colaboradorService,
        IAcessoColaboradorService acessoColaboradorService)
    {
        _colaboradorService = colaboradorService;
        _acessoColaboradorService = acessoColaboradorService;
        Title = "Acesso de Colaboradores";
    }

    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var colaboradores = (await _colaboradorService.ObterTodosAsync(cts.Token)).ToList();
            var dataHoje = DateOnly.FromDateTime(DateTime.Today);
            var acessos = await _acessoColaboradorService.ObterPorColaboradorPeriodoAsync(
                inicio: dataHoje,
                fim: dataHoje,
                cancellationToken: cts.Token);
            var estadosPorColaborador = new Dictionary<int, bool>();
            foreach (var colaborador in colaboradores)
                estadosPorColaborador[colaborador.Id] = await _acessoColaboradorService.EstaNaAcademiaAsync(colaborador.Id, cts.Token);
            var nomesPorColaborador = colaboradores.ToDictionary(colaborador => colaborador.Id, colaborador => colaborador.Nome);
            var acessosHoje = AcessoHojeItem.Criar(
                acessos.Select(acesso => (acesso.Id, acesso.ColaboradorId, acesso.DataHora)),
                nomesPorColaborador);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();
                foreach (var colaborador in colaboradores)
                    Colaboradores.Add(new ColaboradorAcessoItem(colaborador, estadosPorColaborador[colaborador.Id]));

                FiltrarColaboradores();
                AcessosHoje.Clear();
                foreach (var acesso in acessosHoje)
                    AcessosHoje.Add(acesso);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O carregamento dos colaboradores expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar os acessos dos colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync(ColaboradorAcessoItem colaborador)
    {
        if (colaborador is null || IsBusy)
            return;

        var registrandoSaida = colaborador.EstaNaAcademia;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var dataHora = DateTime.Now;
            AcessoColaboradorDto? ultimoAcesso = registrandoSaida
                ? await _acessoColaboradorService.ObterUltimoAcessoAsync(colaborador.ColaboradorId, cts.Token)
                : null;

            var acessoRegistrado = await _acessoColaboradorService.AdicionarAsync(new AcessoColaboradorDto
            {
                ColaboradorId = colaborador.ColaboradorId,
                DataHora = dataHora
            }, cts.Token);

            colaborador.EstaNaAcademia = !registrandoSaida;
            AcessosHoje.Insert(0, new AcessoHojeItem(
                acessoRegistrado.Id,
                colaborador.Nome,
                dataHora,
                registrandoSaida ? "Saída" : "Entrada"));
            var mensagem = registrandoSaida && ultimoAcesso is not null
                ? $"Saída de {colaborador.Nome} registrada. Permanência: {dataHora - ultimoAcesso.DataHora:hh\\:mm\\:ss}."
                : $"{(registrandoSaida ? "Saída" : "Entrada")} de {colaborador.Nome} registrada às {dataHora:HH:mm}.";
            await Shell.Current.DisplayAlertAsync("Acesso registrado", mensagem, "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O registro do acesso expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Não foi possível registrar", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void MostrarColaboradores() => ExibindoAcessosHoje = false;

    [RelayCommand]
    private void MostrarAcessosHoje() => ExibindoAcessosHoje = true;

    private void FiltrarColaboradores()
    {
        var termo = SearchText.Trim();
        var resultados = string.IsNullOrEmpty(termo)
            ? Colaboradores
            : Colaboradores.Where(colaborador =>
                colaborador.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                colaborador.Cpf.Contains(termo, StringComparison.OrdinalIgnoreCase));

        ColaboradoresFiltrados.Clear();
        foreach (var colaborador in resultados)
            ColaboradoresFiltrados.Add(colaborador);
    }
}
