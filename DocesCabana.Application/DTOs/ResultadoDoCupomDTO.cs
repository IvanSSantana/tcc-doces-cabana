namespace DocesCabana.Application.DTOs;

public class ResultadoDoCupomDTO
{
    public string Codigo { get; init; } = string.Empty;

    public Guid? CupomId { get; init; }

    public bool Valido { get; init; }

    public string? Motivo { get; init; }

    public string Rotulo { get; init; } = string.Empty;

    public bool FreteGratis { get; init; }

    public decimal DescontoNosProdutos { get; init; }

    public decimal DescontoNoFrete { get; init; }

    public decimal DescontoTotal => DescontoNosProdutos + DescontoNoFrete;

    public static ResultadoDoCupomDTO ParaRecusa(string codigo, string motivo, string rotulo = "") => new()
    {
        Codigo = codigo,
        Valido = false,
        Motivo = motivo,
        Rotulo = rotulo
    };
}
