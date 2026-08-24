using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IEnderecoService
{
    Task<List<EnderecoDTO>> ListarDoUsuario(Guid usuarioId);
    Task<EnderecoDTO> BuscarDoUsuario(Guid enderecoId, Guid usuarioId);
    Task Cadastrar(EnderecoDTO dto, Guid usuarioId);
    Task Editar(EnderecoDTO dto, Guid usuarioId);
    Task Excluir(Guid enderecoId, Guid usuarioId);
    Task TornarPrincipal(Guid enderecoId, Guid usuarioId);
}
