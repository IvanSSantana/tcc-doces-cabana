using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.MVC.Models;

public class GestaoDeProdutosViewModel
{
    public PaginaDeProdutosDTO Pagina { get; init; } = new();

    public FiltroDeGestaoDeProduto Filtro { get; init; }
}
