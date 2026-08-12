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
}
