using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    private AlunoDto _aluno = CriarAlunoPadrao();
    public AlunoDto Aluno
    {
        get => _aluno;
        set
        {
            if (SetProperty(ref _aluno, value))
                NotificarStatusEndereco();
        }
    }

    private int _alunoId;
    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _confirmarSenha = string.Empty;
    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public bool HasEnderecoVinculado => Aluno.Endereco?.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public AlunoViewModel(IAlunoService alunoService, ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = "Detalhes do Aluno";
    }

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;
        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar Aluno";
            await LoadAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Aluno";
            Aluno = CriarAlunoPadrao();
        }
    }

    private static AlunoDto CriarAlunoPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Endereco = NovoEndereco(),
        Numero = string.Empty,
        Email = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty
    };

    private static LogradouroDto NovoEndereco() => new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    private void NotificarStatusEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadAlunoAsync()
    {
        if (AlunoId <= 0)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var aluno = await _alunoService.ObterPorIdAsync(AlunoId, cts.Token);
            if (aluno is null)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno não encontrado.", "OK");
                return;
            }

            aluno.Endereco ??= NovoEndereco();
            aluno.Senha = string.Empty;
            Aluno = aluno;
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        var cpf = new string((Aluno.Cpf ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cpf.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "Informe um CPF com 11 dígitos numéricos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var aluno = await _alunoService.ObterPorCpfAsync(cpf, cts.Token);
            if (aluno is null)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CPF não encontrado.", "OK");
                return;
            }

            AlunoId = aluno.Id;
            await LoadAlunoAsync();
            IsEditMode = true;
            Title = "Editar Aluno";
            await Shell.Current.DisplayAlertAsync("Aviso", "Aluno já cadastrado! Dados carregados para edição.", "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca por CPF expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        var cep = new string((Aluno.Endereco?.Cep ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cep.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "Informe um CEP com 8 dígitos numéricos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var endereco = await _logradouroService.ObterPorCepAsync(cep, cts.Token);
            if (endereco is null)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP não encontrado. Cadastre o logradouro antes de vincular o aluno.", "OK");
                return;
            }

            Aluno.Endereco = endereco;
            OnPropertyChanged(nameof(Aluno));
            NotificarStatusEndereco();
            await Shell.Current.DisplayAlertAsync("Aviso", "CEP encontrado! Endereço preenchido.", "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca do CEP expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAlunoAsync()
    {
        if (IsBusy || !await ValidarAlunoAsync())
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (Aluno.Endereco is null || Aluno.Endereco.Id <= 0)
            {
                var cep = new string((Aluno.Endereco?.Cep ?? string.Empty).Where(char.IsDigit).ToArray());
                var endereco = cep.Length == 8
                    ? await _logradouroService.ObterPorCepAsync(cep, cts.Token)
                    : null;

                if (endereco is null)
                {
                    await Shell.Current.DisplayAlertAsync("Endereço Obrigatório", "O CEP informado não está cadastrado como logradouro. Cadastre-o primeiro.", "OK");
                    return;
                }

                Aluno.Endereco = endereco;
            }

            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = new string(Aluno.Cpf.Where(char.IsDigit).ToArray());
            Aluno.Telefone = new string(Aluno.Telefone.Where(char.IsDigit).ToArray());
            Aluno.Numero = Aluno.Numero.Trim();
            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email) ? null : Aluno.Email.Trim();
            Aluno.Complemento = string.IsNullOrWhiteSpace(Aluno.Complemento) ? null : Aluno.Complemento.Trim();
            Aluno.Senha = string.IsNullOrWhiteSpace(Aluno.Senha) ? null : Aluno.Senha.Trim();

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno atualizado com sucesso!", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno cadastrado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync("Regra de Negócio", ex.Message, "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync("Validação", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        try
        {
            var escolha = await Shell.Current.DisplayActionSheetAsync("Origem da Imagem", "Cancelar", "Remover Foto", "Galeria", "Câmera");
            if (escolha == "Remover Foto")
            {
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
                return;
            }

            FileResult? arquivo = escolha switch
            {
                "Galeria" => await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione uma imagem",
                    FileTypes = FilePickerFileType.Images
                }),
                "Câmera" when MediaPicker.Default.IsCaptureSupported => await MediaPicker.Default.CapturePhotoAsync(),
                "Câmera" => null,
                _ => null
            };

            if (escolha == "Câmera" && arquivo is null)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Captura de foto não suportada neste dispositivo.", "OK");
                return;
            }

            if (arquivo is null)
                return;

            await using var stream = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria);
            Aluno.Foto = new ArquivoDto { Conteudo = memoria.ToArray() };
            OnPropertyChanged(nameof(Aluno));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }

    private async Task<bool> ValidarAlunoAsync()
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(Aluno.Nome))
            erros.Add("• Nome é obrigatório.");

        var cpf = new string((Aluno.Cpf ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cpf.Length != 11)
            erros.Add("• CPF deve conter exatamente 11 dígitos numéricos.");

        if (Aluno.DataNascimento == default)
            erros.Add("• Data de nascimento é obrigatória.");
        else if (Aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
            erros.Add("• O aluno deve ter no mínimo 12 anos de idade.");

        var telefone = new string((Aluno.Telefone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (telefone.Length != 11)
            erros.Add("• Telefone deve conter 11 dígitos numéricos com DDD.");

        if (!string.IsNullOrWhiteSpace(Aluno.Email) &&
            (!Aluno.Email.Contains('@') || Aluno.Email.StartsWith('@') || Aluno.Email.EndsWith('@')))
            erros.Add("• Formato de e-mail inválido.");

        if (!IsEditMode && string.IsNullOrWhiteSpace(Aluno.Senha))
            erros.Add("• Senha é obrigatória no cadastro.");

        if (!string.IsNullOrWhiteSpace(Aluno.Senha))
        {
            if (Aluno.Senha.Trim().Length < 6 || !Aluno.Senha.Any(char.IsUpper))
                erros.Add("• A senha deve ter no mínimo 6 caracteres e pelo menos uma letra maiúscula.");
            if (Aluno.Senha != ConfirmarSenha)
                erros.Add("• A confirmação de senha não confere com a senha informada.");
        }

        if (Aluno.Endereco is null || string.IsNullOrWhiteSpace(Aluno.Endereco.Cep) ||
            new string(Aluno.Endereco.Cep.Where(char.IsDigit).ToArray()).Length != 8)
            erros.Add("• CEP do endereço deve conter 8 dígitos numéricos.");

        if (string.IsNullOrWhiteSpace(Aluno.Numero))
            erros.Add("• Número do endereço é obrigatório.");

        if (erros.Count == 0)
            return true;

        await Shell.Current.DisplayAlertAsync("Erros de Validação", "Corrija os seguintes campos:\n\n" + string.Join("\n", erros), "OK");
        return false;
    }
}
