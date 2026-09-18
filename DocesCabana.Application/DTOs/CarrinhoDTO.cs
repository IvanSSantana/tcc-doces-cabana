namespace DocesCabana.Application.DTOs;

public class CarrinhoDTO
{
    public List<LinhaDoCarrinhoDTO> Linhas { get; init; } = [];

    public decimal Subtotal { get; init; }

    public int TotalDeItens { get; init; }

    public CotacaoDeFreteDTO? Cotacao { get; init; }

     public bool TemEntregaCalculada => Cotacao is { Opcoes.Count: > 0 };

    public decimal ValorTotal =>
        Subtotal + (TemEntregaCalculada ? Cotacao!.Opcoes.Min(o => o.Preco) : 0m);

    public CarrinhoDTO ComCotacao(CotacaoDeFreteDTO cotacao) => new()
    {
        Linhas = Linhas,
        Subtotal = Subtotal,
        TotalDeItens = TotalDeItens,
        Cotacao = cotacao
    };
}
