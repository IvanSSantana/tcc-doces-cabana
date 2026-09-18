using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;

namespace DocesCabana.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IProdutoRepository _produtoRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository, IProdutoRepository produtoRepository)
    {
        _categoriaRepository = categoriaRepository;
        _produtoRepository = produtoRepository;
    }

    public async Task<List<CategoriaDTO>> ListarComSubcategorias()
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
