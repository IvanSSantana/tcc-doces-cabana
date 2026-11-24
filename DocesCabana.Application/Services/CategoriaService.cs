using DocesCabana.Application.Constants;
using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;

namespace DocesCabana.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private static readonly TimeSpan DuracaoDoCache = TimeSpan.FromMinutes(5);

    private readonly IProdutoRepository _produtoRepository;
    private readonly ICacheDaAplicacao _cache;

    public CategoriaService(ICategoriaRepository categoriaRepository, IProdutoRepository produtoRepository, ICacheDaAplicacao cache)
    {
        _categoriaRepository = categoriaRepository;
        _produtoRepository = produtoRepository;
        _cache = cache;
    }

    public Task<List<CategoriaDTO>> ListarComSubcategorias() =>
        _cache.ObterOuCriar(ChavesDeCache.CategoriasComSubcategorias, ConsultarComSubcategorias, DuracaoDoCache);

    private async Task<List<CategoriaDTO>> ConsultarComSubcategorias()
    {
        var categorias = await _categoriaRepository.BuscarTodasComSubcategorias();
        var contagens = await _produtoRepository.ContarDisponivelPorSubcategoria();

        return categorias
            .Select(categoria => CategoriaMapper.ToDTO(categoria, contagens))
            .ToList();
    }

    public async Task<CategoriaDTO?> BuscarPorApelido(string apelido)
    {
        var categorias = await ListarComSubcategorias();

        return categorias.FirstOrDefault(c => c.Apelido == apelido);
    }
}
