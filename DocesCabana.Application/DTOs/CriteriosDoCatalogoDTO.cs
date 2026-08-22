using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

// O que o endereço pediu, no vocabulário de quem lê o endereço (spec 016,
// plano §5): apelidos, não identificadores. A tradução para o que o
// repositório entende é trabalho exclusivo de CatalogoService.Montar, que é
// o único que conhece a categoria atual e a taxonomia inteira — o
// repositório nunca vê este tipo.
public record CriteriosDoCatalogoDTO(
    string? ApelidoDaCategoria,
    IReadOnlyCollection<string> ApelidosDeSubcategoria,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao,
    // Cru, como veio da URL — normalizar é trabalho de CatalogoService.Montar,
    // não deste registro nem do controller (spec 016, RN-02).
    string? Termo = null);
