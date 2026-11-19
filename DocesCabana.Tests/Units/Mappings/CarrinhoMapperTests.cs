using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using Xunit;

namespace DocesCabana.Tests.Units.Mappings;

public class CarrinhoMapperTests
{
    private static Produto CriarProduto(decimal preco = 10m) =>
        new(Guid.NewGuid(), "Brigadeiro", preco, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m);

    private static PromocoesVigentes PromocoesDe(Produto produto, params Promocao[] promocoes) =>
        new(promocoes, new Dictionary<Guid, Guid>());

    private static Promocao Percentual10(Produto produto) =>
        new("Dez", TipoDeDesconto.Percentual, 10, null, null,
            new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 20), null, null, [produto]);

    private static Promocao Leve3Pague2(Produto produto) =>
        new("Leve 3", TipoDeDesconto.CompreXLeveY, 0, 3, 2,
            new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 20), null, null, [produto]);

    [Fact]
    public void Dado_ProdutoEmPromocao_Quando_Montar_Entao_LinhaESubtotalDevemSerLiquidos()
    {
        var produto = CriarProduto(10m);
        var promocoes = PromocoesDe(produto, Percentual10(produto));

        var carrinho = CarrinhoMapper.Montar([(produto, (short)2)], promocoes: promocoes);

        var linha = Assert.Single(carrinho.Linhas);
        Assert.Equal(2m, linha.DescontoDaPromocao);
        Assert.Equal("-10%", linha.RotuloDaPromocao);
        Assert.Equal(18m, linha.ValorDaLinha);
        Assert.Equal(18m, carrinho.Subtotal);
    }

    [Theory]
    [InlineData(1, 1, "-10%")]
    [InlineData(3, 10, "Leve 3, pague 2")]
    public void Dado_DuasPromocoes_Quando_Montar_Entao_DeveAplicarAMaiorPelaQuantidade(
        short quantidade, int descontoEsperado, string rotuloEsperado)
    {
        var produto = CriarProduto(10m);
        var promocoes = PromocoesDe(produto, Percentual10(produto), Leve3Pague2(produto));

        var carrinho = CarrinhoMapper.Montar([(produto, quantidade)], promocoes: promocoes);

        var linha = Assert.Single(carrinho.Linhas);
        Assert.Equal(descontoEsperado, linha.DescontoDaPromocao);
        Assert.Equal(rotuloEsperado, linha.RotuloDaPromocao);
    }

    [Fact]
    public void Dado_ProdutoIndisponivelEmPromocao_Quando_Montar_Entao_NaoDeveAplicarDesconto()
    {
        var produto = CriarProduto(10m);
        var promocoes = PromocoesDe(produto, Percentual10(produto));
        produto.AlterarStatus(ProdutoStatus.ForaDeEstoque);

        var carrinho = CarrinhoMapper.Montar([(produto, (short)2)], promocoes: promocoes);

        var linha = Assert.Single(carrinho.Linhas);
        Assert.Equal(0m, linha.DescontoDaPromocao);
        Assert.Null(linha.RotuloDaPromocao);
        Assert.Equal(0m, carrinho.Subtotal);
    }

    [Fact]
    public void Dado_NenhumaCotacao_Quando_Montar_Entao_ValorTotalDeveSerOSubtotalETemEntregaCalculadaDeveSerFalso()
    {
        var produto = CriarProduto(10m);

        var carrinho = CarrinhoMapper.Montar([(produto, (short)2)]);

        Assert.Equal(20m, carrinho.Subtotal);
        Assert.Equal(20m, carrinho.ValorTotal);
        Assert.False(carrinho.TemEntregaCalculada);
    }

    [Fact]
    public void Dado_CotacaoComOpcoes_Quando_Montar_Entao_ValorTotalDeveIncluirAMaisBarataETemEntregaCalculadaDeveSerVerdadeiro()
    {
        var produto = CriarProduto(10m);
        var cotacao = new CotacaoDeFreteDTO(
            "01310000",
            [
                new OpcaoDeFreteDTO(2, "Correios", "SEDEX", 30m, 3, 4),
                new OpcaoDeFreteDTO(1, "Correios", "PAC", 18m, 8, 9),
            ],
            Mensagem: null);

        var carrinho = CarrinhoMapper.Montar([(produto, (short)1)], cotacao);

        Assert.True(carrinho.TemEntregaCalculada);
        Assert.Equal(28m, carrinho.ValorTotal);
    }

    [Fact]
    public void Dado_CotacaoSemOpcoes_Quando_Montar_Entao_TemEntregaCalculadaDeveSerFalso()
    {
        var produto = CriarProduto(10m);
        var cotacaoFalha = new CotacaoDeFreteDTO("01310000", [], "Não foi possível calcular o frete agora.");

        var carrinho = CarrinhoMapper.Montar([(produto, (short)1)], cotacaoFalha);

        Assert.False(carrinho.TemEntregaCalculada);
        Assert.Equal(10m, carrinho.ValorTotal);
    }
}
