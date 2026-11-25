using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Infrastructure.Identity.Mappings;

public static class AdministradorMapper
{
    public static AdministradorDTO ToDTO(Usuario usuario, ContaDeAcesso conta) =>
        new()
        {
            Id = usuario.UsuarioId,
            Nome = usuario.Nome,
            Email = conta.Email!,
            Ativo = !conta.Desativada,
            Principal = conta.EhAdministradorPrincipal,
        };
}
