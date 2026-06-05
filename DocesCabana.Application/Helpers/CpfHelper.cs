namespace DocesCabana.Application.Helpers;

public static class CpfHelper
{
    public static string ApenasDigitos(string valor) =>
        new string(valor.Where(char.IsDigit).ToArray());

    public static bool FormatoValido(string cpf) =>
        ApenasDigitos(cpf).Length == 11;

    public static bool DigitoVerificadorValido(string cpf)
    {
        var digitos = ApenasDigitos(cpf);
        if (digitos.Length != 11)
            return false;

        if (new string(digitos[0], 11) == digitos)
            return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var primeirosDigitos = digitos[..9];
        var soma = primeirosDigitos.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();
        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;

        primeirosDigitos += digito;
        soma = primeirosDigitos.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();
        resto = soma % 11;
        digito = resto < 2 ? 0 : 11 - resto;

        return digitos.EndsWith(digito.ToString());
    }

    public static bool CpfValido(string cpf) =>
        FormatoValido(cpf) && DigitoVerificadorValido(cpf);
}
