using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

// O que o formulário do passo de pagamento posta ao confirmar (spec 022).
public class FechamentoDePedidoDTO
{
    public Guid EnderecoId { get; set; }

    public int ServicoDeEntregaId { get; set; }

    public MetodoPagamento MetodoPagamento { get; set; }

    // Alegações a conferir, nunca gravadas (RN-02). O que a tela exibiu
    // volta como isso — o servidor recalcula e compara; adulterar não dá
    // vantagem, e divergência legítima (preço ou frete que mudou) é
    // detectada, em vez de passar despercebida (plano §1).
    public decimal ValorDosProdutosExibido { get; set; }

    public decimal ValorDoFreteExibido { get; set; }
}
