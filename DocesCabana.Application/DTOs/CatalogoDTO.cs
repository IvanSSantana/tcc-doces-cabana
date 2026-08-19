using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public class CatalogoDTO
{
    public List<CategoriaDTO> Categorias { get; init; } = [];

    public CategoriaDTO? CategoriaAtual { get; init; }

    public IReadOnlyCollection<Guid> SubcategoriasMarcadas { get; init; } = [];

    public bool ApenasSemAcucar { get; init; }

    public OrdenacaoCatalogo Ordenacao { get; init; }

    public PaginaDeProdutosDTO Pagina { get; init; } = new();
}
