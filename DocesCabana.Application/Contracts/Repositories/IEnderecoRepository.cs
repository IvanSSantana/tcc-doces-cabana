using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IEnderecoRepository
{
    Task<List<Endereco>> BuscarPorUsuario(Guid usuarioId);

    Task<Endereco?> Buscar(Guid enderecoId, Guid usuarioId);

    Task Adicionar(Endereco endereco);

    void Remover(Endereco endereco);
}
