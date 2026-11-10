using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface IAvaliacaoService
{
    Task<ResumoAvaliacoesDTO> ResumirPorProduto(Guid produtoId);

    Task<PaginaAvaliacoesDTO> ListarPorProduto(Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade, Guid? usuarioAtual);

    Task<Guid> AlternarVotoUtil(Guid avaliacaoId, Guid usuarioId);

    Task<SituacaoDaAvaliacao> VerificarSituacao(Guid produtoId, Guid usuarioId);

    Task Avaliar(NovaAvaliacaoDTO dto, Guid usuarioId);
}
