using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> BuscarPorCpf(string cpf);
}
