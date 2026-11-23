using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IPedidoService
{
    Task<PassoDoFechamentoDTO> MontarPasso(
        PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId,
        int? servicoDeEntregaId = null, string? codigoDoCupom = null,
        ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega, TimeOnly? inicioDaFaixa = null);

    Task<ResultadoDoFechamentoDTO> Fechar(Guid usuarioId, FechamentoDePedidoDTO dados);

    Task<ConfirmacaoDePedidoDTO?> ObterConfirmacao(Guid pedidoId, Guid usuarioId);

    Task<IReadOnlyList<ResumoDePedidoDTO>> ListarDoUsuario(Guid usuarioId);

    Task<DetalheDePedidoDTO> BuscarDetalhe(Guid pedidoId, Guid usuarioId);

    Task MarcarComoEntregue(Guid pedidoId, Guid usuarioId);
}
