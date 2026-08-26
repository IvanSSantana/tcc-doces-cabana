namespace DocesCabana.Infrastructure.Services;

public class FreteSettings
{
    // Sandbox por padrão (spec 020 §10) — trocar para produção é só mudar
    // a URL, o código não muda.
    public string UrlBase { get; set; } = "https://sandbox.melhorenvio.com.br";

    // Nunca versionado (RN-05) — vem de user secrets em desenvolvimento, de
    // variável de ambiente em produção e no teste de ponta a ponta (T050).
    public string Token { get; set; } = string.Empty;

    // CEP da loja, não é segredo (spec 020 §10) — vai versionado.
    public string CepDeOrigem { get; set; } = "17340001";

    // Obrigatório pela API do MelhorEnvio (nome da aplicação + e-mail de
    // contato técnico) — sem ele a API recusa a requisição. Configuração,
    // não literal no código, porque é dado de quem opera a loja. Precisa de
    // um padrão não vazio: HttpHeaders.UserAgent.ParseAdd("") lança
    // FormatException — achado ao rodar o E2E sem a variável configurada,
    // que derrubava a página com 500 em vez de mostrar "não foi possível
    // calcular o frete agora" (RN-02).
    public string UserAgent { get; set; } = "Doces Cabana (contato@docescabana.com.br)";

    public int TimeoutEmSegundos { get; set; } = 10;
}
