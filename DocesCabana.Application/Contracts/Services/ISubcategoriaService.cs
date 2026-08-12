using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ISubcategoriaService
{
    Task<List<SubcategoriaDTO>> BuscarTodasSubcategorias();
}
