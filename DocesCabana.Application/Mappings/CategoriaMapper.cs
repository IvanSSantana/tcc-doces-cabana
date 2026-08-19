using DocesCabana.Application.DTOs;
using DocesCabana.Application.Servicos;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class CategoriaMapper
{
    public static CategoriaDTO ToDTO(Categoria categoria, IReadOnlyDictionary<Guid, int> contagemPorSubcategoria)
    {
        var subcategoriasOrdenadas = categoria.Subcategorias
            .OrderByDescending(s => contagemPorSubcategoria.GetValueOrDefault(s.SubcategoriaId))
            .ThenBy(s => s.Nome)
            .ToList();

        return new CategoriaDTO
        {
            CategoriaId = categoria.CategoriaId,
            Nome = categoria.Nome,
            Apelido = Apelido.De(categoria.Nome),
            Subcategorias = SubcategoriaMapper.ToDTO(subcategoriasOrdenadas)
        };
    }
}
