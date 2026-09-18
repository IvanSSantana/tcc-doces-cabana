using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Helpers;

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

    public async Task<CatalogoDTO> Montar(CriteriosDoCatalogoDTO criterios, int pagina, Guid? usuarioId = null)
    {
        var categorias = await _categoriaService.ListarComSubcategorias();

        CategoriaDTO? categoriaAtual = null;
        if (criterios.ApelidoDaCategoria is not null)
        {
            categoriaAtual = categorias.FirstOrDefault(c => c.Apelido == criterios.ApelidoDaCategoria);

            if (categoriaAtual is null)
                throw new KeyNotFoundException($"Categoria \"{criterios.ApelidoDaCategoria}\" não encontrada.");
        }

        var subcategoriasResolvidas = categoriaAtual?.Subcategorias
            .Where(s => criterios.ApelidosDeSubcategoria.Contains(s.Apelido))
            .ToList() ?? [];

        var termoNormalizado = string.IsNullOrWhiteSpace(criterios.Termo)
            ? null
            : TextoHelper.Normalizar(criterios.Termo);

        var filtro = new FiltroCatalogoDTO(
            categoriaAtual?.CategoriaId,
            subcategoriasResolvidas.Select(s => s.SubcategoriaId).ToList(),
            criterios.ApenasSemAcucar,
            criterios.Ordenacao,
            termoNormalizado);

        var total = await _produtoRepository.ContarNoCatalogo(filtro);
        var totalDePaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanhoDaPagina));

        var paginaSaneada = Math.Clamp(pagina, 1, totalDePaginas);

        var produtos = await _produtoRepository.BuscarPaginaDoCatalogo(filtro, paginaSaneada, TamanhoDaPagina);

        var idsFavoritados = usuarioId.HasValue
            ? await _favoritoRepository.IdsPorUsuario(usuarioId.Value, produtos.Select(p => p.ProdutoId))
            : [];

        return new CatalogoDTO
        {
            Categorias = categorias,
            CategoriaAtual = categoriaAtual,
            SubcategoriasMarcadas = subcategoriasResolvidas.Select(s => s.Apelido).ToList(),
            ApenasSemAcucar = criterios.ApenasSemAcucar,
            Ordenacao = criterios.Ordenacao,
            Termo = criterios.Termo,
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
