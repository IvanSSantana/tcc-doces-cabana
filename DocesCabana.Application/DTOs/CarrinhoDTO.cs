namespace DocesCabana.Application.DTOs;

public class CarrinhoDTO
{
    public List<LinhaDoCarrinhoDTO> Linhas { get; init; } = [];

    // RF-08/RF-17: soma só as linhas disponíveis.
    public decimal Subtotal { get; init; }

    // RF-14: soma de quantidade, inclusive das linhas indisponíveis — é o
    // que fisicamente está no carrinho.
    public int TotalDeItens { get; init; }
}
