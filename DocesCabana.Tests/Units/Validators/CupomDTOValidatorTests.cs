using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Validators;

public class CupomDTOValidatorTests
{
    private readonly CupomDTOValidator _validator = new();

    private static CupomDTO CriarValido() => new()
    {
        Codigo = "BEMVINDO10",
        Tipo = TipoDeDesconto.Percentual,
        Valor = 10,
        DataInicio = new DateOnly(2026, 11, 20),
        DataFim = new DateOnly(2026, 12, 20),
    };

    private void AssertInvalido(CupomDTO dto, string propriedade)
    {
        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == propriedade);
    }

    [Fact]
    public void Dado_CupomValido_Quando_Validar_Entao_DeveSerValido()
    {
        Assert.True(_validator.Validate(CriarValido()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    [InlineData("BEM VINDO")]
    [InlineData("BEM-VINDO")]
    public void Dado_CodigoInvalido_Quando_Validar_Entao_DeveSerInvalido(string codigo)
    {
        var dto = CriarValido();
        dto.Codigo = codigo;

        AssertInvalido(dto, nameof(CupomDTO.Codigo));
    }

    [Fact]
    public void Dado_CodigoMinusculoComEspacosNasPontas_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.Codigo = " frete50 ";

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_DescricaoCom256Caracteres_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValido();
        dto.Descricao = new string('a', 256);

        AssertInvalido(dto, nameof(CupomDTO.Descricao));
    }

    [Fact]
    public void Dado_TipoInexistente_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.Tipo = (TipoDeDesconto)9;

        AssertInvalido(dto, nameof(CupomDTO.Tipo));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(100)]
    public void Dado_PercentualForaDaFaixa_Quando_Validar_Entao_DeveSerInvalido(int? valor)
    {
        var dto = CriarValido();
        dto.Valor = valor;

        AssertInvalido(dto, nameof(CupomDTO.Valor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Dado_ValorFixoInvalido_Quando_Validar_Entao_DeveSerInvalido(int? valor)
    {
        var dto = CriarValido();
        dto.Tipo = TipoDeDesconto.ValorFixo;
        dto.Valor = valor;

        AssertInvalido(dto, nameof(CupomDTO.Valor));
    }

    [Fact]
    public void Dado_FreteGratisSemValor_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.Tipo = TipoDeDesconto.FreteGratis;
        dto.Valor = null;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_LeveXPagueYValido_Quando_Validar_Entao_DeveSerValidoSemValor()
    {
        var dto = CriarValido();
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
        var dto = CriarValido();
        dto.Tipo = TipoDeDesconto.CompreXLeveY;
        dto.QuantidadeLeve = leve;
        dto.QuantidadePague = 1;

        AssertInvalido(dto, nameof(CupomDTO.QuantidadeLeve));
    }

    [Theory]
    [InlineData(null)]
    [InlineData((short)0)]
    [InlineData((short)3)]
    public void Dado_PagueInvalido_Quando_Validar_Entao_DeveSerInvalido(short? pague)
    {
        var dto = CriarValido();
        dto.Tipo = TipoDeDesconto.CompreXLeveY;
        dto.QuantidadeLeve = 3;
        dto.QuantidadePague = pague;

        AssertInvalido(dto, nameof(CupomDTO.QuantidadePague));
    }

    [Fact]
    public void Dado_DataInicioVazia_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValido();
        dto.DataInicio = null;

        AssertInvalido(dto, nameof(CupomDTO.DataInicio));
    }

    [Fact]
    public void Dado_DataFimVazia_Quando_Validar_Entao_DeveSerInvalida()
    {
        var dto = CriarValido();
        dto.DataFim = null;

        AssertInvalido(dto, nameof(CupomDTO.DataFim));
    }

    [Fact]
    public void Dado_FimAntesDoInicio_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.DataFim = dto.DataInicio!.Value.AddDays(-1);

        AssertInvalido(dto, nameof(CupomDTO.DataFim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Dado_ValorMinimoZeroOuNegativo_Quando_Validar_Entao_DeveSerInvalido(int minimo)
    {
        var dto = CriarValido();
        dto.ValorMinimo = minimo;

        AssertInvalido(dto, nameof(CupomDTO.ValorMinimo));
    }

    [Fact]
    public void Dado_ValorMinimoPositivo_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.ValorMinimo = 50;

        Assert.True(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Dado_CategoriaESubcategoria_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido();
        dto.CategoriaId = Guid.NewGuid();
        dto.SubcategoriaId = Guid.NewGuid();

        AssertInvalido(dto, nameof(CupomDTO.SubcategoriaId));
    }

    [Fact]
    public void Dado_SoSubcategoria_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarValido();
        dto.SubcategoriaId = Guid.NewGuid();

        Assert.True(_validator.Validate(dto).IsValid);
    }
}
