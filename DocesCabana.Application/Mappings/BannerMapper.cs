using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using DocesCabana.Application.Helpers;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Mappings;

public static class BannerMapper
{
    /// <summary>
    /// Espera o banner carregado com o destino (categoria; subcategoria e a categoria dela; produto),
    /// como <c>IBannerRepository.ListarVigentes</c> devolve.
    /// </summary>
    public static BannerDoCarrosselDTO ToCarrosselDTO(Banner banner) => banner.TipoDeDestino switch
    {
        TipoDeDestinoDoBanner.Categoria => new(
            banner.ImagemUrl, banner.TextoAlternativo, banner.TipoDeDestino,
            ApelidoHelper.De(banner.Categoria!.Nome), null, null),
        TipoDeDestinoDoBanner.Subcategoria => new(
            banner.ImagemUrl, banner.TextoAlternativo, banner.TipoDeDestino,
            ApelidoHelper.De(banner.Subcategoria!.Categoria!.Nome), ApelidoHelper.De(banner.Subcategoria.Nome), null),
        _ => new(
            banner.ImagemUrl, banner.TextoAlternativo, banner.TipoDeDestino,
            null, null, banner.ProdutoId),
    };

    public static ResumoDeBannerDTO ToResumoDTO(Banner banner, DateOnly hoje) => new(
        banner.BannerId,
        banner.ImagemUrl,
        banner.TextoAlternativo,
        DescreverDestino(banner),
        banner.Ordem,
        banner.DataInicio,
        banner.DataFim,
        Situacao(banner, hoje),
        banner.Ativo);

    public static BannerDTO ToDTO(Banner banner) => new()
    {
        BannerId = banner.BannerId,
        TextoAlternativo = banner.TextoAlternativo,
        Ordem = banner.Ordem,
        DataInicio = banner.DataInicio,
        DataFim = banner.DataFim,
        TipoDeDestino = banner.TipoDeDestino,
        CategoriaId = banner.CategoriaId,
        SubcategoriaId = banner.SubcategoriaId,
        ProdutoId = banner.ProdutoId,
        ImagemUrl = banner.ImagemUrl,
    };

    private static string DescreverDestino(Banner banner) => banner.TipoDeDestino switch
    {
        TipoDeDestinoDoBanner.Categoria => $"Categoria: {banner.Categoria?.Nome}",
        TipoDeDestinoDoBanner.Subcategoria => $"Subcategoria: {banner.Subcategoria?.Categoria?.Nome} › {banner.Subcategoria?.Nome}",
        _ => $"Produto: {banner.Produto?.Nome}",
    };

    private static SituacaoDoBanner Situacao(Banner banner, DateOnly hoje)
    {
        if (!banner.Ativo)
            return SituacaoDoBanner.Desativado;

        if (hoje < banner.DataInicio)
            return SituacaoDoBanner.Agendado;

        if (banner.DataFim is not null && hoje > banner.DataFim)
            return SituacaoDoBanner.Encerrado;

        return SituacaoDoBanner.Vigente;
    }
}
