using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class PromocaoTests
{
    private static readonly DateTime DataInicioValida = new(2026, 1, 1);
    private static readonly DateTime DataFimValida = new(2026, 12, 31);

    [Fact]
    public void Dado_DadosValidos_Quando_CriarPromocao_Entao_DeveRetornarPromocaoInstanciada()
    {
        var promocao = new Promocao("Natal", PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida);

        Assert.NotNull(promocao);
        Assert.Equal("Natal", promocao.Nome);
        Assert.Equal(PromocaoTipo.Percentual, promocao.Tipo);
        Assert.Equal(10, promocao.Valor);
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    public void Dado_NomeInvalido_Quando_CriarPromocao_Entao_DeveLancarExcecaoCorreta(string? nome, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Promocao(nome!, PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida));
    }

    [Fact]
    public void Dado_NomeComMaisDe255Caracteres_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        var nome = new string('a', 256);

        Assert.Throws<ArgumentException>(() =>
            new Promocao(nome, PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida));
    }

    [Fact]
    public void Dado_DataFimAnteriorADataInicio_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", PromocaoTipo.Percentual, 10, DataFimValida, DataInicioValida));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Dado_PercentualForaDaFaixa_Quando_CriarPromocao_Entao_DeveLancarArgumentException(decimal valor)
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", PromocaoTipo.Percentual, valor, DataInicioValida, DataFimValida));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Dado_PercentualNoLimiteDaFaixa_Quando_CriarPromocao_Entao_DeveConstruir(decimal valor)
    {
        var promocao = new Promocao("Natal", PromocaoTipo.Percentual, valor, DataInicioValida, DataFimValida);

        Assert.Equal(valor, promocao.Valor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Dado_ValorFixoInvalido_Quando_CriarPromocao_Entao_DeveLancarArgumentException(decimal valor)
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", PromocaoTipo.ValorFixo, valor, DataInicioValida, DataFimValida));
    }

    [Fact]
    public void Dado_PromocaoAtivaNoPeriodo_Quando_EstaVigente_Entao_DeveRetornarTrue()
    {
        var promocao = new Promocao("Natal", PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida);

        Assert.True(promocao.EstaVigente(new DateTime(2026, 6, 1)));
    }

    [Fact]
    public void Dado_ReferenciaForaDoPeriodo_Quando_EstaVigente_Entao_DeveRetornarFalse()
    {
        var promocao = new Promocao("Natal", PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida);

        Assert.False(promocao.EstaVigente(new DateTime(2027, 1, 1)));
    }

    [Fact]
    public void Dado_PromocaoDesativada_Quando_EstaVigente_Entao_DeveRetornarFalse()
    {
        var promocao = new Promocao("Natal", PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida);
        promocao.Desativar();

        Assert.False(promocao.EstaVigente(new DateTime(2026, 6, 1)));
    }

    [Fact]
    public void Dado_PromocaoDesativada_Quando_Ativar_Entao_DeveFicarVigenteNoPeriodo()
    {
        var promocao = new Promocao("Natal", PromocaoTipo.Percentual, 10, DataInicioValida, DataFimValida);
        promocao.Desativar();

        promocao.Ativar();

        Assert.True(promocao.EstaVigente(new DateTime(2026, 6, 1)));
    }
}
