using DocesCabana.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocesCabana.Infrastructure.Services;

/// <summary>
/// Adaptador de <see cref="IEmailService"/> que grava cada e-mail num arquivo
/// em vez de enviar por SMTP. Existe para o teste de ponta a ponta (spec 007)
/// concluir a redefinição de senha sem depender de serviço externo — nunca é
/// o padrão em produção (ver <see cref="EmailSettings.Adaptador"/>).
/// </summary>
public class EmailServiceArquivo : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailServiceArquivo> _logger;

    public EmailServiceArquivo(IOptions<EmailSettings> options, ILogger<EmailServiceArquivo> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task EnviarEmail(string email, string assunto, string corpo)
    {
        if (string.IsNullOrWhiteSpace(_settings.PastaDeSaida))
            throw new InvalidOperationException(
                "EmailSettings:PastaDeSaida não configurada. O adaptador de arquivo não escolhe um diretório sozinho — configure-a explicitamente antes de usar EmailSettings:Adaptador = \"Arquivo\".");

        Directory.CreateDirectory(_settings.PastaDeSaida);

        var nomeArquivo = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.eml";
        var caminho = Path.Combine(_settings.PastaDeSaida, nomeArquivo);
        var conteudo = $"Para: {email}\r\nAssunto: {assunto}\r\n\r\n{corpo}";

        await File.WriteAllTextAsync(caminho, conteudo);

        _logger.LogInformation("E-mail gravado em arquivo para {Email}: {Caminho}", email, caminho);
    }
}
