namespace DocesCabana.Application.DTOs;

public class RetiradaDisponivelDTO
{
    public DateOnly Data { get; init; }

    public IReadOnlyList<FaixaDeRetiradaDTO> Faixas { get; init; } = [];

    public DateTime? ProximaAbertura { get; init; }

    public bool Disponivel => Faixas.Count > 0;
}
