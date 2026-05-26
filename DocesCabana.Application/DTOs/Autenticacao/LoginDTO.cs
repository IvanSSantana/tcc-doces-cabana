namespace DocesCabana.Application.DTOs.Autenticacao;

public class LoginDTO
{
    public string Login { get; set; } = string.Empty; // Pode ser e-mail ou telefone

    public string Senha { get; set; } = string.Empty;
    
    public bool LembrarMe { get; set; } = false;
}
