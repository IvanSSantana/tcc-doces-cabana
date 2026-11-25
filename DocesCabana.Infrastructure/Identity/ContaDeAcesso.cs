using System.Text.RegularExpressions;
using DocesCabana.Domain.Constants;
using DocesCabana.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Infrastructure.Identity;

public class ContaDeAcesso : IdentityUser<Guid>
{
    public Usuario? Usuario { get; private set; }

    /// <summary>
    /// Desativada pelo admin principal (spec 039, D4). O bloqueio de 15 minutos por senha errada
    /// não chega a esta data e não conta.
    /// </summary>
    public bool Desativada => LockoutEnd >= AdministradorPrincipal.FimDaDesativacao;

    public bool EhAdministradorPrincipal =>
        string.Equals(Email, AdministradorPrincipal.Email, StringComparison.OrdinalIgnoreCase);

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
