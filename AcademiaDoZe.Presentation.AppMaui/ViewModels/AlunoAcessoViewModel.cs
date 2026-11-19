using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoAcessoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IMatriculaService _matriculaService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private string _searchText = string.Empty;

    public ObservableCollection<AlunoAcessoItem> Alunos { get; } = [];
    public ObservableCollection<AlunoAcessoItem> AlunosFiltrados { get; } = [];
    public ObservableCollection<AcessoHojeItem> AcessosHoje { get; } = [];

    private bool _exibindoAcessosHoje;
    public bool ExibindoAcessosHoje
    {
        get => _exibindoAcessosHoje;
        set
        {
            if (SetProperty(ref _exibindoAcessosHoje, value))
                OnPropertyChanged(nameof(ExibindoAlunos));
        }
    }

    public bool ExibindoAlunos => !ExibindoAcessosHoje;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                FiltrarAlunos();
        }
    }

    public AlunoAcessoViewModel(
        IAlunoService alunoService,
        IMatriculaService matriculaService,
        IAcessoAlunoService acessoAlunoService)
    {
        _alunoService = alunoService;
        _matriculaService = matriculaService;
        _acessoAlunoService = acessoAlunoService;
        Title = "Acesso de Alunos";
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var alunosTask = _alunoService.ObterTodosAsync(cts.Token);
            var matriculasTask = _matriculaService.ObterAtivasAsync(cancellationToken: cts.Token);
            var dataHoje = DateOnly.FromDateTime(DateTime.Today);
            var acessosTask = _acessoAlunoService.ObterPorAlunoPeriodoAsync(
                inicio: dataHoje,
                fim: dataHoje,
                cancellationToken: cts.Token);
            await Task.WhenAll(alunosTask, matriculasTask, acessosTask);

            var alunos = (await alunosTask).ToList();
            var alunosMatriculados = (await matriculasTask)
                .Select(matricula => matricula.AlunoMatricula.Id)
                .ToHashSet();
            var estadosPorAluno = new Dictionary<int, bool>();
            foreach (var aluno in alunos)
                estadosPorAluno[aluno.Id] = await _acessoAlunoService.EstaNaAcademiaAsync(aluno.Id, cts.Token);
            var nomesPorAluno = alunos.ToDictionary(aluno => aluno.Id, aluno => aluno.Nome);
            var acessosHoje = AcessoHojeItem.Criar(
                (await acessosTask).Select(acesso => (acesso.Id, acesso.AlunoId, acesso.DataHora)),
                nomesPorAluno);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Alunos.Clear();
                foreach (var aluno in alunos)
                {
                    Alunos.Add(new AlunoAcessoItem(
                        aluno,
                        alunosMatriculados.Contains(aluno.Id),
                        estadosPorAluno[aluno.Id]));
                }

                FiltrarAlunos();
                AcessosHoje.Clear();
                foreach (var acesso in acessosHoje)
                    AcessosHoje.Add(acesso);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O carregamento dos alunos expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar os acessos dos alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync(AlunoAcessoItem aluno)
    {
        if (aluno is null || IsBusy)
            return;

        var registrandoSaida = aluno.EstaNaAcademia;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var dataHora = DateTime.Now;
            AcessoAlunoDto? ultimoAcesso = registrandoSaida
                ? await _acessoAlunoService.ObterUltimoAcessoAsync(aluno.AlunoId, cts.Token)
                : null;

            var acessoRegistrado = await _acessoAlunoService.AdicionarAsync(new AcessoAlunoDto
            {
                AlunoId = aluno.AlunoId,
                DataHora = dataHora
            }, cts.Token);

            aluno.EstaNaAcademia = !registrandoSaida;
            AcessosHoje.Insert(0, new AcessoHojeItem(
                acessoRegistrado.Id,
                aluno.Nome,
                dataHora,
                registrandoSaida ? "Saída" : "Entrada"));
            var mensagem = registrandoSaida && ultimoAcesso is not null
                ? $"Saída de {aluno.Nome} registrada. Permanência: {dataHora - ultimoAcesso.DataHora:hh\\:mm\\:ss}."
                : $"{(registrandoSaida ? "Saída" : "Entrada")} de {aluno.Nome} registrada às {dataHora:HH:mm}.";
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
    private void MostrarAlunos() => ExibindoAcessosHoje = false;

    [RelayCommand]
    private void MostrarAcessosHoje() => ExibindoAcessosHoje = true;

    private void FiltrarAlunos()
    {
        var termo = SearchText.Trim();
        var resultados = string.IsNullOrEmpty(termo)
            ? Alunos
            : Alunos.Where(aluno =>
                aluno.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                aluno.Cpf.Contains(termo, StringComparison.OrdinalIgnoreCase));

        AlunosFiltrados.Clear();
        foreach (var aluno in resultados)
            AlunosFiltrados.Add(aluno);
    }
}
