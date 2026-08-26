namespace DocesCabana.Application.DTOs;

// O que Fechar devolve — sucesso ou recusa, nunca exceção (Princípio VIII):
// divergência de valor, item indisponível e entrega incalculável são erro
// esperado do usuário (RN-02/RN-06 da spec 022).
public class ResultadoDoFechamentoDTO
{
    public bool Sucesso { get; init; }

    public Guid PedidoId { get; init; }

    public string? Mensagem { get; init; }

    // RF-16/CA-18: qual item indisponível impediu o fechamento.
    public string? ItemIndisponivel { get; init; }

    // RF-15/CA-16/CA-17: o valor atual, para a tela reexibir sinalizando o
    // que mudou — só preenchido quando é esse o motivo da recusa.
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
