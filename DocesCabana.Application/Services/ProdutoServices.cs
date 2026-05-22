using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;

namespace DocesCabana.Application.Services;

public class ProdutoServices : IProdutoServices
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoServices(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<List<ProdutoDTO>> BuscarTodosProdutosAsync()
    {
        var produtos = await _produtoRepository.BuscarTodosAsync();

        return ProdutoMapper.ToDto(produtos);
    }

    public async Task<ProdutoDTO> BuscarProdutoPorIdAsync(Guid id)
    {
        var produto = await _produtoRepository.BuscarPorIdAsync(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        return ProdutoMapper.ToDto(produto);
    }
}
