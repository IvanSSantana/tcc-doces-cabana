using System.ComponentModel.DataAnnotations;

namespace DocesCabana.Application.DTOs.Auth;

public class LoginDTO
{
    public string Login { get; set; } = string.Empty; // Pode ser e-mail ou telefone

    public string Senha { get; set; } = string.Empty;
}
