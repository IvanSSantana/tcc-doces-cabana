using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class EnderecoMapper
{
    public static EnderecoDTO ToDTO(Endereco endereco) => new()
    {
        EnderecoId = endereco.EnderecoId,
        Estado = endereco.Estado,
        Cidade = endereco.Cidade,
        Bairro = endereco.Bairro,
        CEP = endereco.CEP,
        Rua = endereco.Rua,
        Numero = endereco.Numero,
        Complemento = endereco.Complemento,
        Padrao = endereco.Padrao,
    };

    public static List<EnderecoDTO> ToDTO(IEnumerable<Endereco> enderecos) =>
        enderecos.Select(ToDTO).ToList();

    // Só para cadastro — edição passa pelo AtualizarDados de uma entidade já
    // existente, não por um novo objeto (Princípio II: estado muda por
    // método de intenção, não por reconstrução).
    public static Endereco ToEntity(EnderecoDTO dto, Guid usuarioId) =>
        new(usuarioId, dto.Estado, dto.Cidade, dto.Bairro, dto.CEP, dto.Rua, dto.Numero, dto.Complemento);
}
