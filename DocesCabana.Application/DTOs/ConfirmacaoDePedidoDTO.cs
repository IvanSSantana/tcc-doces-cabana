using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class ConfirmacaoDePedidoDTO
{
    public Guid PedidoId { get; init; }

    public string Numero { get; init; } = string.Empty;

    public IReadOnlyList<ItemDaConfirmacaoDTO> Itens { get; init; } = [];

    public decimal ValorDosProdutos { get; init; }

    public decimal ValorDoFrete { get; init; }

    public decimal ValorTotal => ValorDosProdutos + ValorDoFrete;

    public string Transportadora { get; init; } = string.Empty;

    public string Servico { get; init; } = string.Empty;

    public int PrazoMinimoEmDias { get; init; }

    public int PrazoMaximoEmDias { get; init; }

    public MetodoPagamento MetodoPagamento { get; init; }
}

public record ItemDaConfirmacaoDTO(
    string Nome, string ImagemUrl, short Quantidade, decimal PrecoUnitario, decimal ValorDaLinha);
