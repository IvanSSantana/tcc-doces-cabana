namespace DocesCabana.Application.DTOs.Autenticacao;

public class LoginDTO
{
    public string? Login { get; set; }  // Pode ser e-mail ou CPF

    public string? Senha { get; set; } 
    public bool? LembrarMe { get; set; } = false;
}
