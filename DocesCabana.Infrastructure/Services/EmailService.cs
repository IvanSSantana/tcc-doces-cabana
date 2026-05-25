using System.Net;
using System.Net.Mail;
using DocesCabana.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocesCabana.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarEmailAsync(string email, string assunto, string corpo)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPortStr = _configuration["EmailSettings:SmtpPort"];
        var smtpUser = _configuration["EmailSettings:SmtpUsername"];
        var smtpPass = _configuration["EmailSettings:SmtpPassword"];
        var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "no-reply@docescabana.com.br";
        var senderName = _configuration["EmailSettings:SenderName"] ?? "Doces Cabana";
        var enableSslStr = _configuration["EmailSettings:EnableSsl"] ?? "true";

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            _logger.LogWarning($"Configurações de SMTP incompletas. O e-mail para '{email}' não foi enviado.");
            return;
        }

        int.TryParse(smtpPortStr, out int smtpPort);
        bool.TryParse(enableSslStr, out bool enableSsl);

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
