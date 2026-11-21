using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.Models;

public class FormularioDeCupomViewModel
{
    public CupomDTO Cupom { get; init; } = new();

    public IReadOnlyList<CategoriaDTO> Categorias { get; init; } = [];

    public bool EhNovo => Cupom.CupomId == Guid.Empty;
}
