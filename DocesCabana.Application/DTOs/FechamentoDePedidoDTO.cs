using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class FechamentoDePedidoDTO
{
    public Guid EnderecoId { get; set; }

    public int ServicoDeEntregaId { get; set; }

    public MetodoPagamento MetodoPagamento { get; set; }

    public decimal ValorDosProdutosExibido { get; set; }

    public decimal ValorDoFreteExibido { get; set; }
}
