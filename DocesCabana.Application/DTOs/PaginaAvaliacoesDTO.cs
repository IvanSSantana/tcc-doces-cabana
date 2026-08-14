using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public class PaginaAvaliacoesDTO
{
    public IReadOnlyList<AvaliacaoDTO> Itens { get; init; } = [];

    public OrdenacaoAvaliacao Ordenacao { get; init; }

    public int Exibindo { get; init; }

    public int Total { get; init; }

    public bool TemMais { get; init; }
}
