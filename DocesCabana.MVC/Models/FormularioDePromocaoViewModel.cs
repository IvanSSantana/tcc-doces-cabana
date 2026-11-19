using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.Models;

public class FormularioDePromocaoViewModel
{
    public PromocaoDTO Promocao { get; init; } = new();

    public IReadOnlyList<CategoriaDTO> Categorias { get; init; } = [];

    public IReadOnlyList<ProdutoDTO> Produtos { get; init; } = [];

    public bool EhNova => Promocao.PromocaoId == Guid.Empty;
}
