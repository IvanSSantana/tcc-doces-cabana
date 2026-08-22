using DocesCabana.Application.Enums;

namespace DocesCabana.Application.DTOs;

// Categoria fica de fora de propósito: BuscarPaginaDoCatalogo/ContarNoCatalogo
// recebem o Guid da categoria como parâmetro à parte — filtrar "todos" (sem
// categoria) é um caso do próprio catálogo, não uma variação do filtro.
public record FiltroCatalogoDTO(
    Guid? CategoriaId,
    IReadOnlyCollection<Guid> SubcategoriaIds,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao,
    // Já normalizado (spec 016) — comparado direto contra Produto.NomeNormalizado.
    // Nulo/vazio não filtra por nome (RF-09).
    string? TermoNormalizado = null);
