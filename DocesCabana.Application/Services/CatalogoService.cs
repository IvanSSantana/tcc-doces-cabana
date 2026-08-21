using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;

namespace DocesCabana.Application.Services;

public class CatalogoService : ICatalogoService
{
    public const int TamanhoDaPagina = 12;

    private readonly ICategoriaService _categoriaService;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IFavoritoRepository _favoritoRepository;

    public CatalogoService(ICategoriaService categoriaService, IProdutoRepository produtoRepository, IFavoritoRepository favoritoRepository)
    {
        _categoriaService = categoriaService;
        _produtoRepository = produtoRepository;
        _favoritoRepository = favoritoRepository;
    }

    public async Task<CatalogoDTO> Montar(string? apelidoDaCategoria, FiltroCatalogoDTO filtro, int pagina, Guid? usuarioId = null)
    {
        var categorias = await _categoriaService.ListarComSubcategorias();

        CategoriaDTO? categoriaAtual = null;
        if (apelidoDaCategoria is not null)
        {
            categoriaAtual = categorias.FirstOrDefault(c => c.Apelido == apelidoDaCategoria);

            // RF-07: apelido que não corresponde a nenhuma categoria é
            // "não encontrado", não catálogo completo silenciosamente.
            if (categoriaAtual is null)
                throw new KeyNotFoundException($"Categoria \"{apelidoDaCategoria}\" não encontrada.");
        }

        var filtroComCategoria = filtro with { CategoriaId = categoriaAtual?.CategoriaId };

        var total = await _produtoRepository.ContarNoCatalogo(filtroComCategoria);
        var totalDePaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanhoDaPagina));

        // RF-21: página fora do intervalo cai no limite válido, nunca numa
        // grade vazia por página inexistente.
        var paginaSaneada = Math.Clamp(pagina, 1, totalDePaginas);

        var produtos = await _produtoRepository.BuscarPaginaDoCatalogo(filtroComCategoria, paginaSaneada, TamanhoDaPagina);

        // RF-02: sem usuário (visitante), nenhum produto vem marcado — nem
        // consultamos favoritos, que ninguém tem quando não está logado.
        var idsFavoritados = usuarioId.HasValue
            ? await _favoritoRepository.IdsPorUsuario(usuarioId.Value, produtos.Select(p => p.ProdutoId))
            : [];

        return new CatalogoDTO
        {
            Categorias = categorias,
            CategoriaAtual = categoriaAtual,
            SubcategoriasMarcadas = filtro.SubcategoriaIds,
            ApenasSemAcucar = filtro.ApenasSemAcucar,
            Ordenacao = filtro.Ordenacao,
            Pagina = new PaginaDeProdutosDTO
            {
                Itens = ProdutoMapper.ToDTO(produtos, idsFavoritados),
                PaginaAtual = paginaSaneada,
                TotalDePaginas = totalDePaginas,
                TotalDeItens = total
            }
        };
    }
}
