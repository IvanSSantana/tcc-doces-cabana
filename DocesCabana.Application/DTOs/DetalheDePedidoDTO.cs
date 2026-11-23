using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

public class DetalheDePedidoDTO
{
    public Guid PedidoId { get; init; }

    public string Numero { get; init; } = string.Empty;

    public DateTime Data { get; init; }

    public PedidoStatus Status { get; init; }

    public IReadOnlyList<ItemDoDetalheDePedidoDTO> Itens { get; init; } = [];

    public EnderecoDTO Endereco { get; init; } = new();

    public ModalidadeDeEntrega Modalidade { get; init; }

    public DateOnly? DataDaRetirada { get; init; }

    public TimeOnly? InicioDaRetirada { get; init; }

    public TimeOnly? FimDaRetirada { get; init; }

    public bool RetiradaNaLoja => Modalidade == ModalidadeDeEntrega.RetiradaNaLoja;

    public string? Transportadora { get; init; }

    public string? Servico { get; init; }

    public int? PrazoMinimoEmDias { get; init; }

    public int? PrazoMaximoEmDias { get; init; }

    public decimal ValorDosProdutos { get; init; }

    public decimal ValorDoFrete { get; init; }

    public string? CodigoDoCupom { get; init; }

    public decimal DescontoDoCupom { get; init; }

    public decimal DescontoNoFrete { get; init; }

    public bool TemCupom => CodigoDoCupom is not null;

    public decimal ValorDosItens => ValorDosProdutos + DescontoDoCupom;

    public decimal ValorTotal => ValorDosProdutos + ValorDoFrete - DescontoNoFrete;

    public MetodoPagamento MetodoPagamento { get; init; }

    public PagamentoStatus StatusDoPagamento { get; init; }
}

public record ItemDoDetalheDePedidoDTO(
    string Nome, string ImagemUrl, short Quantidade, decimal PrecoUnitario, decimal ValorDaLinha,
    decimal DescontoDaPromocao = 0m, string? RotuloDaPromocao = null);
