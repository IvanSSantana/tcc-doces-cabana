using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public class PassoDoFechamentoDTO
{
    public PassoDoFechamento PassoAtivo { get; init; }

    public IReadOnlyList<PassoDoFechamento> PassosVisiveis { get; init; } = [];

    public CarrinhoDTO Carrinho { get; init; } = new();

    public IReadOnlyList<EnderecoDTO> Enderecos { get; init; } = [];

    public Guid? EnderecoSelecionadoId { get; init; }

    public CotacaoDeFreteDTO? Cotacao { get; init; }

    public int? ServicoDeEntregaSelecionadoId { get; init; }

    public ResultadoDoCupomDTO? Cupom { get; init; }

    public decimal? ValorDoFreteDeReferencia { get; init; }

    public bool CupomValido => Cupom is { Valido: true };

    public decimal DescontoTotalDoCupom => CupomValido ? Cupom!.DescontoTotal : 0m;

    public decimal ValorTotal => Carrinho.Subtotal + (ValorDoFreteDeReferencia ?? 0m) - DescontoTotalDoCupom;
}
