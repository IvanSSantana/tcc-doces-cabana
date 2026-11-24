namespace DocesCabana.Application.Constants;

public static class ChavesDeCache
{
    public const string CategoriasComSubcategorias = "categorias-com-subcategorias";

    public static string PromocoesVigentes(DateOnly dia) => $"promocoes-vigentes:{dia:yyyy-MM-dd}";

    public const string BannersVigentes = "banners-vigentes";
}
