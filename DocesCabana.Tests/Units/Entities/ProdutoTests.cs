using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class ProdutoTests
{
    private readonly Guid _subcategoriaValida = Guid.NewGuid();
    private const string _nomeValido = "Bolo de Chocolate";
    private const decimal _precoValido = 10.50m;
    private const string _imagemValida = "https://imagem.com/produto.jpg";

    [Fact]
    public void Dado_DadosValidos_Quando_CriarProduto_Entao_DeveRetornarProdutoInstanciado()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida);

        Assert.NotNull(produto);
    }

    [Fact]
    public void Dado_StatusOmitido_Quando_CriarProduto_Entao_DeveNascerAtivo()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida);

        Assert.Equal(ProdutoStatus.Ativo, produto.Status);
    }

    [Theory]
    [InlineData(ProdutoStatus.Ativo)]
    [InlineData(ProdutoStatus.Inativo)]
    [InlineData(ProdutoStatus.ForaDeEstoque)]
    public void Dado_StatusExplicito_Quando_CriarProduto_Entao_DevePreservarStatus(ProdutoStatus status)
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida, status: status);

        Assert.Equal(status, produto.Status);
    }

    [Fact]
    public void Dado_SubcategoriaInvalida_Quando_CriarProduto_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(Guid.Empty, _nomeValido, _precoValido, _imagemValida));
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("AB", typeof(ArgumentException))]
    public void Dado_NomeInvalido_Quando_CriarProduto_Entao_DeveLancarExcecaoCorreta(string? nome, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Produto(_subcategoriaValida, nome!, _precoValido, _imagemValida));
    }

    [Theory]
    [InlineData(0, typeof(ArgumentException))]
    [InlineData(-5, typeof(ArgumentException))]
    public void Dado_PrecoInvalido_Quando_CriarProduto_Entao_DeveLancarExcecaoCorreta(decimal preco, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Produto(_subcategoriaValida, _nomeValido, preco, _imagemValida));
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("imagem_invalida", typeof(ArgumentException))]
    [InlineData("ftp://imagem.com/produto.jpg", typeof(ArgumentException))]
    public void Dado_ImagemInvalida_Quando_CriarProduto_Entao_DeveLancarExcecaoCorreta(string? imagemUrl, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, imagemUrl!));
    }

    [Fact]
    public void Dado_NomeValido_Quando_AlterarNome_Entao_DeveAtualizarNome()
    {
        var produto = CriarProduto();

        produto.AlterarNome("Novo Nome");

        Assert.Equal("Novo Nome", produto.Nome);
    }

    [Fact]
    public void Dado_PrecoValido_Quando_AlterarPreco_Entao_DeveAtualizarPreco()
    {
        var produto = CriarProduto();

        produto.AlterarPreco(20);

        Assert.Equal(20, produto.Preco);
    }

    [Fact]
    public void Dado_SubcategoriaValida_Quando_AlterarSubcategoria_Entao_DeveAtualizarSubcategoria()
    {
        var produto = CriarProduto();
        var novaSubcategoria = Guid.NewGuid();

        produto.AlterarSubcategoriaId(novaSubcategoria);

        Assert.Equal(novaSubcategoria, produto.SubcategoriaId);
    }

    [Fact]
    public void Dado_ImagemValida_Quando_AlterarImagem_Entao_DeveAtualizarImagem()
    {
        var produto = CriarProduto();
        var novaUrl = "https://imagem.com/nova.jpg";

        produto.AlterarImagem(novaUrl);

        Assert.Equal(novaUrl, produto.ImagemUrl);
    }

    [Theory]
    [InlineData(ProdutoStatus.Ativo)]
    [InlineData(ProdutoStatus.Inativo)]
    [InlineData(ProdutoStatus.ForaDeEstoque)]
    public void Dado_StatusValido_Quando_AlterarStatus_Entao_DeveAtualizarStatus(ProdutoStatus novoStatus)
    {
        var produto = CriarProduto();

        produto.AlterarStatus(novoStatus);

        Assert.Equal(novoStatus, produto.Status);
    }

    [Fact]
    public void Dado_PromocaoValida_Quando_AplicarPromocao_Entao_DeveDefinirPromocaoId()
    {
        var produto = CriarProduto();
        var promocaoId = Guid.NewGuid();

        produto.AplicarPromocao(promocaoId);

        Assert.Equal(promocaoId, produto.PromocaoId);
    }

    [Fact]
    public void Dado_PromocaoInvalida_Quando_AplicarPromocao_Entao_DeveLancarArgumentException()
    {
        var produto = CriarProduto();

        Assert.Throws<ArgumentException>(() =>
            produto.AplicarPromocao(Guid.Empty));
    }

    [Fact]
    public void Dado_ProdutoInativo_Quando_AplicarPromocao_Entao_DeveLancarInvalidOperationException()
    {
        var produto = CriarProduto();
        produto.AlterarStatus(ProdutoStatus.Inativo);

        Assert.Throws<InvalidOperationException>(() =>
            produto.AplicarPromocao(Guid.NewGuid()));
    }

    [Fact]
    public void Dado_ProdutoSemEstoque_Temporariamente_Quando_AplicarPromocao_Entao_DeveLancarInvalidOperationException()
    {
        var produto = CriarProduto();
        produto.AlterarStatus(ProdutoStatus.ForaDeEstoque);

        Assert.Throws<InvalidOperationException>(() =>
            produto.AplicarPromocao(Guid.NewGuid()));
    }

    [Fact]
    public void Dado_PromocaoExistente_Quando_RemoverPromocao_Entao_DeveDeixarPromocaoIdNulo()
    {
        var produto = CriarProduto();
        produto.AplicarPromocao(Guid.NewGuid());

        produto.RemoverPromocao();

        Assert.Null(produto.PromocaoId);
    }

    [Fact]
    public void Dado_DescricaoNula_Quando_CriarProduto_Entao_DeveAceitar()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida, descricao: null);

        Assert.Null(produto.Descricao);
    }

    [Fact]
    public void Dado_DescricaoComQuatroMilCaracteres_Quando_CriarProduto_Entao_DeveAceitar()
    {
        var descricao = new string('a', 4000);

        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida, descricao: descricao);

        Assert.Equal(descricao, produto.Descricao);
    }

    [Fact]
    public void Dado_DescricaoComQuatroMilEUmCaracteres_Quando_CriarProduto_Entao_DeveLancarArgumentException()
    {
        var descricao = new string('a', 4001);

        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida, descricao: descricao));
    }

    [Fact]
    public void Dado_DescricaoValida_Quando_AlterarDescricao_Entao_DeveAtualizarDescricao()
    {
        var produto = CriarProduto();

        produto.AlterarDescricao("Doce caseiro, feito com leite e açúcar.");

        Assert.Equal("Doce caseiro, feito com leite e açúcar.", produto.Descricao);
    }

    private Produto CriarProduto()
    {
        return new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida);
    }
}
