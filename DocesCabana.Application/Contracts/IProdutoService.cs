using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts;

public interface IProdutoService
{
    List<Produto> ObterProdutos(Guid subcategoriaId = default);
    Produto ObterProdutoPorId(Guid id);
}