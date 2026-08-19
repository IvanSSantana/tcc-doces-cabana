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

    // RN-06: subcategorias vêm ordenadas pela quantidade de produtos
    // disponíveis, maior primeiro — é essa ordem que dá sentido a "as oito
    // principais" no menu do cabeçalho (RF-04) e na barra lateral (RF-10).
    // A ordenação por Nome como critério final é só para desempate
    // determinístico entre subcategorias com a mesma contagem.
    public async Task<List<CategoriaDTO>> ListarComSubcategorias()
    {
        var categorias = await _categoriaRepository.BuscarTodasComSubcategorias();
        var contagens = await _produtoRepository.ContarDisponivelPorSubcategoria();

        return categorias
            .Select(categoria => CategoriaMapper.ToDTO(categoria, contagens))
            .ToList();
    }

    // Casamento em memória (plano §8): não há coluna de apelido, então a
    // comparação percorre as poucas categorias que já foram carregadas.
    public async Task<CategoriaDTO?> BuscarPorApelido(string apelido)
    {
        var categorias = await ListarComSubcategorias();

        return categorias.FirstOrDefault(c => c.Apelido == apelido);
    }
}
