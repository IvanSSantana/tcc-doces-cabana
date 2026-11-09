using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<Produto?> BuscarDetalhePorId(Guid id);

    Task<List<Produto>> BuscarPaginaDoCatalogo(FiltroCatalogoDTO filtro, int pagina, int tamanhoDaPagina);

    Task<int> ContarNoCatalogo(FiltroCatalogoDTO filtro);
    
    Task<List<Produto>> BuscarPaginaDaGestao(FiltroDeGestaoDeProduto filtro, int pagina, int tamanhoDaPagina);

    Task<int> ContarNaGestao(FiltroDeGestaoDeProduto filtro);

    Task<Dictionary<Guid, int>> ContarDisponivelPorSubcategoria();
}
