namespace DocesCabana.Application.DTOs;

public class CategoriaDTO
{
    public Guid CategoriaId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string Apelido { get; init; } = string.Empty;

    public List<SubcategoriaDTO> Subcategorias { get; init; } = [];
}
