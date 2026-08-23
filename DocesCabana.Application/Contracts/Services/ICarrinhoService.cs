using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

// Dez métodos — mais que qualquer outro serviço desta base. É o custo direto
// de existirem dois armazenamentos com formas diferentes: o carrinho
// persistido é consultável e transacional; o avulso (de quem ainda não
// entrou) é uma lista que viaja inteira, guardada onde o chamador quiser —
// aqui, na sessão (spec 017, plano §5). Nenhuma regra de negócio mora fora
// deste serviço: as operações avulsas aplicam as mesmas regras das
// persistidas, só que sobre uma lista em memória.
public interface ICarrinhoService
{
    // Carrinho persistido — de quem entrou.
    Task<CarrinhoDTO> ObterDoUsuario(Guid usuarioId);
    Task Acrescentar(Guid usuarioId, Guid produtoId, short quantidade);
    Task AlterarQuantidade(Guid usuarioId, Guid produtoId, short quantidade);
    Task Remover(Guid usuarioId, Guid produtoId);
    Task<int> ContarItens(Guid usuarioId);

    // Carrinho avulso — de quem ainda não entrou.
    Task<CarrinhoDTO> MontarAvulso(IReadOnlyList<ItemDoCarrinhoDTO> itens);
    Task<IReadOnlyList<ItemDoCarrinhoDTO>> AcrescentarAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade);
    IReadOnlyList<ItemDoCarrinhoDTO> AlterarQuantidadeAvulsa(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade);
    IReadOnlyList<ItemDoCarrinhoDTO> RemoverAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId);

    // A ponte entre os dois — RN-05.
    Task Fundir(Guid usuarioId, IReadOnlyList<ItemDoCarrinhoDTO> itensDaSessao);
}
