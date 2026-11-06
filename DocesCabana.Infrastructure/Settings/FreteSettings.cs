namespace DocesCabana.Infrastructure.Settings;

public class FreteSettings
{
    public string UrlBase { get; set; } = "https://sandbox.melhorenvio.com.br";

    public string Token { get; set; } = string.Empty;

    public string CepDeOrigem { get; set; } = "17340001";

    public string UserAgent { get; set; } = "Doces Cabana (contato@docescabana.com.br)";

    public int TimeoutEmSegundos { get; set; } = 10;
}
