using DocesCabana.Domain.Constants;
using DocesCabana.Tests.Integration.Infraestrutura;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Tests.Integration.Repositories;

/// <summary>
/// Spec 039, D4: o fim da desativação precisa sobreviver à ida e volta do Postgres, que guarda
/// microssegundos. Com <see cref="DateTimeOffset.MaxValue"/> o valor voltaria truncado.
/// </summary>
public class DesativacaoDeContaIntegrationTests : InfraestruturaPostgresDescartavel
{
    [Fact]
    public async Task Dado_ContaComLockoutAteOFimDaDesativacao_Quando_LerDoBanco_Entao_DeveContinuarDesativada()
    {
        var contaId = await SemearUsuario();
        var conta = await Contexto.Users.SingleAsync(c => c.Id == contaId);
        conta.LockoutEnabled = true;
        conta.LockoutEnd = AdministradorPrincipal.FimDaDesativacao;
        await Contexto.SaveChangesAsync();

        var lida = await Contexto.Users.AsNoTracking().SingleAsync(c => c.Id == contaId);

        Assert.True(lida.Desativada);
    }

    [Fact]
    public async Task Dado_ContaComBloqueioTemporario_Quando_LerDoBanco_Entao_NaoDeveSerDesativada()
    {
        var contaId = await SemearUsuario();
        var conta = await Contexto.Users.SingleAsync(c => c.Id == contaId);
        conta.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
        await Contexto.SaveChangesAsync();

        var lida = await Contexto.Users.AsNoTracking().SingleAsync(c => c.Id == contaId);

        Assert.False(lida.Desativada);
    }
}
