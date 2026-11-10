namespace DocesCabana.Application.DTOs;

public class NovaAvaliacaoDTO
{
    public Guid ProdutoId { get; set; }

    public byte? Nota { get; set; }

    public string? Comentario { get; set; }
}
