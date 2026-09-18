namespace DocesCabana.Application.DTOs;

public class ResumoAvaliacoesDTO
{
    public decimal? Media { get; init; }

    public int Total { get; init; }

    public IReadOnlyDictionary<byte, int> Distribuicao { get; init; } = new Dictionary<byte, int>();
}
