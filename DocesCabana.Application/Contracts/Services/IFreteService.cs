using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IFreteService
{
    // Nunca lança por falha de transporte: indisponibilidade do serviço,
    // CEP não atendido ou credencial inválida são condição esperada
    // (RN-02 da spec 020, Princípio VIII) e voltam em
    // CotacaoDeFreteDTO.Mensagem — nunca como exceção. Isso evita um ramo
    // novo no FilterException e é o que faz a aplicação, sem credencial
    // nenhuma, se comportar exatamente como com o serviço fora do ar.
    //
    // itensDisponiveis já vem filtrado por quem chama (RN-03): o adaptador
    // não conhece ProdutoStatus.
    Task<CotacaoDeFreteDTO> Cotar(string cepDestino, IReadOnlyList<LinhaDoCarrinhoDTO> itensDisponiveis);
}
