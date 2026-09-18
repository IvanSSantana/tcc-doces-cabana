using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IPedidoService
{
    Task<PassoDoFechamentoDTO> MontarPasso(
        PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId, int? servicoDeEntregaId = null);

    Task<ResultadoDoFechamentoDTO> Fechar(Guid usuarioId, FechamentoDePedidoDTO dados);

    Task<ConfirmacaoDePedidoDTO?> ObterConfirmacao(Guid pedidoId, Guid usuarioId);

    Task<IReadOnlyList<ResumoDePedidoDTO>> ListarDoUsuario(Guid usuarioId);

    Task<DetalheDePedidoDTO> BuscarDetalhe(Guid pedidoId, Guid usuarioId);
}
