using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Application.Contracts.Services;

public interface ICupomService
{
    Task<ResultadoDoCupomDTO> Avaliar(
        string codigo, Guid usuarioId, CarrinhoDTO carrinho, decimal? valorDoFrete,
        ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega);

    Task<bool> CodigoJaExiste(string codigo);

    Task<IReadOnlyList<ResumoDeCupomDTO>> ListarParaGestao();

    Task<CupomDTO> BuscarParaEdicao(Guid cupomId);

    Task Cadastrar(CupomDTO dto);

    Task Atualizar(CupomDTO dto);

    Task Ativar(Guid cupomId);

    Task Desativar(Guid cupomId);
}
