namespace DocesCabana.Infrastructure.Services;

public class SupabaseSettings
{
    // URL, bucket e pasta não são segredo (spec 027 §10) — vão versionados,
    // como FreteSettings.CepDeOrigem. Precisa de um padrão não vazio: `new
    // Uri("")` lança UriFormatException já na montagem do HttpClient tipado,
    // antes de qualquer requisição — achado ao rodar o E2E sem a variável
    // configurada, a mesma armadilha que o UserAgent em branco já tinha
    // ensinado na spec 020 (RN-02 de lá). Chave vazia (o padrão real de
    // "não configurado") já basta para recusar sem tocar a rede.
    public string UrlBase { get; set; } = "https://mjnlzsucdsxqahabsniy.supabase.co";

    public string Bucket { get; set; } = "images";

    public string Pasta { get; set; } = "public";

    // Nunca versionado (RN-04) — vem de user secrets em desenvolvimento, de
    // variável de ambiente em produção e no teste de ponta a ponta. Vazio
    // por padrão, e vazio significa recusar sem tocar a rede (CA-09).
    public string ChaveDeServico { get; set; } = string.Empty;

    public int TimeoutEmSegundos { get; set; } = 10;
}
