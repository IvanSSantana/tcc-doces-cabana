using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Infrastructure.Identity.Mappings;

public static class UsuarioMapper
{
    public static UsuarioDTO ToDTO(Usuario usuario, ContaDeAcesso conta) =>
        new()
        {
            Id = usuario.UsuarioId,
            Nome = usuario.Nome,
            Email = conta.Email!,
            Celular = usuario.Celular,
            DataNascimento = usuario.DataNascimento,
            CPF = usuario.CPF
        };
}
