using DocesCabana.Application.DTOs.Autenticacao;

namespace DocesCabana.Infrastructure.Identity.Mappings;

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
