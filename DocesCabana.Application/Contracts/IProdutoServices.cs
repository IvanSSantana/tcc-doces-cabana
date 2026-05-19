using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts;

public interface IProdutoServices
{
    List<Produto> ObterProdutos(Guid subcategoriaId = default);
    Produto ObterProdutoPorId(Guid id);
}