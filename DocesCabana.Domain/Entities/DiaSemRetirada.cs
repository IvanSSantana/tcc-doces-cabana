namespace DocesCabana.Domain.Entities;

public class DiaSemRetirada
{
    public const int TamanhoMaximoDoMotivo = 100;

    public DateOnly Data { get; private set; }

    public string? Motivo { get; private set; }

    protected DiaSemRetirada() { }

    internal DiaSemRetirada(DateOnly data, string? motivo)
    {
        var motivoLimpo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoLimpo is { Length: > TamanhoMaximoDoMotivo })
            throw new ArgumentException($"O motivo pode ter no máximo {TamanhoMaximoDoMotivo} caracteres.", nameof(motivo));

        Data = data;
        Motivo = motivoLimpo;
    }
}
