using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IProdutoServices
{
    Task<List<ProdutoDTO>> BuscarTodosProdutosAsync();

    Task<ProdutoDTO> BuscarProdutoPorIdAsync(Guid id);
}
