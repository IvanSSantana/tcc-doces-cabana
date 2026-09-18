namespace DocesCabana.Infrastructure.Services;

public class SupabaseSettings
{
    public string UrlBase { get; set; } = "https://mjnlzsucdsxqahabsniy.supabase.co";

    public string Bucket { get; set; } = "images";

    public string Pasta { get; set; } = "public";

    public string ChaveDeServico { get; set; } = string.Empty;

    public int TimeoutEmSegundos { get; set; } = 10;
}
