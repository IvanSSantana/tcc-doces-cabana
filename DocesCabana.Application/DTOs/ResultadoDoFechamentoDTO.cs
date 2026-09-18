namespace DocesCabana.Application.DTOs;

public class ResultadoDoFechamentoDTO
{
    public bool Sucesso { get; init; }

    public Guid PedidoId { get; init; }

    public string? Mensagem { get; init; }

    public string? ItemIndisponivel { get; init; }

    public decimal? ValorDosProdutosAtual { get; init; }

    public decimal? ValorDoFreteAtual { get; init; }

    public static ResultadoDoFechamentoDTO ParaSucesso(Guid pedidoId) =>
        new() { Sucesso = true, PedidoId = pedidoId };

    public static ResultadoDoFechamentoDTO ParaRecusa(
        string mensagem, string? itemIndisponivel = null,
        decimal? valorDosProdutosAtual = null, decimal? valorDoFreteAtual = null) =>
        new()
        {
            Sucesso = false,
            Mensagem = mensagem,
            ItemIndisponivel = itemIndisponivel,
            ValorDosProdutosAtual = valorDosProdutosAtual,
            ValorDoFreteAtual = valorDoFreteAtual
        };
}
