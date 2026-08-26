using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IPedidoService
{
    // O carrinho já vem pronto de quem chama (spec 022, plano §1) — o
    // controlador é quem sabe resolver carrinho de sessão (visitante) ou
    // persistido (autenticado), e o serviço de aplicação não deveria
    // conhecer esse detalhe (HttpContext.Session é da MVC).
    Task<PassoDoFechamentoDTO> MontarPasso(
        PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId, int? servicoDeEntregaId = null);

    /// <summary>
    /// Fecha o pedido, ou explica por que não. Divergência de valor, item
    /// indisponível e entrega incalculável são erro esperado (RN-02/RN-06) e
    /// voltam no resultado — nunca como exceção (Princípio VIII).
    /// </summary>
    Task<ResultadoDoFechamentoDTO> Fechar(Guid usuarioId, FechamentoDePedidoDTO dados);

    // RN-08: null quando o pedido não existe ou não é de quem pede —
    // devolve o mesmo resultado para os dois casos, sem distinguir "não
    // existe" de "não é seu" (mesmo motivo de IEnderecoRepository nunca
    // buscar por id sozinho).
    Task<ConfirmacaoDePedidoDTO?> ObterConfirmacao(Guid pedidoId, Guid usuarioId);
}
