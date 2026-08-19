using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ICategoriaService
{
    Task<List<CategoriaDTO>> ListarComSubcategorias();

    Task<CategoriaDTO?> BuscarPorApelido(string apelido);
}
