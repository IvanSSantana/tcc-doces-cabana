namespace DocesCabana.Application.DTOs;

public class ResumoAvaliacoesDTO
{
    // Nulo quando o produto não tem avaliação — RN-03: não é zero.
    public decimal? Media { get; init; }

    public int Total { get; init; }

    // Sempre com as cinco chaves (1 a 5), mesmo quando alguma nota não tem
    // avaliação nenhuma — RN-04.
    public IReadOnlyDictionary<byte, int> Distribuicao { get; init; } = new Dictionary<byte, int>();
}
