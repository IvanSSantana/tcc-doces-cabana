namespace DocesCabana.Domain.Helpers;

public static class DescontoHelper
{
    public static decimal Arredondar(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    public static decimal CalcularLeveXPagueY(decimal precoUnitario, short quantidade, short leve, short pague)
    {
        var gruposCompletos = quantidade / leve;
        var unidadesGratis = gruposCompletos * (leve - pague);

        return Arredondar(unidadesGratis * precoUnitario);
    }
}
