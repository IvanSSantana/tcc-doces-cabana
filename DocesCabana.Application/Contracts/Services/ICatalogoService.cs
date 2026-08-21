using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ICatalogoService
{
    /// <summary>
    /// Lança <see cref="KeyNotFoundException"/> quando <paramref name="apelidoDaCategoria"/>
    /// não corresponde a nenhuma categoria (RF-07). <c>null</c> é o catálogo completo.
    /// </summary>
    /// <summary>
    /// <paramref name="usuarioId"/> nulo é visitante — nenhum produto vem
    /// marcado como favorito (spec 015, RF-02).
    /// </summary>
    Task<CatalogoDTO> Montar(string? apelidoDaCategoria, FiltroCatalogoDTO filtro, int pagina, Guid? usuarioId = null);
}
