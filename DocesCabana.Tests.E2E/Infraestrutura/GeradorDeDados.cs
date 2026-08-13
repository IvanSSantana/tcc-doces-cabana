namespace DocesCabana.Tests.E2E.Infraestrutura;

/// <summary>
/// E-mail e CPF únicos por chamada — sem isto, dois testes que cadastram
/// alguém colidem no índice único do sistema (RN-01 da 006) e o segundo
/// falha por um motivo que não tem nada a ver com o que ele queria provar
/// (RN-03 da 007).
/// </summary>
public static class GeradorDeDados
{
    private static int _contador;

    public static string EmailUnico(string prefixo = "pessoa")
    {
        var sufixo = Interlocked.Increment(ref _contador);
        return $"{prefixo}.e2e.{DateTime.UtcNow:HHmmssfff}.{sufixo}@teste.doces.com";
    }

    public static string CelularValido() => "14988887777";

    public static string CpfValido()
    {
        var sufixo = Interlocked.Increment(ref _contador);
        var nove = ProximosNoveDigitos(sufixo);

        return CalcularComDigitosVerificadores(nove);
    }

    private static string ProximosNoveDigitos(int sufixo)
    {
        // CpfHelper rejeita CPF com os 11 dígitos iguais; nunca acontece aqui
        // (a base varia por chamada), mas o guard deixa a garantia explícita
        // em vez de "extremamente improvável".
        var valor = 100_000_000L + sufixo;
        var nove = (valor % 900_000_000).ToString("D9");

        while (nove.Distinct().Count() == 1)
        {
            sufixo = Interlocked.Increment(ref _contador);
            valor = 100_000_000L + sufixo;
            nove = (valor % 900_000_000).ToString("D9");
        }

        return nove;
    }

    private static string CalcularComDigitosVerificadores(string nove)
    {
        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var soma = nove.Select((c, i) => (c - '0') * multiplicador1[i]).Sum();
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;

        var dez = nove + d1;
        soma = dez.Select((c, i) => (c - '0') * multiplicador2[i]).Sum();
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;

        return dez + d2;
    }
}
