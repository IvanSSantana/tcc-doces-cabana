using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;

namespace DocesCabana.Application.Contracts.Services;

public interface IPromocaoService
{
    Task<PromocoesVigentes> CarregarVigentes();

    Task<IReadOnlyList<ResumoDePromocaoDTO>> ListarParaGestao();

    Task<PromocaoDTO> BuscarParaEdicao(Guid promocaoId);

    Task Cadastrar(PromocaoDTO dto);

    Task Atualizar(PromocaoDTO dto);

    Task Ativar(Guid promocaoId);

    Task Desativar(Guid promocaoId);
}
