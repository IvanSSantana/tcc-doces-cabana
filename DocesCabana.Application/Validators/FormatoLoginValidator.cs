using System.ComponentModel.DataAnnotations;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Application.Validators;

/// <summary>
/// Regra de formato compartilhada por todo validator cujo campo "login" aceita
/// e-mail ou CPF (Login, EsqueceuSenha). Extraída para não duplicar a mesma
/// checagem em cada validator.
/// </summary>
public static class FormatoLoginValidator
{
    public static bool ValidarEmailOuCpf(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return false;

        var validadorEmail = new EmailAddressAttribute();

        bool ehEmail = validadorEmail.IsValid(login);
        bool ehCpf = CpfHelper.CpfValido(login);

        return ehCpf || ehEmail;
    }
}
