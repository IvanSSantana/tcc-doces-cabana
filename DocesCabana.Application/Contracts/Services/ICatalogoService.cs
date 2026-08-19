using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ICatalogoService
{
    /// <summary>
    /// Lança <see cref="KeyNotFoundException"/> quando <paramref name="apelidoDaCategoria"/>
    /// não corresponde a nenhuma categoria (RF-07). <c>null</c> é o catálogo completo.
    /// </summary>
    Task<CatalogoDTO> Montar(string? apelidoDaCategoria, FiltroCatalogoDTO filtro, int pagina);
}
