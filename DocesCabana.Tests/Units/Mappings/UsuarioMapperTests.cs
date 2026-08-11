using DocesCabana.Application.DTOs.Autenticacao;
using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.Identity.Mappings;
using Microsoft.AspNetCore.Identity;

namespace DocesCabana.Tests.Units.Mappings;

public class UsuarioMapperTests
{
    [Fact]
    public void Dado_UmCadastroDTOComCpfETelefoneFormatados_Quando_CadastroToEntity_Entao_DeveNormalizarParaDigitos()
    {
        var dto = new CadastroDTO
        {
            Nome = "João Silva",
            Email = "joao.silva@example.com",
            Celular = "(11) 98765-4321",
            DataNascimento = new DateTime(1990, 1, 1),
            CPF = "529.982.247-25",
            Senha = "SenhaForte@123"
        };

        var usuario = UsuarioMapper.CadastroToEntity(dto);

        Assert.Equal("11987654321", usuario.PhoneNumber);
        Assert.Equal("52998224725", usuario.CPF);
        Assert.Equal(dto.Nome, usuario.Nome);
        Assert.Equal(dto.Email, usuario.Email);
    }

    [Fact]
    public void Dado_UmaEntidade_Quando_ToDTO_Entao_DevePreservarTodosOsCampos()
    {
        var usuario = new Usuario("João Silva", "joao.silva@example.com", "11987654321", new DateTime(1990, 1, 1), "52998224725");
        typeof(IdentityUser<Guid>).GetProperty(nameof(IdentityUser<Guid>.Id))!.SetValue(usuario, Guid.NewGuid());

        var dto = UsuarioMapper.ToDTO(usuario);

        Assert.Equal(usuario.Id, dto.Id);
        Assert.Equal(usuario.Nome, dto.Nome);
        Assert.Equal(usuario.Email, dto.Email);
        Assert.Equal(usuario.PhoneNumber, dto.Celular);
        Assert.Equal(usuario.DataNascimento, dto.DataNascimento);
        Assert.Equal(usuario.CPF, dto.CPF);
    }
}
