using DocesCabana.Application.DTOs;
using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Domain.Helpers;

namespace DocesCabana.Infrastructure.Identity.Mappings;

public static class UsuarioMapper
{
    public static Usuario CadastroToEntity(CadastroDTO dto) =>
        new(
            dto.Nome!,
            dto.Email!,
            TelefoneHelper.ApenasDigitos(dto.Telefone!),
            dto.DataNascimento ?? new DateTime(),
            CpfHelper.ApenasDigitos(dto.CPF!));

    public static UsuarioDTO ToDTO(Usuario usuario) =>
        new()
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email!,
            Celular = usuario.PhoneNumber!,
            DataNascimento = usuario.DataNascimento,
            CPF = usuario.CPF
        };
}
