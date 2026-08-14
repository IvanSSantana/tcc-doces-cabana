using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IAvaliacaoService
{
    Task<ResumoAvaliacoesDTO> ResumirPorProduto(Guid produtoId);

    Task<PaginaAvaliacoesDTO> ListarPorProduto(Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade, Guid? usuarioAtual);

    /// <summary>Alterna o voto de útil e devolve o <c>ProdutoId</c> da avaliação, para o controller redirecionar de volta.</summary>
    Task<Guid> AlternarVotoUtil(Guid avaliacaoId, Guid usuarioId);
}
