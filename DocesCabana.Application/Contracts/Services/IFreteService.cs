using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IFreteService
{
    Task<CotacaoDeFreteDTO> Cotar(string cepDestino, IReadOnlyList<LinhaDoCarrinhoDTO> itensDisponiveis);
}
