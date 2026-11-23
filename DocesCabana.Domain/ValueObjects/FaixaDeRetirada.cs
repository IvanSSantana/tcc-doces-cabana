namespace DocesCabana.Domain.ValueObjects;

public record FaixaDeRetirada(DateOnly Data, TimeOnly Inicio, TimeOnly Fim);
