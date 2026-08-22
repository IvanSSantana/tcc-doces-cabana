using DocesCabana.Application.DTOs;
using DocesCabana.Application.Servicos;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class SubcategoriaMapper
{
    public static SubcategoriaDTO ToDTO(Subcategoria subcategoria) =>
        new()
        {
            SubcategoriaId = subcategoria.SubcategoriaId,
            Nome = subcategoria.Nome,
            Apelido = Apelido.De(subcategoria.Nome)
        };

    public static List<SubcategoriaDTO> ToDTO(IEnumerable<Subcategoria> subcategorias) =>
        subcategorias.Select(ToDTO).ToList();
}
