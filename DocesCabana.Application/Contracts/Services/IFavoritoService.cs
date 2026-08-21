using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IFavoritoService
{
    /// <summary>Alterna o favorito e devolve o estado resultante: true = passou a favorito.</summary>
    Task<bool> Alternar(Guid produtoId, Guid usuarioId);

    Task<List<ProdutoDTO>> ListarDoUsuario(Guid usuarioId);
}
