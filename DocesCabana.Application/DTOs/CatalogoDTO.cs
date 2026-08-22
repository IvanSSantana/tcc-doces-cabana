using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public class CatalogoDTO
{
    public List<CategoriaDTO> Categorias { get; init; } = [];

    public CategoriaDTO? CategoriaAtual { get; init; }

    // Apelidos, não identificadores (spec 016) — é o vocabulário do
    // endereço, e a barra lateral e a paginação remontam links a partir
    // dele. Só contém os que casaram com uma subcategoria real da categoria
    // atual: um apelido desconhecido nunca chega até aqui (RN-04).
    public IReadOnlyCollection<string> SubcategoriasMarcadas { get; init; } = [];

    public bool ApenasSemAcucar { get; init; }

    public OrdenacaoCatalogo Ordenacao { get; init; }

    // Nulo fora de busca. Cru, como foi digitado — é o que a tela reexibe
    // na barra de pesquisa e na etiqueta removível (spec 016, RF-06/RF-07).
    public string? Termo { get; init; }

    public PaginaDeProdutosDTO Pagina { get; init; } = new();
}
