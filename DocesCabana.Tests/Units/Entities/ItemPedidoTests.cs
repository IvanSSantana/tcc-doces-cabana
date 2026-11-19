using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class ItemPedidoTests
{
    private readonly Guid _pedidoValido = Guid.NewGuid();
    private readonly Guid _produtoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarItemPedido_Entao_DeveRetornarItemPedidoInstanciado()
    {
        var item = new ItemPedido(_pedidoValido, _produtoValido, 2, 5.50m);

        Assert.Equal(2, item.Quantidade);
        Assert.Equal(5.50m, item.PrecoUnitario);
    }

    [Fact]
    public void Dado_PedidoInvalido_Quando_CriarItemPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ItemPedido(Guid.Empty, _produtoValido, 2, 5.50m));
    }

    [Fact]
    public void Dado_ProdutoInvalido_Quando_CriarItemPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ItemPedido(_pedidoValido, Guid.Empty, 2, 5.50m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_QuantidadeInvalida_Quando_CriarItemPedido_Entao_DeveLancarArgumentException(short quantidade)
    {
        Assert.Throws<ArgumentException>(() => new ItemPedido(_pedidoValido, _produtoValido, quantidade, 5.50m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_PrecoUnitarioInvalido_Quando_CriarItemPedido_Entao_DeveLancarArgumentException(decimal preco)
    {
        Assert.Throws<ArgumentException>(() => new ItemPedido(_pedidoValido, _produtoValido, 2, preco));
    }

    [Fact]
    public void Dado_ItemSemDesconto_Quando_CriarItemPedido_Entao_DescontoDeveSerZeroERotuloNulo()
    {
        var item = new ItemPedido(_pedidoValido, _produtoValido, 2, 5.50m);

        Assert.Equal(0m, item.DescontoDaPromocao);
        Assert.Null(item.RotuloDaPromocao);
    }

    [Fact]
    public void Dado_DescontoNegativo_Quando_CriarItemPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ItemPedido(_pedidoValido, _produtoValido, 2, 5.50m, -0.01m, "-10%"));
    }

    [Theory]
    [InlineData(11.00)]
    [InlineData(12.00)]
    public void Dado_DescontoIgualOuMaiorQueOBruto_Quando_CriarItemPedido_Entao_DeveLancarArgumentException(decimal desconto)
    {
        Assert.Throws<ArgumentException>(() =>
            new ItemPedido(_pedidoValido, _produtoValido, 2, 5.50m, desconto, "-10%"));
    }

    [Fact]
    public void Dado_RotuloComMaisDe100Caracteres_Quando_CriarItemPedido_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ItemPedido(_pedidoValido, _produtoValido, 2, 5.50m, 1m, new string('a', 101)));
    }

    [Fact]
    public void Dado_ItemComDesconto_Quando_CalcularValorDaLinha_Entao_DeveSubtrairDesconto()
    {
        var item = new ItemPedido(_pedidoValido, _produtoValido, 3, 10m, 10m, "Leve 3, pague 2");

        Assert.Equal(20m, item.CalcularValorDaLinha());
        Assert.Equal("Leve 3, pague 2", item.RotuloDaPromocao);
    }

    [Fact]
    public void Dado_ItemSemDesconto_Quando_CalcularValorDaLinha_Entao_DeveSerPrecoVezesQuantidade()
    {
        var item = new ItemPedido(_pedidoValido, _produtoValido, 2, 5.50m);

        Assert.Equal(11m, item.CalcularValorDaLinha());
    }
}
