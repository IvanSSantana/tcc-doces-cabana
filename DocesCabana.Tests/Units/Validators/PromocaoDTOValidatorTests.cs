using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Validators;

public class PromocaoDTOValidatorTests
{
    private readonly PromocaoDTOValidator _validator = new();

    private static PromocaoDTO CriarValida() => new()
    {
        Nome = "Semana do doce",
        Tipo = TipoDeDesconto.Percentual,
        Valor = 10,
        DataInicio = new DateOnly(2026, 11, 20),
        DataFim = new DateOnly(2026, 11, 25),
        Abrangencia = AbrangenciaDaPromocao.Categoria,
        CategoriaId = Guid.NewGuid(),
    };

    private void AssertInvalido(PromocaoDTO dto, string propriedade)
    {
        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == propriedade);
    }

    [Fact]
    public void Dado_PromocaoValida_Quando_Validar_Entao_DeveSerValida()
    {
        Assert.True(_validator.Validate(CriarValida()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Dado_NomeVazio_Quando_Validar_Entao_DeveSerInvalido(string? nome)
    {
        var dto = CriarValida();
        dto.Nome = nome!;

        AssertInvalido(dto, nameof(PromocaoDTO.Nome));
    }

    [Fact]
    public void Dado_NomeCom256Caracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValida();
        dto.Nome = new string('a', 256);

        AssertInvalido(dto, nameof(PromocaoDTO.Nome));
    }

    [Fact]
    public void Dado_DescricaoCom256Caracteres_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Descricao = new string('a', 256);

        AssertInvalido(dto, nameof(PromocaoDTO.Descricao));
    }

    [Fact]
    public void Dado_DescricaoCom255Caracteres_Quando_Validar_Entao_DeveSerValida()
    {
        var dto = CriarValida();
        dto.Descricao = new string('a', 255);

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_TipoFreteGratis_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValida();
        dto.Tipo = TipoDeDesconto.FreteGratis;

        AssertInvalido(dto, nameof(PromocaoDTO.Tipo));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(100)]
    public void Dado_PercentualForaDaFaixa_Quando_Validar_Entao_DeveSerInvalido(int? valor)
    {
        var dto = CriarValida();
        dto.Valor = valor;

        AssertInvalido(dto, nameof(PromocaoDTO.Valor));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Dado_PercentualNoLimite_Quando_Validar_Entao_DeveSerValido(int valor)
    {
        var dto = CriarValida();
        dto.Valor = valor;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Dado_ValorFixoInvalido_Quando_Validar_Entao_DeveSerInvalido(int? valor)
    {
        var dto = CriarValida();
        dto.Tipo = TipoDeDesconto.ValorFixo;
        dto.Valor = valor;

        AssertInvalido(dto, nameof(PromocaoDTO.Valor));
    }

    [Fact]
    public void Dado_ValorFixoAcimaDe100_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValida();
        dto.Tipo = TipoDeDesconto.ValorFixo;
        dto.Valor = 150;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_LeveXPagueYValido_Quando_Validar_Entao_DeveSerValidoSemValor()
    {
        var dto = CriarValida();
        dto.Tipo = TipoDeDesconto.CompreXLeveY;
        dto.Valor = null;
        dto.QuantidadeLeve = 3;
        dto.QuantidadePague = 2;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData((short)1)]
    public void Dado_LeveInvalido_Quando_Validar_Entao_DeveSerInvalido(short? leve)
    {
        var dto = CriarValida();
        dto.Tipo = TipoDeDesconto.CompreXLeveY;
        dto.QuantidadeLeve = leve;
        dto.QuantidadePague = 1;

        AssertInvalido(dto, nameof(PromocaoDTO.QuantidadeLeve));
    }

    [Theory]
    [InlineData(null)]
    [InlineData((short)0)]
    [InlineData((short)3)]
    public void Dado_PagueInvalido_Quando_Validar_Entao_DeveSerInvalido(short? pague)
    {
        var dto = CriarValida();
        dto.Tipo = TipoDeDesconto.CompreXLeveY;
        dto.QuantidadeLeve = 3;
        dto.QuantidadePague = pague;

        AssertInvalido(dto, nameof(PromocaoDTO.QuantidadePague));
    }

    [Fact]
    public void Dado_DataInicioVazia_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.DataInicio = null;

        AssertInvalido(dto, nameof(PromocaoDTO.DataInicio));
    }

    [Fact]
    public void Dado_DataFimVazia_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.DataFim = null;

        AssertInvalido(dto, nameof(PromocaoDTO.DataFim));
    }

    [Fact]
    public void Dado_FimAntesDoInicio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValida();
        dto.DataFim = dto.DataInicio!.Value.AddDays(-1);

        AssertInvalido(dto, nameof(PromocaoDTO.DataFim));
    }

    [Fact]
    public void Dado_InicioIgualAoFim_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValida();
        dto.DataFim = dto.DataInicio;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_AbrangenciaCategoriaSemCategoria_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.CategoriaId = null;

        AssertInvalido(dto, nameof(PromocaoDTO.CategoriaId));
    }

    [Fact]
    public void Dado_AbrangenciaSubcategoriaSemSubcategoria_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Abrangencia = AbrangenciaDaPromocao.Subcategoria;

        AssertInvalido(dto, nameof(PromocaoDTO.SubcategoriaId));
    }

    [Fact]
    public void Dado_AbrangenciaSubcategoriaComSubcategoria_Quando_Validar_Entao_DeveSerValida()
    {
        var dto = CriarValida();
        dto.Abrangencia = AbrangenciaDaPromocao.Subcategoria;
        dto.SubcategoriaId = Guid.NewGuid();

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_AbrangenciaProdutosSemProdutos_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValida();
        dto.Abrangencia = AbrangenciaDaPromocao.ProdutosEscolhidos;

        AssertInvalido(dto, nameof(PromocaoDTO.ProdutoIds));
    }

    [Fact]
    public void Dado_AbrangenciaProdutosComProdutos_Quando_Validar_Entao_DeveSerValida()
    {
        var dto = CriarValida();
        dto.Abrangencia = AbrangenciaDaPromocao.ProdutosEscolhidos;
        dto.ProdutoIds = [Guid.NewGuid()];

        Assert.True(_validator.Validate(dto).IsValid);
    }
}
