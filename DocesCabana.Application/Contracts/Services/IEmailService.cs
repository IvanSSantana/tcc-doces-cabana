namespace DocesCabana.Application.Contracts.Services;

public interface IEmailService
{
    Task EnviarEmailAsync(string email, string assunto, string corpo);
}
