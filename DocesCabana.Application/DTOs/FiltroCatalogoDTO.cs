using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public record FiltroCatalogoDTO(
    Guid? CategoriaId,
    IReadOnlyCollection<Guid> SubcategoriaIds,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao,
    string? TermoNormalizado = null);
