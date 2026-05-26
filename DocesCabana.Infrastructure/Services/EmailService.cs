using System.Net;
using System.Net.Mail;
using DocesCabana.Application.Contracts.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DocesCabana.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task EnviarEmail(string email, string assunto, string corpo)
    {
        var smtpHost = _settings.SmtpHost;
        var smtpPort = _settings.SmtpPort;
        var smtpUser = _settings.SmtpUsername;
        var smtpPass = _settings.SmtpPassword;
        var senderEmail = _settings.SenderEmail;
        var senderName = _settings.SenderName;
        var enableSsl = _settings.EnableSsl;

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            _logger.LogWarning($"Configurações de SMTP incompletas. O e-mail para '{email}' não foi enviado.");
            return;
        }

        try
        {
            using var clienteSmtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = enableSsl
            };

            var mensagemEmail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = assunto,
                Body = corpo,
                IsBodyHtml = true
            };

            mensagemEmail.To.Add(email);

            await clienteSmtp.SendMailAsync(mensagemEmail);
            _logger.LogInformation($"E-mail enviado com sucesso para {email}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao enviar e-mail para {email}.");
            throw;
        }
    }
}
