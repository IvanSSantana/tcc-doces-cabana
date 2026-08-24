namespace DocesCabana.Domain.Helpers;

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

        // RN-01 (spec 019): os dois dígitos verificadores são conferidos
        // contra o que a pessoa digitou — não só o segundo. O primeiro
        // (posição 9) é o penúltimo dígito do CPF, e ficava sem conferência
        // nenhuma: era calculado, concatenado ao parcial e usado para achar
        // o segundo, mas nunca comparado com o dígito informado.
        var primeiroDigito = CalcularDigito(digitos[..9], multiplicador1);
        if (digitos[9] != primeiroDigito)
            return false;

        var segundoDigito = CalcularDigito(digitos[..10], multiplicador2);
        return digitos[10] == segundoDigito;
    }

    private static char CalcularDigito(string parcial, int[] multiplicadores)
    {
        var soma = parcial.Select((t, i) => (t - '0') * multiplicadores[i]).Sum();
        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;
        return digito.ToString()[0];
    }

    public static bool CpfValido(string cpf) =>
        FormatoValido(cpf) && DigitoVerificadorValido(cpf);
}
