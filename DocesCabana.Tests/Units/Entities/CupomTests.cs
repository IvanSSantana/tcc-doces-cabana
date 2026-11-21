using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class CupomTests
{
    private static readonly DateOnly DataInicioValida = new(2026, 11, 20);
    private static readonly DateOnly DataFimValida = new(2026, 11, 25);

    private static Cupom CriarCupom(
        string codigo = "BEMVINDO10",
        TipoDeDesconto tipo = TipoDeDesconto.Percentual,
        decimal valor = 10,
        short? leve = null,
        short? pague = null,
        decimal? valorMinimo = null,
        bool umUsoPorCliente = false,
        Guid? categoriaId = null,
        Guid? subcategoriaId = null,
        string? descricao = null) =>
        new(codigo, tipo, valor, leve, pague, DataInicioValida, DataFimValida,
            valorMinimo, umUsoPorCliente, categoriaId, subcategoriaId, descricao);

    [Fact]
    public void Dado_DadosValidos_Quando_CriarCupom_Entao_DeveRetornarCupomInstanciado()
    {
        var cupom = CriarCupom(valorMinimo: 50, umUsoPorCliente: true, descricao: "Primeira compra");

        Assert.NotEqual(Guid.Empty, cupom.CupomId);
        Assert.Equal("BEMVINDO10", cupom.Codigo);
        Assert.Equal(TipoDeDesconto.Percentual, cupom.Tipo);
        Assert.Equal(10, cupom.Valor);
        Assert.Equal(50, cupom.ValorMinimo);
        Assert.True(cupom.UmUsoPorCliente);
        Assert.Equal("Primeira compra", cupom.Descricao);
        Assert.True(cupom.Ativo);
    }

    [Fact]
    public void Dado_CodigoComEspacosEMinusculas_Quando_CriarCupom_Entao_DeveGuardarMaiusculoSemEspacos()
    {
        var cupom = CriarCupom(codigo: "  bemVindo10 ");

        Assert.Equal("BEMVINDO10", cupom.Codigo);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    [InlineData("BEM VINDO")]
    [InlineData("BEM-VINDO")]
    [InlineData("")]
    public void Dado_CodigoComFormatoInvalido_Quando_CriarCupom_Entao_DeveLancarArgumentException(string codigo)
    {
        Assert.ThrowsAny<ArgumentException>(() => CriarCupom(codigo: codigo));
    }

    [Fact]
    public void Dado_CodigoNulo_Quando_CriarCupom_Entao_DeveLancarArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => CriarCupom(codigo: null!));
    }

    [Fact]
    public void Dado_CodigoComMinusculas_Quando_NormalizarCodigo_Entao_DeveRetornarMaiusculoSemEspacos()
    {
        Assert.Equal("FRETE50", Cupom.NormalizarCodigo(" frete50 "));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Dado_PercentualForaDaFaixa_Quando_CriarCupom_Entao_DeveLancarArgumentException(decimal valor)
    {
        Assert.Throws<ArgumentException>(() => CriarCupom(valor: valor));
    }

    [Fact]
    public void Dado_ValorFixoZero_Quando_CriarCupom_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarCupom(tipo: TipoDeDesconto.ValorFixo, valor: 0));
    }

    [Theory]
    [InlineData(null, (short)1)]
    [InlineData((short)1, (short)1)]
    [InlineData((short)3, (short)3)]
    public void Dado_QuantidadesInvalidas_Quando_CriarCupomLeveXPagueY_Entao_DeveLancarArgumentException(short? leve, short? pague)
    {
        Assert.Throws<ArgumentException>(() =>
            CriarCupom(tipo: TipoDeDesconto.CompreXLeveY, valor: 0, leve: leve, pague: pague));
    }

    [Fact]
    public void Dado_LeveXPagueY_Quando_CriarCupom_Entao_DeveGuardarQuantidadesEZerarValor()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.CompreXLeveY, valor: 5, leve: 3, pague: 2);

        Assert.Equal(0, cupom.Valor);
        Assert.Equal((short)3, cupom.QuantidadeLeve);
        Assert.Equal((short)2, cupom.QuantidadePague);
    }

    [Fact]
    public void Dado_FreteGratis_Quando_CriarCupom_Entao_DeveZerarValorEQuantidades()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.FreteGratis, valor: 7, leve: 3, pague: 2);

        Assert.Equal(TipoDeDesconto.FreteGratis, cupom.Tipo);
        Assert.Equal(0, cupom.Valor);
        Assert.Null(cupom.QuantidadeLeve);
        Assert.Null(cupom.QuantidadePague);
    }

    [Fact]
    public void Dado_DataFimAnteriorAoInicio_Quando_CriarCupom_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Cupom("BEMVINDO10", TipoDeDesconto.Percentual, 10, null, null,
                DataFimValida, DataInicioValida, null, false, null, null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_ValorMinimoZeroOuNegativo_Quando_CriarCupom_Entao_DeveLancarArgumentException(decimal minimo)
    {
        Assert.Throws<ArgumentException>(() => CriarCupom(valorMinimo: minimo));
    }

    [Fact]
    public void Dado_CategoriaESubcategoria_Quando_CriarCupom_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            CriarCupom(categoriaId: Guid.NewGuid(), subcategoriaId: Guid.NewGuid()));
    }

    [Fact]
    public void Dado_CategoriaVazia_Quando_CriarCupom_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarCupom(categoriaId: Guid.Empty));
    }

    [Fact]
    public void Dado_DescricaoComMaisDe255Caracteres_Quando_CriarCupom_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarCupom(descricao: new string('a', 256)));
    }

    [Fact]
    public void Dado_CupomDesativado_Quando_Ativar_Entao_DeveFicarAtivo()
    {
        var cupom = CriarCupom();
        cupom.Desativar();
        Assert.False(cupom.Ativo);

        cupom.Ativar();

        Assert.True(cupom.Ativo);
    }

    [Fact]
    public void Dado_CupomCriado_Quando_AlterarDados_Entao_DeveTrocarTipoEValor()
    {
        var cupom = CriarCupom();

        cupom.AlterarDados(TipoDeDesconto.ValorFixo, 15, null, null, "Quinze reais");

        Assert.Equal(TipoDeDesconto.ValorFixo, cupom.Tipo);
        Assert.Equal(15, cupom.Valor);
        Assert.Equal("Quinze reais", cupom.Descricao);
    }

    [Fact]
    public void Dado_CupomCriado_Quando_AlterarRestricoes_Entao_DeveTrocarMinimoUsoECategoria()
    {
        var cupom = CriarCupom(categoriaId: Guid.NewGuid());
        var subcategoriaId = Guid.NewGuid();

        cupom.AlterarRestricoes(80, true, null, subcategoriaId);

        Assert.Equal(80, cupom.ValorMinimo);
        Assert.True(cupom.UmUsoPorCliente);
        Assert.Null(cupom.CategoriaId);
        Assert.Equal(subcategoriaId, cupom.SubcategoriaId);
    }

    [Theory]
    [InlineData(2026, 11, 19, false)]
    [InlineData(2026, 11, 20, true)]
    [InlineData(2026, 11, 25, true)]
    [InlineData(2026, 11, 26, false)]
    public void Dado_Data_Quando_EstaVigente_Entao_DeveRespeitarOPeriodo(int ano, int mes, int dia, bool esperado)
    {
        var cupom = CriarCupom();

        Assert.Equal(esperado, cupom.EstaVigente(new DateOnly(ano, mes, dia)));
    }

    [Fact]
    public void Dado_CupomDesativado_Quando_EstaVigente_Entao_DeveSerFalso()
    {
        var cupom = CriarCupom();
        cupom.Desativar();

        Assert.False(cupom.EstaVigente(DataInicioValida));
    }

    [Fact]
    public void Dado_CupomSemRestricao_Quando_Alcanca_Entao_DeveSerVerdadeiro()
    {
        Assert.True(CriarCupom().Alcanca(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Dado_CupomDeCategoria_Quando_Alcanca_Entao_SoDeveAlcancarACategoria()
    {
        var categoriaId = Guid.NewGuid();
        var cupom = CriarCupom(categoriaId: categoriaId);

        Assert.True(cupom.Alcanca(Guid.NewGuid(), categoriaId));
        Assert.False(cupom.Alcanca(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Dado_CupomDeSubcategoria_Quando_Alcanca_Entao_SoDeveAlcancarASubcategoria()
    {
        var subcategoriaId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var cupom = CriarCupom(subcategoriaId: subcategoriaId);

        Assert.True(cupom.Alcanca(subcategoriaId, categoriaId));
        Assert.False(cupom.Alcanca(Guid.NewGuid(), categoriaId));
    }

    [Fact]
    public void Dado_BaseElegivelAbaixoDoMinimo_Quando_AtingeMinimo_Entao_DeveSerFalso()
    {
        var cupom = CriarCupom(valorMinimo: 80);

        Assert.False(cupom.AtingeMinimo(79.99m));
        Assert.True(cupom.AtingeMinimo(80m));
    }

    [Fact]
    public void Dado_CupomSemMinimo_Quando_AtingeMinimo_Entao_DeveSerVerdadeiro()
    {
        Assert.True(CriarCupom().AtingeMinimo(0.01m));
    }

    [Fact]
    public void Dado_Percentual10ComBase80_Quando_CalcularDescontoNosProdutos_Entao_DeveSer8()
    {
        var cupom = CriarCupom(valor: 10);

        var desconto = cupom.CalcularDescontoNosProdutos([(50m, 2), (30m, 1)]);

        Assert.Equal(8m, desconto);
    }

    [Fact]
    public void Dado_Percentual_Quando_CalcularDescontoNosProdutos_Entao_DeveArredondar()
    {
        var cupom = CriarCupom(valor: 15);

        var desconto = cupom.CalcularDescontoNosProdutos([(10.05m, 1)]);

        Assert.Equal(1.51m, desconto);
    }

    [Fact]
    public void Dado_ValorFixo15ComBase80_Quando_CalcularDescontoNosProdutos_Entao_DeveSer15()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.ValorFixo, valor: 15);

        Assert.Equal(15m, cupom.CalcularDescontoNosProdutos([(50m, 2), (30m, 1)]));
    }

    [Fact]
    public void Dado_ValorFixo15ComBase10_Quando_CalcularDescontoNosProdutos_Entao_DeveSer10()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.ValorFixo, valor: 15);

        Assert.Equal(10m, cupom.CalcularDescontoNosProdutos([(10m, 1)]));
    }

    [Fact]
    public void Dado_Leve3Pague2ComLinhaLiquida27Em3Unidades_Quando_CalcularDescontoNosProdutos_Entao_DeveSer9()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.CompreXLeveY, valor: 0, leve: 3, pague: 2);

        Assert.Equal(9m, cupom.CalcularDescontoNosProdutos([(27m, 3)]));
    }

    [Fact]
    public void Dado_Leve3Pague2ComDuasLinhasDeDuasUnidades_Quando_CalcularDescontoNosProdutos_Entao_DeveSerZero()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.CompreXLeveY, valor: 0, leve: 3, pague: 2);

        Assert.Equal(0m, cupom.CalcularDescontoNosProdutos([(20m, 2), (20m, 2)]));
    }

    [Fact]
    public void Dado_FreteGratis_Quando_CalcularDescontoNosProdutos_Entao_DeveSerZero()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.FreteGratis, valor: 0);

        Assert.Equal(0m, cupom.CalcularDescontoNosProdutos([(50m, 2)]));
    }

    [Fact]
    public void Dado_FreteGratis_Quando_CalcularDescontoNoFrete_Entao_DeveSerOValorDoFrete()
    {
        var cupom = CriarCupom(tipo: TipoDeDesconto.FreteGratis, valor: 0);

        Assert.Equal(23.45m, cupom.CalcularDescontoNoFrete(23.45m));
    }

    [Fact]
    public void Dado_Percentual_Quando_CalcularDescontoNoFrete_Entao_DeveSerZero()
    {
        Assert.Equal(0m, CriarCupom().CalcularDescontoNoFrete(23.45m));
    }
}
