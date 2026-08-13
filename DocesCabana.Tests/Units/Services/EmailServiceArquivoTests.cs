using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DocesCabana.Tests.Units.Services;

public class EmailServiceArquivoTests : IDisposable
{
    private readonly string _pastaTemporaria;

    public EmailServiceArquivoTests()
    {
        _pastaTemporaria = Path.Combine(Path.GetTempPath(), $"doces-cabana-testes-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_pastaTemporaria))
            Directory.Delete(_pastaTemporaria, recursive: true);
    }

    private static EmailServiceArquivo CriarServico(string pastaDeSaida)
    {
        var settings = new EmailSettings { PastaDeSaida = pastaDeSaida };
        var options = Options.Create(settings);
        var loggerMock = new Mock<ILogger<EmailServiceArquivo>>();
        return new EmailServiceArquivo(options, loggerMock.Object);
    }

    [Fact]
    public async Task Dado_PastaConfigurada_Quando_EnviarEmail_Entao_DeveGravarArquivoComOCorpo()
    {
        Directory.CreateDirectory(_pastaTemporaria);
        var servico = CriarServico(_pastaTemporaria);

        await servico.EnviarEmail("cliente@teste.com", "Assunto Teste", "Corpo do e-mail de teste");

        var arquivos = Directory.GetFiles(_pastaTemporaria);
        Assert.Single(arquivos);

        var conteudo = await File.ReadAllTextAsync(arquivos[0]);
        Assert.Contains("cliente@teste.com", conteudo);
        Assert.Contains("Assunto Teste", conteudo);
        Assert.Contains("Corpo do e-mail de teste", conteudo);
    }

    [Fact]
    public async Task Dado_PastaInexistente_Quando_EnviarEmail_Entao_DeveCriarAPasta()
    {
        Assert.False(Directory.Exists(_pastaTemporaria));
        var servico = CriarServico(_pastaTemporaria);

        await servico.EnviarEmail("cliente@teste.com", "Assunto", "Corpo");

        Assert.True(Directory.Exists(_pastaTemporaria));
    }

    [Fact]
    public async Task Dado_DoisEnvios_Quando_EnviarEmail_Entao_DeveGerarDoisArquivos()
    {
        var servico = CriarServico(_pastaTemporaria);

        await servico.EnviarEmail("um@teste.com", "Assunto Um", "Corpo Um");
        await servico.EnviarEmail("dois@teste.com", "Assunto Dois", "Corpo Dois");

        var arquivos = Directory.GetFiles(_pastaTemporaria);
        Assert.Equal(2, arquivos.Length);
    }

    [Fact]
    public async Task Dado_PastaNaoConfigurada_Quando_EnviarEmail_Entao_DeveLancarInvalidOperationException()
    {
        // O adaptador não escolhe um diretório sozinho — inventar um é como
        // e-mail acaba servido por HTTP se cair sob o content root (risco 1,
        // plano §8 da 007).
        var servico = CriarServico(string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.EnviarEmail("cliente@teste.com", "Assunto", "Corpo"));
    }
}
