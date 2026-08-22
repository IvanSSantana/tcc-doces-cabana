using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ICatalogoService
{
    /// <summary>
    /// Lança <see cref="KeyNotFoundException"/> quando
    /// <paramref name="criterios"/>.ApelidoDaCategoria não corresponde a
    /// nenhuma categoria (RF-07 da 012). <c>null</c> é o catálogo completo.
    /// Apelido de subcategoria que não existir na categoria resolvida é
    /// ignorado, não recusado (spec 016, RN-04).
    /// </summary>
    /// <summary>
    /// <paramref name="usuarioId"/> nulo é visitante — nenhum produto vem
    /// marcado como favorito (spec 015, RF-02).
    /// </summary>
    Task<CatalogoDTO> Montar(CriteriosDoCatalogoDTO criterios, int pagina, Guid? usuarioId = null);
}
