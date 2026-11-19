using DocesCabana.Application.Helpers;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Helpers;

public class PromocoesVigentesTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 20);
    private static readonly Guid CategoriaId = Guid.NewGuid();
    private static readonly Guid SubcategoriaId = Guid.NewGuid();

    private readonly Produto _produto = new(SubcategoriaId, "Brigadeiro", 10m,
        "https://exemplo.com/brigadeiro.png", 0.1m, 5m, 5m, 5m);

    private static readonly Dictionary<Guid, Guid> Mapa = new() { [SubcategoriaId] = CategoriaId };

    private Promocao Percentual(decimal valor) =>
        new($"{valor}%", TipoDeDesconto.Percentual, valor, null, null, Hoje, Hoje, null, null, [_produto]);

    private Promocao Leve3Pague2() =>
        new("Leve 3", TipoDeDesconto.CompreXLeveY, 0, 3, 2, Hoje, Hoje, null, null, [_produto]);

    private static Promocao ValorFixoNaCategoria(decimal valor) =>
        new("Fixo", TipoDeDesconto.ValorFixo, valor, null, null, Hoje, Hoje, CategoriaId, null, null);

    private PromocoesVigentes Com(params Promocao[] promocoes) => new(promocoes, Mapa);

    [Fact]
    public void Dado_Percentual10ELeve3Pague2Com1Unidade_Quando_MelhorPara_Entao_DeveSerPercentual()
    {
        var desconto = Com(Percentual(10), Leve3Pague2()).MelhorPara(_produto.ProdutoId, SubcategoriaId, 10m, 1);

        Assert.NotNull(desconto);
        Assert.Equal(1m, desconto.Valor);
        Assert.Equal("-10%", desconto.Rotulo);
    }

    [Fact]
    public void Dado_Percentual10ELeve3Pague2Com3Unidades_Quando_MelhorPara_Entao_DeveSerLeve3Pague2()
    {
        var desconto = Com(Percentual(10), Leve3Pague2()).MelhorPara(_produto.ProdutoId, SubcategoriaId, 10m, 3);

        Assert.NotNull(desconto);
        Assert.Equal(10m, desconto.Valor);
        Assert.Equal("Leve 3, pague 2", desconto.Rotulo);
    }

    [Fact]
    public void Dado_DuasPercentuais_Quando_MelhorPara_Entao_DeveSerAMaior()
    {
        var desconto = Com(Percentual(5), Percentual(20)).MelhorPara(_produto.ProdutoId, SubcategoriaId, 10m, 1);

        Assert.Equal(2m, desconto!.Valor);
    }

    [Fact]
    public void Dado_PromocaoPorCategoria_Quando_MelhorParaProdutoDeSubcategoriaDaCategoria_Entao_DeveAplicar()
    {
        var desconto = Com(ValorFixoNaCategoria(2)).MelhorPara(Guid.NewGuid(), SubcategoriaId, 10m, 2);

        Assert.NotNull(desconto);
        Assert.Equal(4m, desconto.Valor);
        Assert.Equal("-R$ 2,00", desconto.Rotulo);
    }

    [Fact]
    public void Dado_ProdutoForaDaPromocao_Quando_MelhorPara_Entao_DeveSerNulo()
    {
        var desconto = Com(Percentual(10)).MelhorPara(Guid.NewGuid(), Guid.NewGuid(), 10m, 1);

        Assert.Null(desconto);
    }

    [Fact]
    public void Dado_SoLeve3Pague2Com1Unidade_Quando_MelhorPara_Entao_DeveSerNulo()
    {
        var desconto = Com(Leve3Pague2()).MelhorPara(_produto.ProdutoId, SubcategoriaId, 10m, 1);

        Assert.Null(desconto);
    }

    [Fact]
    public void Dado_ValorFixoMaiorQueOPreco_Quando_MelhorPara_Entao_DeveSerNulo()
    {
        var desconto = Com(ValorFixoNaCategoria(50)).MelhorPara(Guid.NewGuid(), SubcategoriaId, 10m, 1);

        Assert.Null(desconto);
    }

    [Fact]
    public void Dado_PercentualELeve_Quando_ParaVitrine_Entao_DeveTerPrecoPromocionalEDoisSelos()
    {
        var (precoPromocional, selos) = Com(Percentual(10), Leve3Pague2())
            .ParaVitrine(_produto.ProdutoId, SubcategoriaId, 10m);

        Assert.Equal(9m, precoPromocional);
        Assert.Equal(new[] { "-10%", "Leve 3, pague 2" }, selos);
    }

    [Fact]
    public void Dado_SoLeve_Quando_ParaVitrine_Entao_DeveTerSeloSemPrecoPromocional()
    {
        var (precoPromocional, selos) = Com(Leve3Pague2()).ParaVitrine(_produto.ProdutoId, SubcategoriaId, 10m);

        Assert.Null(precoPromocional);
        Assert.Equal(new[] { "Leve 3, pague 2" }, selos);
    }

    [Fact]
    public void Dado_SemPromocao_Quando_ParaVitrine_Entao_DeveSerNulo()
    {
        var (precoPromocional, selos) = PromocoesVigentes.Nenhuma.ParaVitrine(_produto.ProdutoId, SubcategoriaId, 10m);

        Assert.Null(precoPromocional);
        Assert.Empty(selos);
    }

    [Fact]
    public void Dado_ValorFixoMaiorQueOPreco_Quando_ParaVitrine_Entao_NaoDeveTerSelo()
    {
        var (precoPromocional, selos) = Com(ValorFixoNaCategoria(50)).ParaVitrine(Guid.NewGuid(), SubcategoriaId, 10m);

        Assert.Null(precoPromocional);
        Assert.Empty(selos);
    }
}
