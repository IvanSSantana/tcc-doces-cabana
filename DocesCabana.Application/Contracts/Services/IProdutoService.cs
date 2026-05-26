using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IProdutoService
{
    Task<List<ProdutoDTO>> BuscarTodosProdutos();

    Task<ProdutoDTO> BuscarProdutoPorId(Guid id);
}
