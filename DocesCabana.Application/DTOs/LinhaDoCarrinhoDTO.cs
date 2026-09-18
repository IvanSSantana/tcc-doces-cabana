using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public class LinhaDoCarrinhoDTO
{
    public Guid ProdutoId { get; init; }

    public string Nome { get; init; } = string.Empty;

    public string ImagemUrl { get; init; } = string.Empty;

    public decimal PrecoUnitario { get; init; }

    public short Quantidade { get; init; }

    public decimal ValorDaLinha { get; init; }

    public MotivoIndisponibilidade MotivoIndisponibilidade { get; init; }

    public decimal Peso { get; init; }

    public decimal Altura { get; init; }

    public decimal Largura { get; init; }

    public decimal Comprimento { get; init; }

    public bool Disponivel => MotivoIndisponibilidade == MotivoIndisponibilidade.Nenhum;
}
