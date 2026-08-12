namespace DocesCabana.Domain.Helpers;

public static class CepHelper
{
    public static string ApenasDigitos(string valor) =>
        new string(valor.Where(char.IsDigit).ToArray());

    public static bool FormatoValido(string cep) =>
        ApenasDigitos(cep).Length == 8;
}
