using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class ItemCarrinhoTests
{
    private static readonly Guid UsuarioValido = Guid.NewGuid();
    private static readonly Guid ProdutoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarItemCarrinho_Entao_DeveRetornarInstanciado()
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, 3);

        Assert.Equal(UsuarioValido, item.UsuarioId);
        Assert.Equal(ProdutoValido, item.ProdutoId);
        Assert.Equal(3, item.Quantidade);
    }

    [Fact]
    public void Dado_QuantidadeOmitida_Quando_CriarItemCarrinho_Entao_DeveNascerComUma()
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido);

        Assert.Equal(1, item.Quantidade);
    }

    [Fact]
    public void Dado_UsuarioInvalido_Quando_CriarItemCarrinho_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ItemCarrinho(Guid.Empty, ProdutoValido, 1));
    }

    [Fact]
    public void Dado_ProdutoInvalido_Quando_CriarItemCarrinho_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ItemCarrinho(UsuarioValido, Guid.Empty, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Dado_QuantidadeForaDoIntervalo_Quando_CriarItemCarrinho_Entao_DeveLancarArgumentException(short quantidade)
    {
        Assert.Throws<ArgumentException>(() => new ItemCarrinho(UsuarioValido, ProdutoValido, quantidade));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Dado_QuantidadeNosLimites_Quando_CriarItemCarrinho_Entao_DeveAceitar(short quantidade)
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, quantidade);

        Assert.Equal(quantidade, item.Quantidade);
    }

    [Fact]
    public void Dado_ItemExistente_Quando_AlterarQuantidade_Entao_DeveAtualizar()
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, 1);

        item.AlterarQuantidade(5);

        Assert.Equal(5, item.Quantidade);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Dado_QuantidadeForaDoIntervalo_Quando_AlterarQuantidade_Entao_DeveLancarArgumentException(short quantidade)
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, 1);

        Assert.Throws<ArgumentException>(() => item.AlterarQuantidade(quantidade));
    }

    [Fact]
    public void Dado_ItemComQuantidadeDois_Quando_Acrescentar3_Entao_DeveSomarParaCinco()
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, 2);

        item.Acrescentar(3);

        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public void Dado_ItemPertoDoTeto_Quando_AcrescentarAlemDoLimite_Entao_DeveLimitarA99()
    {
        // RN-02: a soma nunca ultrapassa o teto — corta, não recusa.
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, 95);

        item.Acrescentar(10);

        Assert.Equal(ItemCarrinho.QuantidadeMaxima, item.Quantidade);
    }

    [Fact]
    public void Dado_ItemNoTeto_Quando_Acrescentar_Entao_DevePermanecerNoTeto()
    {
        var item = new ItemCarrinho(UsuarioValido, ProdutoValido, ItemCarrinho.QuantidadeMaxima);

        item.Acrescentar(1);

        Assert.Equal(ItemCarrinho.QuantidadeMaxima, item.Quantidade);
    }
}
