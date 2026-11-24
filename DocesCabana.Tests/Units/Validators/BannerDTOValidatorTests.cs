using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Validators;

public class BannerDTOValidatorTests
{
    private readonly BannerDTOValidator _validator = new();

    private static BannerDTO CriarValido() => new()
    {
        TextoAlternativo = "Brigadeiros de Natal",
        Ordem = 1,
        DataInicio = new DateOnly(2026, 12, 1),
        DataFim = new DateOnly(2026, 12, 25),
        TipoDeDestino = TipoDeDestinoDoBanner.Categoria,
        CategoriaId = Guid.NewGuid(),
    };

    private void AssertInvalido(BannerDTO dto, string propriedade)
    {
        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == propriedade);
    }

    [Fact]
    public void Dado_BannerValido_Quando_Validar_Entao_DeveSerValido()
    {
        Assert.True(_validator.Validate(CriarValido()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Dado_TextoAlternativoVazio_Quando_Validar_Entao_DeveSerInvalido(string? texto)
    {
        var dto = CriarValido();
        dto.TextoAlternativo = texto!;

        AssertInvalido(dto, nameof(BannerDTO.TextoAlternativo));
    }

    [Fact]
    public void Dado_TextoAlternativoCom151Caracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.TextoAlternativo = new string('a', 151);

        AssertInvalido(dto, nameof(BannerDTO.TextoAlternativo));
    }

    [Fact]
    public void Dado_TextoAlternativoCom150Caracteres_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.TextoAlternativo = new string('a', 150);

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_OrdemAusente_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.Ordem = null;

        AssertInvalido(dto, nameof(BannerDTO.Ordem));
    }

    [Fact]
    public void Dado_OrdemNegativa_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.Ordem = -1;

        AssertInvalido(dto, nameof(BannerDTO.Ordem));
    }

    [Fact]
    public void Dado_DataInicioAusente_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.DataInicio = null;

        AssertInvalido(dto, nameof(BannerDTO.DataInicio));
    }

    [Fact]
    public void Dado_DataFimAusente_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.DataFim = null;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_DataFimAnteriorAoInicio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.DataFim = dto.DataInicio!.Value.AddDays(-1);

        AssertInvalido(dto, nameof(BannerDTO.DataFim));
    }

    [Fact]
    public void Dado_TipoForaDoEnum_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.TipoDeDestino = (TipoDeDestinoDoBanner)99;

        AssertInvalido(dto, nameof(BannerDTO.TipoDeDestino));
    }

    [Fact]
    public void Dado_DestinoCategoriaSemCategoria_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.CategoriaId = null;

        AssertInvalido(dto, nameof(BannerDTO.CategoriaId));
    }

    [Fact]
    public void Dado_DestinoSubcategoriaSemSubcategoria_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.TipoDeDestino = TipoDeDestinoDoBanner.Subcategoria;
        dto.SubcategoriaId = Guid.Empty;

        AssertInvalido(dto, nameof(BannerDTO.SubcategoriaId));
    }

    [Fact]
    public void Dado_DestinoSubcategoriaComSubcategoria_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.TipoDeDestino = TipoDeDestinoDoBanner.Subcategoria;
        dto.SubcategoriaId = Guid.NewGuid();

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_DestinoProdutoSemProduto_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.TipoDeDestino = TipoDeDestinoDoBanner.Produto;

        AssertInvalido(dto, nameof(BannerDTO.ProdutoId));
    }

    [Fact]
    public void Dado_DestinoProdutoComProduto_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.TipoDeDestino = TipoDeDestinoDoBanner.Produto;
        dto.ProdutoId = Guid.NewGuid();

        Assert.True(_validator.Validate(dto).IsValid);
    }
}
