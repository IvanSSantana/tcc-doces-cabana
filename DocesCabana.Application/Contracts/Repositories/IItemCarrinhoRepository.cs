using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

// Não estende IRepository<T>: a chave de ItemCarrinho é composta
// (UsuarioId, ProdutoId), e IRepository<T>.BuscarPorId(Guid) assume chave
// simples — mesmo motivo de IFavoritoRepository (spec 015, plano §1).
public interface IItemCarrinhoRepository
{
    // Com o produto incluído — é o que a tela precisa para montar cada linha.
    Task<List<ItemCarrinho>> BuscarPorUsuario(Guid usuarioId);

    Task<ItemCarrinho?> Buscar(Guid usuarioId, Guid produtoId);

    // Soma as quantidades, não conta linhas — é o número que o cabeçalho
    // exibe (RF-14).
    Task<int> ContarItens(Guid usuarioId);

    Task Adicionar(ItemCarrinho item);

    void Remover(ItemCarrinho item);
}
