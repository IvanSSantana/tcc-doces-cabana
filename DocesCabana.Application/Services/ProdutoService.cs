using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;

namespace DocesCabana.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<List<ProdutoDTO>> BuscarTodosProdutos()
    {
        var produtos = await _produtoRepository.BuscarTodos();

        return ProdutoMapper.ToDto(produtos);
    }

    public async Task<ProdutoDTO> BuscarProdutoPorId(Guid id)
    {
        var produto = await _produtoRepository.BuscarPorId(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        return ProdutoMapper.ToDto(produto);
    }
}
