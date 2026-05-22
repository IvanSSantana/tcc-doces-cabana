using DocesCabana.Application.DTOs.Auth;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Mappings;

public static class UsuarioMapper
{
    public static Usuario ToEntity(CadastroDTO dto) =>
        new(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.DataNascimento,
            dto.CPF);
}
