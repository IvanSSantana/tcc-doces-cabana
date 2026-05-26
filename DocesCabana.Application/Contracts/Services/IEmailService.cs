namespace DocesCabana.Application.Contracts.Services;

public interface IEmailService
{
    Task EnviarEmail(string email, string assunto, string corpo);
}
