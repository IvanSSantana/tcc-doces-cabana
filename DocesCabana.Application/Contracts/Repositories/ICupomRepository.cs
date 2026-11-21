using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface ICupomRepository : IRepository<Cupom>
{
    Task<Cupom?> BuscarPorCodigo(string codigoNormalizado);

    Task<bool> ExisteCodigo(string codigoNormalizado);

    Task<List<Cupom>> ListarParaGestao();
}
