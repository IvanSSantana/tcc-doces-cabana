using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

public class NovaAvaliacaoDTOValidatorTests
{
    private readonly NovaAvaliacaoDTOValidator _validator = new();

    private static NovaAvaliacaoDTO CriarAvaliacaoValida() => new()
    {
        ProdutoId = Guid.NewGuid(),
        Nota = 5,
        Comentario = "Muito bom!"
    };

    [Fact]
    public void Dado_UmaAvaliacaoValida_Quando_Validar_Entao_DeveSerValida()
    {
        var resultado = _validator.Validate(CriarAvaliacaoValida());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_NotaNula_Quando_Validar_Entao_DeveFalharEmNota()
    {
        var dto = CriarAvaliacaoValida();
        dto.Nota = null;

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(NovaAvaliacaoDTO.Nota)
            && e.ErrorMessage == "Escolha uma nota de 1 a 5.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Dado_NotaForaDaFaixa_Quando_Validar_Entao_DeveFalharEmNota(byte nota)
    {
        var dto = CriarAvaliacaoValida();
        dto.Nota = nota;

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(NovaAvaliacaoDTO.Nota));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Dado_NotaNosLimitesSemComentario_Quando_Validar_Entao_DeveSerValido(byte nota)
    {
        var dto = CriarAvaliacaoValida();
        dto.Nota = nota;
        dto.Comentario = null;

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_ComentarioCom256Caracteres_Quando_Validar_Entao_DeveFalharEmComentario()
    {
        var dto = CriarAvaliacaoValida();
        dto.Comentario = new string('a', 256);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(NovaAvaliacaoDTO.Comentario)
            && e.ErrorMessage == "O comentário deve ter no máximo 255 caracteres.");
    }

    [Fact]
    public void Dado_ComentarioCom255Caracteres_Quando_Validar_Entao_DeveSerValido()
    {
        var dto = CriarAvaliacaoValida();
        dto.Comentario = new string('a', 255);

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Dado_ProdutoIdVazio_Quando_Validar_Entao_DeveFalharEmProdutoId()
    {
        var dto = CriarAvaliacaoValida();
        dto.ProdutoId = Guid.Empty;

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(NovaAvaliacaoDTO.ProdutoId));
    }
}
