using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;

namespace DocesCabana.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IProdutoRepository produtoRepository, IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProdutoDTO>> BuscarTodosProdutos()
    {
        var produtos = await _produtoRepository.BuscarTodos();

        return ProdutoMapper.ToDTO(produtos);
    }

    public async Task<ProdutoDTO> BuscarProdutoPorId(Guid id)
    {
        var produto = await _produtoRepository.BuscarPorId(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        return ProdutoMapper.ToDTO(produto);
    }

    public async Task<ProdutoDTO> Cadastrar(ProdutoDTO dto)
    {
        var produto = ProdutoMapper.ToEntity(dto);
        await _produtoRepository.Adicionar(produto);
        await _unitOfWork.SalvarAlteracoes();

        return ProdutoMapper.ToDTO(produto);
    }
}
