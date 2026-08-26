namespace DocesCabana.Application.DTOs;

public class CarrinhoDTO
{
    public List<LinhaDoCarrinhoDTO> Linhas { get; init; } = [];

    // RF-08/RF-17: soma só as linhas disponíveis.
    public decimal Subtotal { get; init; }

    // RF-14: soma de quantidade, inclusive das linhas indisponíveis — é o
    // que fisicamente está no carrinho.
    public int TotalDeItens { get; init; }

    // RF-05 (spec 021): nula até a pessoa informar um CEP no carrinho —
    // quem calcula é a entrega de cotação de frete (spec 020).
    public CotacaoDeFreteDTO? Cotacao { get; init; }

    // RF-06/RN-02 (spec 021): sem entrega calculada, o destaque não pode se
    // chamar "total a pagar" — seria afirmar um preço que ignora a entrega.
    public bool TemEntregaCalculada => Cotacao is { Opcoes.Count: > 0 };

    // RF-07/RN-06 (spec 021): havendo mais de uma opção, a mais barata é a
    // que compõe o total — estimativa até o fechamento (022) escolher de
    // fato.
    public decimal ValorTotal =>
        Subtotal + (TemEntregaCalculada ? Cotacao!.Opcoes.Min(o => o.Preco) : 0m);

    // RF-04 (spec 020): o carrinho já vem montado sem cotação — o
    // controlador cota depois, com o CEP da query, e anexa aqui. Devolve
    // uma cópia em vez de mutar (CarrinhoDTO é, por convenção, imutável
    // depois de montado).
    public CarrinhoDTO ComCotacao(CotacaoDeFreteDTO cotacao) => new()
    {
        Linhas = Linhas,
        Subtotal = Subtotal,
        TotalDeItens = TotalDeItens,
        Cotacao = cotacao
    };
}
