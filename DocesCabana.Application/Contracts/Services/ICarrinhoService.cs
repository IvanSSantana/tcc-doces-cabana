using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ICarrinhoService
{
    Task<CarrinhoDTO> ObterDoUsuario(Guid usuarioId);
    Task Acrescentar(Guid usuarioId, Guid produtoId, short quantidade);
    Task AlterarQuantidade(Guid usuarioId, Guid produtoId, short quantidade);
    Task Remover(Guid usuarioId, Guid produtoId);
    Task<int> ContarItens(Guid usuarioId);

    Task Esvaziar(Guid usuarioId);

    Task<CarrinhoDTO> MontarAvulso(IReadOnlyList<ItemDoCarrinhoDTO> itens);
    Task<IReadOnlyList<ItemDoCarrinhoDTO>> AcrescentarAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade);
    IReadOnlyList<ItemDoCarrinhoDTO> AlterarQuantidadeAvulsa(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade);
    IReadOnlyList<ItemDoCarrinhoDTO> RemoverAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId);

    Task Fundir(Guid usuarioId, IReadOnlyList<ItemDoCarrinhoDTO> itensDaSessao);
}
