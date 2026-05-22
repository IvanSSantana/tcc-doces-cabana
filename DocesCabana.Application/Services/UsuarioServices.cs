using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs.Auth;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Application.Services;

public class UsuarioServices : IUsuarioServices
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioServices(
        UserManager<Usuario> userManager,
        IUsuarioRepository usuarioRepository)
    {
        _userManager = userManager;
        _usuarioRepository = usuarioRepository;
    }

    public Usuario CadastrarUsuario(CadastroDTO dto)
    {
        var usuario = UsuarioMapper.ToEntity(dto);

        var resultado = _userManager.CreateAsync(usuario, dto.Senha).GetAwaiter().GetResult();

        if (!resultado.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultado));

        return usuario;
    }

    public Usuario BuscarUsuarioPorId(Guid usuarioId)
    {
        var usuario = _usuarioRepository.BuscarPorIdAsync(usuarioId).GetAwaiter().GetResult();

        if (usuario is null)
            throw new KeyNotFoundException($"Usuário com ID {usuarioId} não encontrado.");

        return usuario;
    }

    public bool RedefinirSenhaUsuario(Usuario usuario, string novaSenha)
    {
        var token = _userManager.GeneratePasswordResetTokenAsync(usuario).GetAwaiter().GetResult();
        var resultado = _userManager.ResetPasswordAsync(usuario, token, novaSenha).GetAwaiter().GetResult();

        return resultado.Succeeded;
    }

    public Usuario AlterarDadosUsuario(Usuario usuario)
    {
        var resultado = _userManager.UpdateAsync(usuario).GetAwaiter().GetResult();

        if (!resultado.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultado));

        return usuario;
    }

    private static string ObterMensagensErro(IdentityResult resultado) =>
        string.Join(" ", resultado.Errors.Select(e => e.Description));
}
