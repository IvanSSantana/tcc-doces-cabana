using System.Text.RegularExpressions;
using DocesCabana.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity;

public class ContaDeAcesso : IdentityUser<Guid>
{
    // Navegação Infra -> Domain: permitida. É a exceção que o Princípio I
    // documenta — esta classe depende do Identity, então mora aqui; o
    // Usuario do domínio não sabe que ela existe.
    public Usuario? Usuario { get; private set; }

    protected ContaDeAcesso() { }

    public ContaDeAcesso(string email)
    {
        ValidarEmail(email);

        UserName = email;
        Email = email;
    }

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled);

    private void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email é obrigatório!");

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Email inválido.", nameof(email));
    }
}
