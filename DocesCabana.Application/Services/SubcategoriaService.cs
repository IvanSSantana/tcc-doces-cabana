using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;

namespace DocesCabana.Application.Services;

public class SubcategoriaService : ISubcategoriaService
{
    private readonly ISubcategoriaRepository _subcategoriaRepository;

    public SubcategoriaService(ISubcategoriaRepository subcategoriaRepository)
    {
        _subcategoriaRepository = subcategoriaRepository;
    }

    public async Task<List<SubcategoriaDTO>> BuscarTodasSubcategorias()
    {
        var subcategorias = await _subcategoriaRepository.BuscarTodos();

        return SubcategoriaMapper.ToDTO(subcategorias);
    }
}
