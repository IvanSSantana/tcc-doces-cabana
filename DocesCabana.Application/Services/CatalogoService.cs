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

            // RF-07 (012): apelido que não corresponde a nenhuma categoria é
            // "não encontrado", não catálogo completo silenciosamente.
            if (categoriaAtual is null)
                throw new KeyNotFoundException($"Categoria \"{criterios.ApelidoDaCategoria}\" não encontrada.");
        }

        // RN-04 (016): apelido de subcategoria que não existir na categoria
        // atual é um filtro que não se aplica, não um erro — é ignorado, e
        // a categoria inteira é exibida. Sem categoria (catálogo completo),
        // nenhum apelido de subcategoria pode ser resolvido: RN-03 escopa a
        // unicidade por categoria, então fora de uma categoria o apelido não
        // tem contra o que ser comparado.
        var subcategoriasResolvidas = categoriaAtual?.Subcategorias
            .Where(s => criterios.ApelidosDeSubcategoria.Contains(s.Apelido))
            .ToList() ?? [];

        // RF-09: campo vazio (ou só espaço) não vira filtro — é o catálogo
        // completo, não um erro. TextoHelper.Normalizar aparado devolveria
        // "" para "   ", então o teste de vazio cobre os dois casos.
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

        // RF-21: página fora do intervalo cai no limite válido, nunca numa
        // grade vazia por página inexistente.
        var paginaSaneada = Math.Clamp(pagina, 1, totalDePaginas);

        var produtos = await _produtoRepository.BuscarPaginaDoCatalogo(filtro, paginaSaneada, TamanhoDaPagina);

        // RF-02: sem usuário (visitante), nenhum produto vem marcado — nem
        // consultamos favoritos, que ninguém tem quando não está logado.
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
