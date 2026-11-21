using DocesCabana.Domain.Enums;

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

    public static void ValidarTipoEValor(TipoDeDesconto tipo, decimal valor, short? quantidadeLeve, short? quantidadePague)
    {
        switch (tipo)
        {
            case TipoDeDesconto.Percentual:
                if (valor < 1 || valor > 99)
                    throw new ArgumentException("Desconto percentual deve ter valor entre 1 e 99.", nameof(valor));
                break;

            case TipoDeDesconto.ValorFixo:
                if (valor <= 0)
                    throw new ArgumentException("Valor do desconto deve ser maior que zero.", nameof(valor));
                break;

            case TipoDeDesconto.CompreXLeveY:
                if (quantidadeLeve is null || quantidadePague is null)
                    throw new ArgumentException("Informe as quantidades de \"leve\" e \"pague\".", nameof(quantidadeLeve));

                if (quantidadeLeve < 2)
                    throw new ArgumentException("A quantidade de \"leve\" deve ser no mínimo 2.", nameof(quantidadeLeve));

                if (quantidadePague < 1 || quantidadePague >= quantidadeLeve)
                    throw new ArgumentException("A quantidade de \"pague\" deve ser no mínimo 1 e menor que a de \"leve\".", nameof(quantidadePague));
                break;

            case TipoDeDesconto.FreteGratis:
                break;

            default:
                throw new ArgumentException("Tipo de desconto inválido.", nameof(tipo));
        }
    }
}
