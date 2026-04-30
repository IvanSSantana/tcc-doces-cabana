using DocesCabana.Domain.Entities;

namespace DocesCabana.Units.Tests;

public class ProdutoTests
{
    private readonly Guid _subcategoriaValida = Guid.NewGuid();
    private const string _nomeValido = "Bolo de Chocolate";
    private const decimal _precoValido = 10.50m;
    private const string _imagemValida = "https://imagem.com/produto.jpg";

    [Fact]
    public void Inserir_Produto_Valido_Retorna_Produto()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida);

        Assert.NotNull(produto);
        Assert.Equal(1, produto.Status);
    }

    [Fact]
    public void Inserir_Subcategoria_Invalida_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(Guid.Empty, _nomeValido, _precoValido, _imagemValida));
    }

    [Fact]
    public void Inserir_Nome_Nulo_Lanca_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Produto(_subcategoriaValida, "", _precoValido, _imagemValida));
    }

    [Fact]
    public void Inserir_Nome_Muito_Curto_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, "AB", _precoValido, _imagemValida));
    }

    [Fact]
    public void Inserir_Preco_Invalido_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, 0, _imagemValida));
    }

    [Fact]
    public void Inserir_Imagem_Nula_Lanca_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, ""));
    }

    [Fact]
    public void Inserir_Imagem_Invalida_Lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, "imagem_invalida"));
    }

    [Fact]
    public void Alterar_Nome_Valido_Atualiza_Nome()
    {
        var produto = CriarProduto();

        produto.AlterarNome("Novo Nome");

        Assert.Equal("Novo Nome", produto.Nome);
    }

    [Fact]
    public void Alterar_Preco_Valido_Atualiza_Preco()
    {
        var produto = CriarProduto();

        produto.AlterarPreco(20);

        Assert.Equal(20, produto.Preco);
    }

    [Fact]
    public void Alterar_Subcategoria_Valida_Atualiza_Subcategoria()
    {
        var produto = CriarProduto();
        var novaSubcategoria = Guid.NewGuid();

        produto.AlterarSubcategoria(novaSubcategoria);

        Assert.Equal(novaSubcategoria, produto.SubcategoriaId);
    }

    [Fact]
    public void Alterar_Imagem_Valida_Atualiza_Imagem()
    {
        var produto = CriarProduto();
        var novaUrl = "https://imagem.com/nova.jpg";

        produto.AlterarImagem(novaUrl);

        Assert.Equal(novaUrl, produto.ImagemUrl);
    }

    [Fact]
    public void Inativar_Produto_Altera_Status_Para_Zero()
    {
        var produto = CriarProduto();

        produto.Inativar();

        Assert.Equal(0, produto.Status);
    }

    [Fact]
    public void Ativar_Produto_Altera_Status_Para_Um()
    {
        var produto = CriarProduto();
        produto.Inativar();

        produto.Ativar();

        Assert.Equal(1, produto.Status);
    }

    [Fact]
    public void Aplicar_Promocao_Valida_Define_PromocaoId()
    {
        var produto = CriarProduto();
        var promocaoId = Guid.NewGuid();

        produto.AplicarPromocao(promocaoId);

        Assert.Equal(promocaoId, produto.PromocaoId);
    }

    [Fact]
    public void Aplicar_Promocao_Invalida_Lanca_ArgumentException()
    {
        var produto = CriarProduto();

        Assert.Throws<ArgumentException>(() =>
            produto.AplicarPromocao(Guid.Empty));
    }

    [Fact]
    public void Aplicar_Promocao_Produto_Inativo_Lanca_InvalidOperationException()
    {
        var produto = CriarProduto();
        produto.Inativar();

        Assert.Throws<InvalidOperationException>(() =>
            produto.AplicarPromocao(Guid.NewGuid()));
    }

    [Fact]
    public void Remover_Promocao_Deixa_PromocaoId_Nulo()
    {
        var produto = CriarProduto();
        produto.AplicarPromocao(Guid.NewGuid());

        produto.RemoverPromocao();

        Assert.Null(produto.PromocaoId);
    }

    private Produto CriarProduto()
    {
        return new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida);
    }
}