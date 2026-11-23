using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;
using Moq;

namespace DocesCabana.Tests.Units.Validators;

public class DiaSemRetiradaDTOValidatorTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 23);

    private readonly DiaSemRetiradaDTOValidator _validator;

    public DiaSemRetiradaDTOValidatorTests()
    {
        var relogio = new Mock<TimeProvider>();
        relogio.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 23, 15, 0, 0, TimeSpan.Zero));
        _validator = new DiaSemRetiradaDTOValidator(relogio.Object);
    }

    private void AssertInvalido(DiaSemRetiradaDTO dto, string propriedade)
    {
        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == propriedade);
    }

    [Fact]
    public void Dado_DataDeHojeSemMotivo_Quando_Validar_Entao_DeveSerValido()
    {
        Assert.True(_validator.Validate(new DiaSemRetiradaDTO { Data = Hoje }).IsValid);
    }

    [Fact]
    public void Dado_DataAusente_Quando_Validar_Entao_DeveSerInvalido()
    {
        AssertInvalido(new DiaSemRetiradaDTO(), nameof(DiaSemRetiradaDTO.Data));
    }

    [Fact]
    public void Dado_DataNoPassado_Quando_Validar_Entao_DeveSerInvalido()
    {
        AssertInvalido(new DiaSemRetiradaDTO { Data = Hoje.AddDays(-1) }, nameof(DiaSemRetiradaDTO.Data));
    }

    [Fact]
    public void Dado_MotivoCom101Caracteres_Quando_Validar_Entao_DeveSerInvalido()
    {
        AssertInvalido(new DiaSemRetiradaDTO { Data = Hoje, Motivo = new string('a', 101) }, nameof(DiaSemRetiradaDTO.Motivo));
    }

    [Fact]
    public void Dado_MotivoCom100Caracteres_Quando_Validar_Entao_DeveSerValido()
    {
        Assert.True(_validator.Validate(new DiaSemRetiradaDTO { Data = Hoje, Motivo = new string('a', 100) }).IsValid);
    }
}
