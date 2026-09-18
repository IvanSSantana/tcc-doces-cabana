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

    public static Endereco ToEntity(EnderecoDTO dto, Guid usuarioId) =>
        new(usuarioId, dto.Estado, dto.Cidade, dto.Bairro, dto.CEP, dto.Rua, dto.Numero, dto.Complemento);
}
