using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Entities;

public class ProdutoTests
{
    private readonly Guid _subcategoriaValida = Guid.NewGuid();
    private const string _nomeValido = "Bolo de Chocolate";
    private const decimal _precoValido = 10.50m;
    private const string _imagemValida = "https://imagem.com/produto.jpg";
    private const decimal _pesoValido = 0.5m;
    private const decimal _alturaValida = 10m;
    private const decimal _larguraValida = 15m;
    private const decimal _comprimentoValido = 20m;

    [Fact]
    public void Dado_DadosValidos_Quando_CriarProduto_Entao_DeveRetornarProdutoInstanciado()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido);

        Assert.NotNull(produto);
    }

    [Fact]
    public void Dado_StatusOmitido_Quando_CriarProduto_Entao_DeveNascerAtivo()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido);

        Assert.Equal(ProdutoStatus.Ativo, produto.Status);
    }

    [Theory]
    [InlineData(ProdutoStatus.Ativo)]
    [InlineData(ProdutoStatus.Inativo)]
    [InlineData(ProdutoStatus.ForaDeEstoque)]
    public void Dado_StatusExplicito_Quando_CriarProduto_Entao_DevePreservarStatus(ProdutoStatus status)
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, status: status);

        Assert.Equal(status, produto.Status);
    }

    [Fact]
    public void Dado_SubcategoriaInvalida_Quando_CriarProduto_Entao_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(Guid.Empty, _nomeValido, _precoValido, _imagemValida,
                _pesoValido, _alturaValida, _larguraValida, _comprimentoValido));
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("AB", typeof(ArgumentException))]
    public void Dado_NomeInvalido_Quando_CriarProduto_Entao_DeveLancarExcecaoCorreta(string? nome, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Produto(_subcategoriaValida, nome!, _precoValido, _imagemValida,
                _pesoValido, _alturaValida, _larguraValida, _comprimentoValido));
    }

    [Theory]
    [InlineData(0, typeof(ArgumentException))]
    [InlineData(-5, typeof(ArgumentException))]
    public void Dado_PrecoInvalido_Quando_CriarProduto_Entao_DeveLancarExcecaoCorreta(decimal preco, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Produto(_subcategoriaValida, _nomeValido, preco, _imagemValida,
                _pesoValido, _alturaValida, _larguraValida, _comprimentoValido));
    }

    [Theory]
    [InlineData("", typeof(ArgumentNullException))]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("imagem_invalida", typeof(ArgumentException))]
    [InlineData("ftp://imagem.com/produto.jpg", typeof(ArgumentException))]
    public void Dado_ImagemInvalida_Quando_CriarProduto_Entao_DeveLancarExcecaoCorreta(string? imagemUrl, Type tipoExcecao)
    {
        Assert.Throws(tipoExcecao, () =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, imagemUrl!,
                _pesoValido, _alturaValida, _larguraValida, _comprimentoValido));
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
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, descricao: null);

        Assert.Null(produto.Descricao);
    }

    [Fact]
    public void Dado_DescricaoComQuatroMilCaracteres_Quando_CriarProduto_Entao_DeveAceitar()
    {
        var descricao = new string('a', 4000);

        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, descricao: descricao);

        Assert.Equal(descricao, produto.Descricao);
    }

    [Fact]
    public void Dado_DescricaoComQuatroMilEUmCaracteres_Quando_CriarProduto_Entao_DeveLancarArgumentException()
    {
        var descricao = new string('a', 4001);

        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
                _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, descricao: descricao));
    }

    [Fact]
    public void Dado_DescricaoValida_Quando_AlterarDescricao_Entao_DeveAtualizarDescricao()
    {
        var produto = CriarProduto();

        produto.AlterarDescricao("Doce caseiro, feito com leite e açúcar.");

        Assert.Equal("Doce caseiro, feito com leite e açúcar.", produto.Descricao);
    }

    [Fact]
    public void Dado_SemAcucarOmitido_Quando_CriarProduto_Entao_DeveNascerFalse()
    {
        var produto = CriarProduto();

        Assert.False(produto.SemAcucar);
    }

    [Fact]
    public void Dado_SemAcucarExplicito_Quando_CriarProduto_Entao_DevePreservarValor()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, semAcucar: true);

        Assert.True(produto.SemAcucar);
    }

    [Fact]
    public void Dado_ProdutoComAcucar_Quando_MarcarComoSemAcucar_Entao_DeveAtualizarParaTrue()
    {
        var produto = CriarProduto();

        produto.MarcarComoSemAcucar();

        Assert.True(produto.SemAcucar);
    }

    [Fact]
    public void Dado_ProdutoSemAcucar_Quando_DesmarcarSemAcucar_Entao_DeveAtualizarParaFalse()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, semAcucar: true);

        produto.DesmarcarSemAcucar();

        Assert.False(produto.SemAcucar);
    }

    // spec 016: NomeNormalizado é derivado, nunca atribuído — os dois únicos
    // pontos que mudam o nome (construtor e AlterarNome) têm de produzir o
    // mesmo derivado que TextoHelper.Normalizar produziria.
    [Fact]
    public void Dado_NomeComAcentoECaixaAlta_Quando_CriarProduto_Entao_NomeNormalizadoDeveSairSemAcentoEEmCaixaBaixa()
    {
        var produto = new Produto(_subcategoriaValida, "Café Especial", _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido);

        Assert.Equal("cafe especial", produto.NomeNormalizado);
    }

    [Fact]
    public void Dado_ProdutoExistente_Quando_AlterarNome_Entao_NomeNormalizadoDeveAcompanhar()
    {
        var produto = CriarProduto();

        produto.AlterarNome("Cachaça Envelhecida");

        Assert.Equal("cachaca envelhecida", produto.NomeNormalizado);
    }

    [Fact]
    public void Dado_ProdutoAtivo_Quando_VerificarDisponivelParaCompra_Entao_DeveSerVerdadeiro()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, ProdutoStatus.Ativo);

        Assert.True(produto.DisponivelParaCompra());
    }

    [Fact]
    public void Dado_ProdutoInativo_Quando_VerificarDisponivelParaCompra_Entao_DeveSerFalso()
    {
        // RN-06 (spec 017): produto que saiu do catálogo é incomprável, com
        // o mesmo efeito de produto fora de estoque.
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, ProdutoStatus.Inativo);

        Assert.False(produto.DisponivelParaCompra());
    }

    [Fact]
    public void Dado_ProdutoForaDeEstoque_Quando_VerificarDisponivelParaCompra_Entao_DeveSerFalso()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido, ProdutoStatus.ForaDeEstoque);

        Assert.False(produto.DisponivelParaCompra());
    }

    // ── Peso e dimensões (spec 020) ──────────────────────────────────────
    // RN-01: produto sem medida não é despachável, e a loja não deve
    // conseguir criar um — a recusa vale para qualquer caminho de criação,
    // não só o formulário (Princípio III, a outra barreira).

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_PesoInvalido_Quando_CriarProduto_Entao_DeveLancarArgumentException(decimal peso)
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
                peso, _alturaValida, _larguraValida, _comprimentoValido));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_AlturaInvalida_Quando_CriarProduto_Entao_DeveLancarArgumentException(decimal altura)
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
                _pesoValido, altura, _larguraValida, _comprimentoValido));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_LarguraInvalida_Quando_CriarProduto_Entao_DeveLancarArgumentException(decimal largura)
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
                _pesoValido, _alturaValida, largura, _comprimentoValido));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_ComprimentoInvalido_Quando_CriarProduto_Entao_DeveLancarArgumentException(decimal comprimento)
    {
        Assert.Throws<ArgumentException>(() =>
            new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
                _pesoValido, _alturaValida, _larguraValida, comprimento));
    }

    [Fact]
    public void Dado_MedidasValidas_Quando_CriarProduto_Entao_DevePreservarAsQuatro()
    {
        var produto = new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido);

        Assert.Equal(_pesoValido, produto.Peso);
        Assert.Equal(_alturaValida, produto.Altura);
        Assert.Equal(_larguraValida, produto.Largura);
        Assert.Equal(_comprimentoValido, produto.Comprimento);
    }

    [Fact]
    public void Dado_MedidasNovasValidas_Quando_AlterarDimensoes_Entao_DeveAtualizarAsQuatro()
    {
        var produto = CriarProduto();

        produto.AlterarDimensoes(1.2m, 32, 8, 8);

        Assert.Equal(1.2m, produto.Peso);
        Assert.Equal(32, produto.Altura);
        Assert.Equal(8, produto.Largura);
        Assert.Equal(8, produto.Comprimento);
    }

    [Fact]
    public void Dado_PesoInvalido_Quando_AlterarDimensoes_Entao_DeveLancarArgumentException()
    {
        var produto = CriarProduto();

        Assert.Throws<ArgumentException>(() =>
            produto.AlterarDimensoes(0, _alturaValida, _larguraValida, _comprimentoValido));
    }

    private Produto CriarProduto()
    {
        return new Produto(_subcategoriaValida, _nomeValido, _precoValido, _imagemValida,
            _pesoValido, _alturaValida, _larguraValida, _comprimentoValido);
    }
}
