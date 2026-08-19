namespace DocesCabana.Application.DTOs;

public class PaginaDeProdutosDTO
{
    public List<ProdutoDTO> Itens { get; init; } = [];

    public int PaginaAtual { get; init; }

    public int TotalDePaginas { get; init; }

    public int TotalDeItens { get; init; }
}
