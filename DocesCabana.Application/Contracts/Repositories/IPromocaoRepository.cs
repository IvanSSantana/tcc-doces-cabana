using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IPromocaoRepository : IRepository<Promocao>
{
    Task<List<Promocao>> ListarVigentes(DateOnly hoje);

    Task<List<Promocao>> ListarParaGestao();

    Task<Promocao?> BuscarComProdutos(Guid promocaoId);
}
