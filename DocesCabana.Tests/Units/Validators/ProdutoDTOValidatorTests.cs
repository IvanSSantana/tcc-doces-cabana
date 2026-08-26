using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Validators;

public class ProdutoDTOValidatorTests
{
    private readonly ProdutoDTOValidator _validator = new();

    [Fact]
    public void Dado_UmProdutoValido_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarProdutoValido();

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_NomeVazio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(nome: "");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Dado_NomeComMenosDeTresCaracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(nome: "Bo");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Dado_PrecoZero_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(preco: 0);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Preco");
    }

    [Fact]
    public void Dado_PrecoNegativo_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(preco: -5);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Preco");
    }

    [Fact]
    public void Dado_ImagemVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(imagemUrl: "");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ImagemUrl");
    }

    [Fact]
    public void Dado_ImagemComUrlRelativa_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(imagemUrl: "foto.png");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ImagemUrl");
    }

    [Fact]
    public void Dado_ImagemComEsquemaNaoHttp_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(imagemUrl: "ftp://imagem.com/produto.jpg");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ImagemUrl");
    }

    [Fact]
    public void Dado_SubcategoriaVazia_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(subcategoriaId: Guid.Empty);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "SubcategoriaId");
    }

    [Fact]
    public void Dado_DescricaoVazia_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarProdutoValido(descricao: "");

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_DescricaoComQuatroMilEUmCaracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarProdutoValido(descricao: new string('a', 4001));

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Descricao");
    }

    // ── Peso e dimensões (spec 020, RF-02) ───────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_PesoInvalido_Quando_Validar_Entao_DeveSerInvalido(decimal peso)
    {
        var dto = CriarProdutoValido(peso: peso);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Peso");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_AlturaInvalida_Quando_Validar_Entao_DeveSerInvalido(decimal altura)
    {
        var dto = CriarProdutoValido(altura: altura);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Altura");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_LarguraInvalida_Quando_Validar_Entao_DeveSerInvalido(decimal largura)
    {
        var dto = CriarProdutoValido(largura: largura);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Largura");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Dado_ComprimentoInvalido_Quando_Validar_Entao_DeveSerInvalido(decimal comprimento)
    {
        var dto = CriarProdutoValido(comprimento: comprimento);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Comprimento");
    }

    [Fact]
    public void Dado_MedidasValidas_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarProdutoValido(peso: 0.5m, altura: 10m, largura: 15m, comprimento: 20m);

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    private static ProdutoDTO CriarProdutoValido(
        string nome = "Brigadeiro Gourmet",
        decimal preco = 4.50m,
        string imagemUrl = "https://imagem.com/brigadeiro.jpg",
        Guid? subcategoriaId = null,
        string? descricao = null,
        decimal peso = 0.5m,
        decimal altura = 10m,
        decimal largura = 15m,
        decimal comprimento = 20m) =>
        new()
        {
            Nome = nome,
            Preco = preco,
            Status = ProdutoStatus.Ativo,
            ImagemUrl = imagemUrl,
            SubcategoriaId = subcategoriaId ?? Guid.NewGuid(),
            Descricao = descricao,
            Peso = peso,
            Altura = altura,
            Largura = largura,
            Comprimento = comprimento
        };
}
