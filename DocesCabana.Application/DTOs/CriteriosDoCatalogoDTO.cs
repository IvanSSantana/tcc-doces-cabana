using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

public record CriteriosDoCatalogoDTO(
    string? ApelidoDaCategoria,
    IReadOnlyCollection<string> ApelidosDeSubcategoria,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao,
    string? Termo = null);
