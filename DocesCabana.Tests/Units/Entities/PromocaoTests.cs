using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class PromocaoTests
{
    private static readonly DateOnly DataInicioValida = new(2026, 11, 20);
    private static readonly DateOnly DataFimValida = new(2026, 11, 25);
    private static readonly Guid CategoriaValida = Guid.NewGuid();

    private static Produto CriarProduto(ProdutoStatus status = ProdutoStatus.Ativo, Guid subcategoriaId = default) =>
        new(subcategoriaId == Guid.Empty ? Guid.NewGuid() : subcategoriaId,
            "Brigadeiro", 10m, "https://exemplo.com/brigadeiro.png", 0.1m, 5m, 5m, 5m, status);

    private static Promocao CriarPercentual(decimal valor = 10, Guid? categoriaId = null,
        Guid? subcategoriaId = null, IEnumerable<Produto>? produtos = null) =>
        new("Semana do doce", TipoDeDesconto.Percentual, valor, null, null,
            DataInicioValida, DataFimValida,
            categoriaId ?? (subcategoriaId is null && produtos is null ? CategoriaValida : null),
            subcategoriaId, produtos);

    private static Promocao CriarValorFixo(decimal valor) =>
        new("Desconto fixo", TipoDeDesconto.ValorFixo, valor, null, null,
            DataInicioValida, DataFimValida, CategoriaValida, null, null);

    private static Promocao CriarLeveXPagueY(short? leve, short? pague) =>
        new("Leve mais", TipoDeDesconto.CompreXLeveY, 0, leve, pague,
            DataInicioValida, DataFimValida, CategoriaValida, null, null);

    [Fact]
    public void Dado_DadosValidos_Quando_CriarPromocao_Entao_DeveRetornarPromocaoInstanciada()
    {
        var promocao = CriarPercentual();

        Assert.Equal("Semana do doce", promocao.Nome);
        Assert.Equal(TipoDeDesconto.Percentual, promocao.Tipo);
        Assert.Equal(10, promocao.Valor);
        Assert.Equal(AbrangenciaDaPromocao.Categoria, promocao.Abrangencia);
        Assert.True(promocao.Ativa);
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    public void Dado_NomeInvalido_Quando_CriarPromocao_Entao_DeveLancarExcecaoCorreta(string? nome, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Promocao(nome!, TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, CategoriaValida, null, null));
    }

    [Fact]
    public void Dado_NomeComMaisDe255Caracteres_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao(new string('a', 256), TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, CategoriaValida, null, null));
    }

    [Fact]
    public void Dado_TipoFreteGratis_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        var excecao = Assert.Throws<ArgumentException>(() =>
            new Promocao("Frete", TipoDeDesconto.FreteGratis, 0, null, null,
                DataInicioValida, DataFimValida, CategoriaValida, null, null));

        Assert.StartsWith("Frete grátis só existe como cupom.", excecao.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(101)]
    public void Dado_PercentualForaDaFaixa_Quando_CriarPromocao_Entao_DeveLancarArgumentException(decimal valor)
    {
        Assert.Throws<ArgumentException>(() => CriarPercentual(valor));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Dado_PercentualNoLimiteDaFaixa_Quando_CriarPromocao_Entao_DeveConstruir(decimal valor)
    {
        Assert.Equal(valor, CriarPercentual(valor).Valor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Dado_ValorFixoInvalido_Quando_CriarPromocao_Entao_DeveLancarArgumentException(decimal valor)
    {
        Assert.Throws<ArgumentException>(() => CriarValorFixo(valor));
    }

    [Fact]
    public void Dado_LeveSemQuantidades_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarLeveXPagueY(null, null));
    }

    [Fact]
    public void Dado_LeveMenorQue2_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CriarLeveXPagueY(1, 1));
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(3, 4)]
    [InlineData(3, 0)]
    public void Dado_PagueMaiorOuIgualALeve_Quando_CriarPromocao_Entao_DeveLancarArgumentException(short leve, short pague)
    {
        Assert.Throws<ArgumentException>(() => CriarLeveXPagueY(leve, pague));
    }

    [Fact]
    public void Dado_Leve3Pague2_Quando_CriarPromocao_Entao_DeveGuardarQuantidadesEValorZero()
    {
        var promocao = CriarLeveXPagueY(3, 2);

        Assert.Equal((short)3, promocao.QuantidadeLeve);
        Assert.Equal((short)2, promocao.QuantidadePague);
        Assert.Equal(0, promocao.Valor);
    }

    [Fact]
    public void Dado_InicioIgualAoFim_Quando_CriarPromocao_Entao_DeveCriar()
    {
        var promocao = new Promocao("Um dia", TipoDeDesconto.Percentual, 10, null, null,
            DataInicioValida, DataInicioValida, CategoriaValida, null, null);

        Assert.Equal(promocao.DataInicio, promocao.DataFim);
    }

    [Fact]
    public void Dado_FimAntesDoInicio_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", TipoDeDesconto.Percentual, 10, null, null,
                DataFimValida, DataInicioValida, CategoriaValida, null, null));
    }

    [Fact]
    public void Dado_NenhumaAbrangencia_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, null, null, null));
    }

    [Fact]
    public void Dado_CategoriaESubcategoria_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, Guid.NewGuid(), Guid.NewGuid(), null));
    }

    [Fact]
    public void Dado_CategoriaEProdutos_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, Guid.NewGuid(), null, [CriarProduto()]));
    }

    [Fact]
    public void Dado_ListaDeProdutosVazia_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, null, null, []));
    }

    [Fact]
    public void Dado_CategoriaVazia_Quando_CriarPromocao_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promocao("Natal", TipoDeDesconto.Percentual, 10, null, null,
                DataInicioValida, DataFimValida, Guid.Empty, null, null));
    }

    [Fact]
    public void Dado_ProdutosAtivos_Quando_CriarPromocaoPorProdutos_Entao_DeveGuardarProdutos()
    {
        var produto = CriarProduto();

        var promocao = CriarPercentual(produtos: [produto]);

        Assert.Equal(AbrangenciaDaPromocao.ProdutosEscolhidos, promocao.Abrangencia);
        Assert.Single(promocao.Produtos);
        Assert.Null(promocao.CategoriaId);
    }

    [Fact]
    public void Dado_ProdutoInativo_Quando_CriarPromocaoPorProdutos_Entao_DeveLancarInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CriarPercentual(produtos: [CriarProduto(ProdutoStatus.Inativo)]));
    }

    [Fact]
    public void Dado_ProdutoForaDeEstoque_Quando_CriarPromocaoPorProdutos_Entao_DeveLancarInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CriarPercentual(produtos: [CriarProduto(ProdutoStatus.ForaDeEstoque)]));
    }

    [Fact]
    public void Dado_PromocaoPorCategoria_Quando_AlterarAbrangenciaParaProdutos_Entao_DeveTrocarAForma()
    {
        var promocao = CriarPercentual();

        promocao.AlterarAbrangencia(null, null, [CriarProduto(), CriarProduto()]);

        Assert.Equal(AbrangenciaDaPromocao.ProdutosEscolhidos, promocao.Abrangencia);
        Assert.Equal(2, promocao.Produtos.Count);
        Assert.Null(promocao.CategoriaId);
    }

    [Fact]
    public void Dado_PromocaoPorProdutos_Quando_AlterarAbrangenciaParaSubcategoria_Entao_DeveLimparProdutos()
    {
        var promocao = CriarPercentual(produtos: [CriarProduto()]);
        var subcategoriaId = Guid.NewGuid();

        promocao.AlterarAbrangencia(null, subcategoriaId, null);

        Assert.Equal(AbrangenciaDaPromocao.Subcategoria, promocao.Abrangencia);
        Assert.Empty(promocao.Produtos);
        Assert.Equal(subcategoriaId, promocao.SubcategoriaId);
    }

    [Fact]
    public void Dado_ProdutoInativo_Quando_AlterarAbrangencia_Entao_DeveLancarInvalidOperationException()
    {
        var promocao = CriarPercentual();

        Assert.Throws<InvalidOperationException>(() =>
            promocao.AlterarAbrangencia(null, null, [CriarProduto(ProdutoStatus.Inativo)]));
    }

    [Fact]
    public void Dado_PromocaoPercentual_Quando_AlterarDadosParaLeveXPagueY_Entao_DeveTrocarTipoEZerarValor()
    {
        var promocao = CriarPercentual();

        promocao.AlterarDados("Leve 3", "Nova descrição", TipoDeDesconto.CompreXLeveY, 15, 3, 2);

        Assert.Equal("Leve 3", promocao.Nome);
        Assert.Equal("Nova descrição", promocao.Descricao);
        Assert.Equal(TipoDeDesconto.CompreXLeveY, promocao.Tipo);
        Assert.Equal(0, promocao.Valor);
        Assert.Equal((short)3, promocao.QuantidadeLeve);
    }

    [Fact]
    public void Dado_PromocaoLeveXPagueY_Quando_AlterarDadosParaPercentual_Entao_DeveLimparQuantidades()
    {
        var promocao = CriarLeveXPagueY(3, 2);

        promocao.AlterarDados("Dez por cento", null, TipoDeDesconto.Percentual, 10, 3, 2);

        Assert.Null(promocao.QuantidadeLeve);
        Assert.Null(promocao.QuantidadePague);
        Assert.Equal(10, promocao.Valor);
    }

    [Fact]
    public void Dado_DadosInvalidos_Quando_AlterarDados_Entao_DeveLancarArgumentException()
    {
        var promocao = CriarPercentual();

        Assert.Throws<ArgumentException>(() =>
            promocao.AlterarDados("Frete", null, TipoDeDesconto.FreteGratis, 0, null, null));
    }

    [Fact]
    public void Dado_PeriodoValido_Quando_AlterarPeriodo_Entao_DeveTrocarDatas()
    {
        var promocao = CriarPercentual();
        var novoFim = new DateOnly(2026, 12, 31);

        promocao.AlterarPeriodo(DataInicioValida, novoFim);

        Assert.Equal(novoFim, promocao.DataFim);
    }

    public static TheoryData<AbrangenciaDaPromocao, bool> CasosDeAlcance => new()
    {
        { AbrangenciaDaPromocao.ProdutosEscolhidos, true },
        { AbrangenciaDaPromocao.ProdutosEscolhidos, false },
        { AbrangenciaDaPromocao.Categoria, true },
        { AbrangenciaDaPromocao.Categoria, false },
        { AbrangenciaDaPromocao.Subcategoria, true },
        { AbrangenciaDaPromocao.Subcategoria, false },
    };

    [Theory]
    [MemberData(nameof(CasosDeAlcance))]
    public void Dado_Abrangencia_Quando_Alcanca_Entao_DeveConsiderarSoAFormaEscolhida(
        AbrangenciaDaPromocao abrangencia, bool produtoDentro)
    {
        var categoriaId = Guid.NewGuid();
        var subcategoriaId = Guid.NewGuid();
        var produtoEscolhido = CriarProduto(subcategoriaId: subcategoriaId);

        var promocao = abrangencia switch
        {
            AbrangenciaDaPromocao.ProdutosEscolhidos => CriarPercentual(produtos: [produtoEscolhido]),
            AbrangenciaDaPromocao.Categoria => CriarPercentual(categoriaId: categoriaId),
            _ => CriarPercentual(subcategoriaId: subcategoriaId),
        };

        var alcanca = produtoDentro
            ? promocao.Alcanca(produtoEscolhido.ProdutoId, subcategoriaId, categoriaId)
            : promocao.Alcanca(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(produtoDentro, alcanca);
    }

    [Fact]
    public void Dado_Percentual10Com3Unidades_Quando_CalcularDesconto_Entao_DeveDescontar10PorCentoDaLinha()
    {
        Assert.Equal(3.75m, CriarPercentual(10).CalcularDesconto(12.50m, 3));
    }

    [Fact]
    public void Dado_ValorFixo2Com3Unidades_Quando_CalcularDesconto_Entao_DeveDescontar6()
    {
        Assert.Equal(6m, CriarValorFixo(2).CalcularDesconto(10m, 3));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public void Dado_ValorFixoMaiorOuIgualAoPreco_Quando_CalcularDesconto_Entao_DeveSerZero(decimal valor)
    {
        Assert.Equal(0m, CriarValorFixo(valor).CalcularDesconto(10m, 2));
    }

    [Fact]
    public void Dado_Leve3Pague2Com3Unidades_Quando_CalcularDesconto_Entao_DeveDescontarUmaUnidade()
    {
        Assert.Equal(10m, CriarLeveXPagueY(3, 2).CalcularDesconto(10m, 3));
    }

    [Theory]
    [InlineData(2026, 11, 19, true, false)]
    [InlineData(2026, 11, 20, true, true)]
    [InlineData(2026, 11, 25, true, true)]
    [InlineData(2026, 11, 26, true, false)]
    [InlineData(2026, 11, 22, false, false)]
    public void Dado_Data_Quando_EstaVigente_Entao_DeveIncluirPrimeiroEUltimoDiaERespeitarAtiva(
        int ano, int mes, int dia, bool ativa, bool esperado)
    {
        var promocao = CriarPercentual();
        if (!ativa)
            promocao.Desativar();

        Assert.Equal(esperado, promocao.EstaVigente(new DateOnly(ano, mes, dia)));
    }

    [Fact]
    public void Dado_PromocaoDesativada_Quando_Ativar_Entao_DeveFicarVigenteNoPeriodo()
    {
        var promocao = CriarPercentual();
        promocao.Desativar();

        promocao.Ativar();

        Assert.True(promocao.EstaVigente(DataInicioValida));
    }
}
