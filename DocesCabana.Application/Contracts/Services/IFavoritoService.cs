using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IFavoritoService
{
    Task<bool> Alternar(Guid produtoId, Guid usuarioId);

    Task<List<ProdutoDTO>> ListarDoUsuario(Guid usuarioId);
}
