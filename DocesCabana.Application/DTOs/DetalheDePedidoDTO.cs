using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

// O pedido inteiro (spec 023, RF-06 a RF-10) — o preço de cada item, o
// frete, a transportadora e o prazo são os gravados no fechamento (RN-02),
// nunca recalculados aqui: esta tela só lê.
public class DetalheDePedidoDTO
{
    public Guid PedidoId { get; init; }

    public string Numero { get; init; } = string.Empty;

    public DateTime Data { get; init; }

    public PedidoStatus Status { get; init; }

    public IReadOnlyList<ItemDoDetalheDePedidoDTO> Itens { get; init; } = [];

    public EnderecoDTO Endereco { get; init; } = new();

    public string Transportadora { get; init; } = string.Empty;

    public string Servico { get; init; } = string.Empty;

    public int PrazoMinimoEmDias { get; init; }

    public int PrazoMaximoEmDias { get; init; }

    public decimal ValorDosProdutos { get; init; }

    public decimal ValorDoFrete { get; init; }

    public decimal ValorTotal => ValorDosProdutos + ValorDoFrete;

    public MetodoPagamento MetodoPagamento { get; init; }

    public PagamentoStatus StatusDoPagamento { get; init; }
}

public record ItemDoDetalheDePedidoDTO(
    string Nome, string ImagemUrl, short Quantidade, decimal PrecoUnitario, decimal ValorDaLinha);
