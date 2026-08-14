using DocesCabana.Application.Enums;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IAvaliacaoRepository : IRepository<Avaliacao>
{
    Task<IEnumerable<Avaliacao>> BuscarPorProduto(Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade);

    Task<int> ContarPorProduto(Guid produtoId);

    Task<IReadOnlyDictionary<byte, int>> ContarPorNota(Guid produtoId);

    Task<Avaliacao?> BuscarComVotos(Guid avaliacaoId);
}
