using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Tests.Units.Mappings;

public class UsuarioMapperTests
{
    [Fact]
    public void Dado_UsuarioEConta_Quando_ToDTO_Entao_DevePreservarTodosOsCampos()
    {
        var id = Guid.NewGuid();
        var conta = new ContaDeAcesso("joao.silva@example.com");
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(conta, id);

        var usuario = new Usuario(id, "João Silva", "529.982.247-25", "(11) 98765-4321", new DateTime(1990, 1, 1));

        var dto = UsuarioMapper.ToDTO(usuario, conta);

        Assert.Equal(id, dto.Id);
        Assert.Equal("João Silva", dto.Nome);
        Assert.Equal("joao.silva@example.com", dto.Email);
        Assert.Equal("11987654321", dto.Celular);
        Assert.Equal(new DateTime(1990, 1, 1), dto.DataNascimento);
        Assert.Equal("52998224725", dto.CPF);
    }
}
