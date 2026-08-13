using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Application.Mensagens;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Helpers;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocesCabana.Infrastructure.Identity.Services;

public class UsuarioService : IUsuarioService
{
    private readonly UserManager<ContaDeAcesso> _userManager;
    private readonly SignInManager<ContaDeAcesso> _signInManager;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        UserManager<ContaDeAcesso> userManager,
        SignInManager<ContaDeAcesso> signInManager,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<UsuarioService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<UsuarioDTO> CadastrarUsuario(CadastroDTO dto, string? papel = null)
    {
        var conta = new ContaDeAcesso(dto.Email!);
        var resultado = await _userManager.CreateAsync(conta, dto.Senha!);

        if (!resultado.Succeeded)
        {
            var erroDuplicidade = resultado.Errors.Any(e =>
                e.Code == "DuplicateUserName" ||
                e.Code == "DuplicateEmail");

            if (erroDuplicidade)
                throw new InvalidOperationException(MensagensCadastro.DadosJaAssociados);

            throw new InvalidOperationException(
                ObterMensagensErro(resultado));
        }

        // A partir daqui, se qualquer coisa falhar — cadastro do usuário ou
        // atribuição do papel —, a conta já criada é desfeita: não deixamos
        // credencial sem cadastro nem conta de cliente que ninguém pediu
        // (RN-08 da spec 004, RN-05 da spec 005).
        try
        {
            var usuario = new Usuario(conta.Id, dto.Nome!, dto.CPF!, dto.Celular!, dto.DataNascimento ?? new DateTime());
            await _usuarioRepository.Adicionar(usuario);
            await _unitOfWork.SalvarAlteracoes();

            if (!string.IsNullOrWhiteSpace(papel))
            {
                var resultadoPapel = await _userManager.AddToRoleAsync(conta, papel);
                if (!resultadoPapel.Succeeded)
                    throw new InvalidOperationException(ObterMensagensErro(resultadoPapel));
            }

            return UsuarioMapper.ToDTO(usuario, conta);
        }
        catch (DbUpdateException)
        {
            // ContaJaExiste não pega a corrida entre duas requisições
            // simultâneas com o mesmo CPF — quem pega é o índice único, aqui.
            // A consulta vem antes do DeleteAsync de propósito: ela não
            // descarrega o ChangeTracker, e o DeleteAsync grava (spec 006).
            var cpf = CpfHelper.ApenasDigitos(dto.CPF!);
            var colisaoDeCpf = await _usuarioRepository.BuscarPorCpf(cpf) is not null;

            await _userManager.DeleteAsync(conta);

            if (colisaoDeCpf)
                throw new InvalidOperationException(MensagensCadastro.DadosJaAssociados);

            throw;
        }
        catch
        {
            await _userManager.DeleteAsync(conta);
            throw;
        }
    }

    public async Task<bool> ContaJaExiste(string email, string cpf) =>
        await BuscarPorLogin(email) is not null || await BuscarPorLogin(cpf) is not null;

    public async Task<UsuarioDTO> BuscarUsuarioPorId(Guid usuarioId)
    {
        var conta = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (conta is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");

        var usuario = await _usuarioRepository.BuscarPorId(usuarioId);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");

        return UsuarioMapper.ToDTO(usuario, conta);
    }

    public async Task<UsuarioDTO?> BuscarPorLogin(string login)
    {
        var resolvido = await ResolverUsuario(login);

        return resolvido is null ? null : UsuarioMapper.ToDTO(resolvido.Value.Usuario, resolvido.Value.Conta);
    }

    private async Task<(Usuario Usuario, ContaDeAcesso Conta)?> ResolverUsuario(string login)
    {
        var conta = await _userManager.FindByEmailAsync(login);

        if (conta is not null)
        {
            var usuarioPorConta = await _usuarioRepository.BuscarPorId(conta.Id);
            return usuarioPorConta is null ? null : (usuarioPorConta, conta);
        }

        var cpf = CpfHelper.ApenasDigitos(login);
        var usuario = await _usuarioRepository.BuscarPorCpf(cpf);
        if (usuario is null)
            return null;

        var contaDoUsuario = await _userManager.FindByIdAsync(usuario.UsuarioId.ToString());
        return contaDoUsuario is null ? null : (usuario, contaDoUsuario);
    }

    public async Task<UsuarioDTO> AlterarDadosUsuario(UsuarioDTO usuarioDto)
    {
        var conta = await _userManager.FindByIdAsync(usuarioDto.Id.ToString());
        if (conta is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioDto.Id} não encontrado.");

        var usuario = await _usuarioRepository.BuscarPorId(usuarioDto.Id);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioDto.Id} não encontrado.");

        usuario.AtualizarDados(usuarioDto.Nome, TelefoneHelper.ApenasDigitos(usuarioDto.Celular), usuarioDto.DataNascimento);

        _usuarioRepository.Atualizar(usuario);
        await _unitOfWork.SalvarAlteracoes();

        return UsuarioMapper.ToDTO(usuario, conta);
    }

    public async Task<SignInResult> RealizarLogin(string login, string senha, bool lembrarMe)
    {
        var resolvido = await ResolverUsuario(login);
        if (resolvido is null)
            return SignInResult.Failed;

        var resultado = await _signInManager.PasswordSignInAsync(resolvido.Value.Conta.Email!, senha, lembrarMe, lockoutOnFailure: true);
        return resultado;
    }

    public async Task RealizarLogout()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<string> GerarTokenRedefinicaoSenha(string email)
    {
        var conta = await _userManager.FindByEmailAsync(email);
        if (conta is null)
            throw new KeyNotFoundException($"Usuário com e-mail {email} não encontrado.");

        return await _userManager.GeneratePasswordResetTokenAsync(conta);
    }

    public async Task<bool> SolicitarRedefinicaoSenha(string email, string corpo)
    {
        var conta = await _userManager.FindByEmailAsync(email);
        if (conta is null)
        {
            return false;
        }

        var assunto = "Doces Cabana - Redefinição de Senha";

        await _emailService.EnviarEmail(email, assunto, corpo);
        return true;
    }

    public async Task<bool> ConfirmarRedefinicaoSenha(string email, string token, string novaSenha)
    {
        var conta = await _userManager.FindByEmailAsync(email);
        if (conta is null)
            return false;

        var resultado = await _userManager.ResetPasswordAsync(conta, token, novaSenha);
        return resultado.Succeeded;
    }

    public async Task<bool> ConfirmarEmailDoUsuario(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        var conta = await _userManager.FindByEmailAsync(email);

        if (conta is null)
            return false;

        var resultado = await _userManager.ConfirmEmailAsync(conta, token);

        if (!resultado.Succeeded)
            return false;
        return true;
    }

    private static string ObterMensagensErro(IdentityResult resultado) =>
        string.Join(" ", resultado.Errors.Select(e => e.Description));
}
