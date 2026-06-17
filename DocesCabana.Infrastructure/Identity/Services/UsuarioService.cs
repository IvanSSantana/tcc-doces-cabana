using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Helpers;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Identity.Services;

public class UsuarioService : IUsuarioService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IEmailService emailService,
        ILogger<UsuarioService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<UsuarioDTO> CadastrarUsuario(CadastroDTO dto)
    {
        var usuario = UsuarioMapper.CadastroToEntity(dto);
        var resultado = await _userManager.CreateAsync(usuario, dto.Senha!);

        if (!resultado.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultado));

        return UsuarioMapper.ToDTO(usuario);
    }

    public async Task<UsuarioDTO> BuscarUsuarioPorId(Guid usuarioId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");

        return UsuarioMapper.ToDTO(usuario);
    }

    public async Task<UsuarioDTO?> BuscarPorLogin(string login)
    {
        var usuario = await _userManager.FindByEmailAsync(login);
        
        if (usuario is null)
          usuario= await _userManager.Users.FirstOrDefaultAsync(u => u.CPF == login);
        
        return usuario is null ? null : UsuarioMapper.ToDTO(usuario);
    }

    public async Task<bool> RedefinirSenhaUsuario(UsuarioDTO usuarioDto, string novaSenha)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioDto.Id.ToString());
        
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioDto.Id} não encontrado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, novaSenha);

        return resultado.Succeeded;
    }

    public async Task<UsuarioDTO> AlterarDadosUsuario(UsuarioDTO usuarioDto)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioDto.Id.ToString());
        
        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioDto.Id} não encontrado.");

        usuario.AtualizarDados(usuarioDto.Nome, TelefoneHelper.ApenasDigitos(usuarioDto.Celular), usuarioDto.DataNascimento);
        
        var resultado = await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultado));

        return UsuarioMapper.ToDTO(usuario);
    }

    public async Task<SignInResult> RealizarLogin(string login, string senha, bool lembrarMe)
    {
        var usuario = await BuscarPorLogin(login);
        if (usuario is null)
            return SignInResult.Failed;

        var entity = await _userManager.FindByEmailAsync(login);
        if (entity is null)
            return SignInResult.Failed;

        var resultado = await _signInManager.PasswordSignInAsync(entity.Email!, senha, lembrarMe, lockoutOnFailure: false);
        return resultado;
    }

    public async Task RealizarLogout()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<bool> SolicitarRedefinicaoSenha(string email)
    {
        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario is null)
        {
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var assunto = "Doces Cabana - Redefinição de Senha";
        var corpo = $@"<div>Token: {token}</div>";

        await _emailService.EnviarEmail(email, assunto, corpo);
        return true;
    }

    public async Task<bool> ConfirmarRedefinicaoSenha(string email, string token, string novaSenha)
    {
        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario is null)
            return false;

        var resultado = await _userManager.ResetPasswordAsync(usuario, token, novaSenha);
        return resultado.Succeeded;
    }

    public async Task<bool> ConfirmarEmailDoUsuario(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        var usuario = await _userManager.FindByEmailAsync(email);
        
        if (usuario is null)
            return false;

        var resultado = await _userManager.ConfirmEmailAsync(usuario, token);
        
        if (!resultado.Succeeded)
            return false;
        return true;
    }

    private static string ObterMensagensErro(IdentityResult resultado) =>
        string.Join(" ", resultado.Errors.Select(e => e.Description));
}
