namespace DocesCabana.Application.DTOs;

public class AdministradorDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public bool Principal { get; set; }
}
