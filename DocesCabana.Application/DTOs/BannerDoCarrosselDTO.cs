using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.DTOs;

/// <summary>
/// Banner pronto para o carrossel. O endereço é montado na view a partir dos apelidos (ou do
/// id do produto), porque a rota do catálogo é coisa do MVC.
/// </summary>
public record BannerDoCarrosselDTO(
    string ImagemUrl,
    string TextoAlternativo,
    TipoDeDestinoDoBanner TipoDeDestino,
    string? ApelidoDaCategoria,
    string? ApelidoDaSubcategoria,
    Guid? ProdutoId);
