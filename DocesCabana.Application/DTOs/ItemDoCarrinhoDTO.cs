namespace DocesCabana.Application.DTOs;

// O par que atravessa a fronteira do carrinho de visitante (sessão) — sem
// preço (RN-04 da spec 017): o carrinho é intenção, não contrato.
public record ItemDoCarrinhoDTO(Guid ProdutoId, short Quantidade);
