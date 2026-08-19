using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    // BuscarPorId (do IRepository genérico) não traz a subcategoria — a
    // página do produto precisa do nome dela no caminho de navegação (RF-02).
    Task<Produto?> BuscarDetalhePorId(Guid id);

    // Página do catálogo (spec 012): filtro + ordenação + Skip/Take, sempre
    // com Nome como desempate final (RN-05) — sem isso, Skip/Take não tem
    // resultado determinístico.
    Task<List<Produto>> BuscarPaginaDoCatalogo(FiltroCatalogoDTO filtro, int pagina, int tamanhoDaPagina);

    Task<int> ContarNoCatalogo(FiltroCatalogoDTO filtro);

    // RN-06: "as oito principais" de uma categoria são as subcategorias com
    // mais produtos disponíveis — a contagem que ordena o menu do cabeçalho
    // (RF-04) e a barra lateral (RF-10) vem daqui, não de escolha manual.
    Task<Dictionary<Guid, int>> ContarDisponivelPorSubcategoria();
}
