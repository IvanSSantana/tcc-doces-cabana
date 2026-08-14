namespace DocesCabana.Application.DTOs;

public class AvaliacaoDTO
{
    public Guid AvaliacaoId { get; init; }

    public string AutorNome { get; init; } = string.Empty;

    public byte Nota { get; init; }

    public string? Comentario { get; init; }

    public DateTime DataCriacao { get; init; }

    public int TotalUteis { get; init; }

    public bool MarcadaPeloUsuarioAtual { get; init; }

    public bool EhDoUsuarioAtual { get; init; }
}
