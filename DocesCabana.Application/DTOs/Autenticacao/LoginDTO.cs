namespace DocesCabana.Application.DTOs.Autenticacao;

public class LoginDTO
{
    public string? Login { get; set; }

    public string? Senha { get; set; } 
    public bool? LembrarMe { get; set; } = false;
}
