using System.ComponentModel.DataAnnotations;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Application.Validators;

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
