using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

// Sem BuscarPorId(enderecoId) sozinho — de propósito (spec 018, plano §5).
// Todo método recebe usuarioId, e Buscar exige o par: é o desenho que torna
// a RN-05 (isolamento entre pessoas) difícil de violar por esquecimento, em
// vez de depender de o chamador lembrar de conferir o dono depois de buscar.
public interface IEnderecoRepository
{
    // Ordenado por DataCadastro — é o critério que a RN-04 usa para saber
    // qual endereço promover ao excluir o principal.
    Task<List<Endereco>> BuscarPorUsuario(Guid usuarioId);

    Task<Endereco?> Buscar(Guid enderecoId, Guid usuarioId);

    Task Adicionar(Endereco endereco);

    void Remover(Endereco endereco);
}
