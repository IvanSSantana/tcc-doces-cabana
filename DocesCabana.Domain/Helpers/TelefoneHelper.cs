using System.Text.RegularExpressions;

namespace DocesCabana.Domain.Helpers;

public static class TelefoneHelper
{
    private static readonly Regex CelularBrasileiro = new(
        @"^(?:[14689][1-9]|2[12478]|3[1-5]|3[7-8]|5[1345]|7[134579])9\d{8}$",
        RegexOptions.Compiled);

    public static string ApenasDigitos(string valor) =>
        new string(valor.Where(char.IsDigit).ToArray());

    public static bool CelularValido(string telefone) =>
        CelularBrasileiro.IsMatch(ApenasDigitos(telefone));
}
