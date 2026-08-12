using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class EstoqueTests
{
    private readonly Guid _produtoValido = Guid.NewGuid();

    [Fact]
    public void Dado_DadosValidos_Quando_CriarEstoque_Entao_DeveRetornarEstoqueInstanciado()
    {
        var estoque = new Estoque(_produtoValido, 10);

        Assert.NotNull(estoque);
        Assert.Equal(10, estoque.Quantidade);
    }

    [Fact]
    public void Dado_ProdutoInvalido_Quando_CriarEstoque_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Estoque(Guid.Empty, 10));
    }

    [Fact]
    public void Dado_QuantidadeInicialNegativa_Quando_CriarEstoque_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Estoque(_produtoValido, -1));
    }

    [Fact]
    public void Dado_EstoqueComTresUnidades_Quando_Adicionar_Entao_DeveSomarQuantidade()
    {
        var estoque = new Estoque(_produtoValido, 3);

        estoque.Adicionar(2);

        Assert.Equal(5, estoque.Quantidade);
    }

    [Fact]
    public void Dado_EstoqueComTresUnidades_Quando_RetirarDuas_Entao_DeveSubtrairQuantidade()
    {
        var estoque = new Estoque(_produtoValido, 3);

        estoque.Retirar(2);

        Assert.Equal(1, estoque.Quantidade);
    }

    [Fact]
    public void Dado_EstoqueComTresUnidades_Quando_RetirarCinco_Entao_DeveLancarInvalidOperationExceptionSemAlterarQuantidade()
    {
        var estoque = new Estoque(_produtoValido, 3);

        Assert.Throws<InvalidOperationException>(() => estoque.Retirar(5));
        Assert.Equal(3, estoque.Quantidade);
    }
}
