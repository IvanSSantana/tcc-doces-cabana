using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DocesCabana.Infrastructure.Identity.Services;

public class UsuarioServices : IUsuarioServices
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuarioServices> _logger;

    public UsuarioServices(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IEmailService emailService,
        ILogger<UsuarioServices> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger;
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

    public async Task<Usuario?> BuscarPorLogin(string login)
    {
        var buscarUsuarioPorEmail = await _userManager.FindByEmailAsync(login);
        var buscarUsuarioPorCPF = _userManager.Users.FirstOrDefault(user => user.CPF == login);
        
        return buscarUsuarioPorEmail ?? buscarUsuarioPorCPF;
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

    public async Task<bool> SolicitarRedefinicaoSenha(string email)
    {
        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario is null)
        {
            _logger.LogWarning($"Usuário com e-mail {email} não encontrado.");
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

        var assunto = "Doces Cabana - Redefinição de Senha";
        var corpo = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f9f9f9; padding: 40px 20px; text-align: center; min-height: 100%;"">
    <div style=""max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 16px; box-shadow: 0 8px 30px rgba(0, 0, 0, 0.04); overflow: hidden; border: 1px solid #f0f0f0;"">
        <!-- Banner de Cabeçalho -->
        <div style=""background: linear-gradient(135deg, #FF6B8B 0%, #FF8E53 100%); padding: 40px 30px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 32px; font-weight: 800; letter-spacing: -0.5px; text-shadow: 0 2px 4px rgba(0,0,0,0.1);"">Doces Cabana</h1>
            <p style=""color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 14px; text-transform: uppercase; letter-spacing: 2px; font-weight: 600;"">Portal do Cliente</p>
        </div>
        
        <!-- Conteúdo do E-mail -->
        <div style=""padding: 40px 35px; text-align: left; color: #4A4A4A; line-height: 1.6;"">
            <h2 style=""color: #2D2D2D; margin-top: 0; font-size: 22px; font-weight: 700; letter-spacing: -0.3px;"">Recuperação de Senha</h2>
            <p style=""font-size: 16px;"">Olá, <strong>{usuario.Nome}</strong>!</p>
            <p style=""font-size: 15px;"">Recebemos uma solicitação para redefinir a senha da sua conta no portal <strong>Doces Cabana</strong>. Se você não solicitou essa alteração, fique tranquilo(a), nenhuma ação é necessária e você pode desconsiderar este e-mail.</p>
            
            <p style=""font-size: 15px; margin-top: 25px;"">Para concluir a redefinição de sua senha, utilize o token exclusivo de segurança apresentado abaixo:</p>
            
            <!-- Caixa de Destaque do Token -->
            <div style=""background-color: #FAF5F6; border: 2px dashed #FF6B8B; padding: 20px; border-radius: 12px; text-align: center; font-family: 'Consolas', 'Courier New', monospace; font-size: 22px; font-weight: 700; color: #FF6B8B; letter-spacing: 2px; margin: 30px 0; word-break: break-all; box-shadow: inset 0 2px 4px rgba(255, 107, 139, 0.02);"">
                {token}
            </div>
            
            <p style=""font-size: 13px; color: #8C8C8C; margin-top: 30px; border-top: 1px solid #F0F0F0; padding-top: 20px; line-height: 1.5;"">
                <strong>Atenção:</strong> Por razões de privacidade e segurança, este código de validação expira em breve e é válido apenas para uma única tentativa de recuperação.
            </p>
        </div>
        
        <!-- Rodapé do E-mail -->
        <div style=""background-color: #FAF8F9; padding: 25px; text-align: center; border-top: 1px solid #F3ECEF; font-size: 12px; color: #A09699;"">
            <p style=""margin: 0 0 8px 0;"">Este é um e-mail automático enviado pelo sistema. Por favor, não responda.</p>
            <p style=""margin: 0; font-weight: 600;"">&copy; {DateTime.Now.Year} Doces Cabana. Todos os direitos reservados.</p>
        </div>
    </div>
</div>";

        await _emailService.EnviarEmailAsync(email, assunto, corpo);
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

    private static string ObterMensagensErro(IdentityResult resultado) =>
        string.Join(" ", resultado.Errors.Select(e => e.Description));
}
