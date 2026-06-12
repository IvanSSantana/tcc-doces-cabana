using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Infrastructure.Identity.Mappings;

public static class UsuarioMapper
{
    public static Usuario ToEntity(CadastroDTO dto) =>
        new(
            dto.Nome!,
            dto.Email!,
            TelefoneHelper.ApenasDigitos(dto.Telefone!),
            dto.DataNascimento ?? new DateTime(),
            CpfHelper.ApenasDigitos(dto.CPF!));
}
