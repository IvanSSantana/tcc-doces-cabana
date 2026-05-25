using DocesCabana.Application.DTOs.Auth;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity.Services;

public class UsuarioServices : IUsuarioServices
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;

    public UsuarioServices(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Usuario> CadastrarUsuario(CadastroDTO dto)
    {
        var usuario = UsuarioMapper.ToEntity(dto);

        var resultado = await _userManager.CreateAsync(usuario, dto.Senha);

        if (!resultado.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultado));

        return usuario;
    }

    public async Task<Usuario> BuscarUsuarioPorId(Guid usuarioId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");

        return usuario;
    }

    public async Task<bool> RedefinirSenhaUsuario(Usuario usuario, string novaSenha)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, novaSenha);

        return resultado.Succeeded;
    }

    public async Task<Usuario> AlterarDadosUsuario(Usuario usuario)
    {
        var resultado = await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultado));

        return usuario;
    }

    public async Task<SignInResult> RealizarLogin(string email, string senha, bool lembrarMe)
    {
        var resultado = await _signInManager.PasswordSignInAsync(email, senha, lembrarMe, lockoutOnFailure: false);
        return resultado;
    }

    public async Task RealizarLogout()
    {
        await _signInManager.SignOutAsync();
    }

    public Task<bool> SolicitarRedefinicaoSenha(string email)
    {
        throw new NotImplementedException();
    }

    public Task<string> GerarTokenRedefinicaoSenha(string email)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConfirmarRedefinicaoSenha(string email, string token, string novaSenha)
    {
        throw new NotImplementedException();
    }

    private static string ObterMensagensErro(IdentityResult resultado) =>
        string.Join(" ", resultado.Errors.Select(e => e.Description));
}
