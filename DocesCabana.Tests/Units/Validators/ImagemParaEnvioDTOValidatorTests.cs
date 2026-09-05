using DocesCabana.Application.DTOs;
using DocesCabana.Application.Validators;

namespace DocesCabana.Tests.Units.Validators;

// Barreira de entrada sobre metadados do arquivo (RF-03/RF-04, CA-03/CA-04) —
// o conteúdo nunca é lido aqui, só nome e tamanho declarados.
public class ImagemParaEnvioDTOValidatorTests
{
    private readonly ImagemParaEnvioDTOValidator _validator = new();

    [Theory]
    [InlineData("foto.txt", "text/plain")]
    [InlineData("foto.pdf", "application/pdf")]
    [InlineData("foto.gif", "image/gif")]
    public void Dado_ExtensaoForaDaLista_Quando_Validar_Entao_DeveSerInvalido(string nome, string contentType)
    {
        var dto = CriarValido(nome: nome, contentType: contentType);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "NomeDoArquivo");
    }

    [Fact]
    public void Dado_ContentTypeForaDaLista_Quando_Validar_Entao_DeveSerInvalido()
    {
        // Extensão aceita, mas o Content-Type declarado não bate com nenhum
        // formato aceito — um arquivo renomeado na mão, por exemplo.
        var dto = CriarValido(nome: "foto.jpg", contentType: "application/octet-stream");

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "ContentType");
    }

    [Fact]
    public void Dado_AcimaDoTamanhoMaximo_Quando_Validar_Entao_DeveSerInvalido()
    {
        var dto = CriarValido(tamanhoEmBytes: (5 * 1024 * 1024) + 1);

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == "TamanhoEmBytes");
    }

    [Theory]
    [InlineData("foto.jpg", "image/jpeg")]
    [InlineData("foto.jpeg", "image/jpeg")]
    [InlineData("foto.png", "image/png")]
    [InlineData("foto.webp", "image/webp")]
    public void Dado_ArquivoValido_Quando_Validar_Entao_DeveSerValido(string nome, string contentType)
    {
        var dto = CriarValido(nome: nome, contentType: contentType);

        var resultado = _validator.Validate(dto);

        Assert.True(resultado.IsValid);
    }

    private static ImagemParaEnvioDTO CriarValido(
        string nome = "foto.jpg", string contentType = "image/jpeg", long tamanhoEmBytes = 1024) =>
        new(nome, contentType, tamanhoEmBytes);
}
