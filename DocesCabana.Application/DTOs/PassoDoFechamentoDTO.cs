using DocesCabana.Application.Enums;
using DocesCabana.Domain.Enums;

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

    public ModalidadeDeEntrega Modalidade { get; init; }

    /// <summary>Faixas de hoje; preenchido nos passos Endereço e Pagamento, para as duas modalidades.</summary>
    public RetiradaDisponivelDTO? Retirada { get; init; }

    public TimeOnly? InicioDaFaixaSelecionada { get; init; }

    public bool RetiradaNaLoja => Modalidade == ModalidadeDeEntrega.RetiradaNaLoja;

    public FaixaDeRetiradaDTO? FaixaSelecionada =>
        Retirada?.Faixas.FirstOrDefault(f => f.Inicio == InicioDaFaixaSelecionada);

    public bool CupomValido => Cupom is { Valido: true };

    public decimal DescontoTotalDoCupom => CupomValido ? Cupom!.DescontoTotal : 0m;

    public decimal ValorTotal => Carrinho.Subtotal + (ValorDoFreteDeReferencia ?? 0m) - DescontoTotalDoCupom;
}
