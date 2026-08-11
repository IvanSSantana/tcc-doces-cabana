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

    private static ProdutoDTO CriarProdutoValido(
        string nome = "Brigadeiro Gourmet",
        decimal preco = 4.50m,
        string imagemUrl = "https://imagem.com/brigadeiro.jpg",
        Guid? subcategoriaId = null) =>
        new()
        {
            Nome = nome,
            Preco = preco,
            Status = ProdutoStatus.Ativo,
            ImagemUrl = imagemUrl,
            SubcategoriaId = subcategoriaId ?? Guid.NewGuid()
        };
}
