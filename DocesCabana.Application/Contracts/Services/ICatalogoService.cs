using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface ICatalogoService
{
    Task<CatalogoDTO> Montar(CriteriosDoCatalogoDTO criterios, int pagina, Guid? usuarioId = null);
}
