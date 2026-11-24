using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.Models;

public class FormularioDeBannerViewModel
{
    public BannerDTO Banner { get; init; } = new();

    public IReadOnlyList<CategoriaDTO> Categorias { get; init; } = [];

    public IReadOnlyList<ProdutoDTO> Produtos { get; init; } = [];

    public bool EhNovo => Banner.BannerId == Guid.Empty;
}
