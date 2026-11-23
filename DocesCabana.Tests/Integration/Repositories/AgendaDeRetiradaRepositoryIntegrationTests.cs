using DocesCabana.Tests.Integration.Infraestrutura;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext.Seed;
using DocesCabana.Infrastructure.Repositories;

namespace DocesCabana.Tests.Integration.Repositories;

public class AgendaDeRetiradaRepositoryIntegrationTests : InfraestruturaPostgresDescartavel
{
    private static readonly DateOnly Segunda = new(2026, 11, 23);

    [Fact]
    public async Task Dado_AgendaComHorariosEBloqueio_Quando_SalvarERecarregar_Entao_DeveManterColecoes()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new AgendaDeRetiradaRepository(Contexto);
        var agenda = new AgendaDeRetirada(90, 30);
        agenda.DefinirHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));
        agenda.BloquearDia(Segunda, "Feriado", Segunda);
        await repositorio.Adicionar(agenda);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var recarregada = await repositorio.Obter();

        Assert.Equal(90, recarregada.DuracaoDaFaixaEmMinutos);
        Assert.Equal(30, recarregada.PreparoEmMinutos);
        Assert.Equal(7, recarregada.Horarios.Count);
        var segunda = recarregada.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Monday);
        Assert.Equal(new TimeOnly(9, 0), segunda.Abertura);
        Assert.Equal(new TimeOnly(18, 0), segunda.Fechamento);
        Assert.False(recarregada.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Sunday).Aberto);
        var bloqueio = Assert.Single(recarregada.DiasBloqueados);
        Assert.Equal(Segunda, bloqueio.Data);
        Assert.Equal("Feriado", bloqueio.Motivo);
    }

    [Fact]
    public async Task Dado_AgendaRastreada_Quando_AlterarEDesbloquear_Entao_DevePersistirAsMudancas()
    {
        var unidadeDeTrabalho = new UnitOfWork(Contexto);
        var repositorio = new AgendaDeRetiradaRepository(Contexto);
        var agenda = new AgendaDeRetirada(60, 60);
        agenda.BloquearDia(Segunda, null, Segunda);
        await repositorio.Adicionar(agenda);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var rastreada = await repositorio.Obter();
        rastreada.AlterarFaixas(30, 0);
        rastreada.DefinirHorario(DayOfWeek.Saturday, new TimeOnly(8, 0), new TimeOnly(12, 0));
        rastreada.DesbloquearDia(Segunda);
        rastreada.BloquearDia(Segunda.AddDays(1), "Folga", Segunda);
        await unidadeDeTrabalho.SalvarAlteracoes();
        Contexto.ChangeTracker.Clear();

        var recarregada = await repositorio.Obter();

        Assert.Equal(30, recarregada.DuracaoDaFaixaEmMinutos);
        Assert.Equal(new TimeOnly(8, 0), recarregada.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Saturday).Abertura);
        var bloqueio = Assert.Single(recarregada.DiasBloqueados);
        Assert.Equal(Segunda.AddDays(1), bloqueio.Data);
    }

    [Fact]
    public async Task Dado_BancoSemAgenda_Quando_Obter_Entao_DeveLancarInvalidOperationException()
    {
        var repositorio = new AgendaDeRetiradaRepository(Contexto);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repositorio.Obter());
    }

    [Fact]
    public async Task Dado_BancoSemeadoDuasVezes_Quando_SemearAgendaDeRetirada_Entao_DeveHaverUmaAgendaDeSegundaASabado()
    {
        await DbInitializer.SemearAgendaDeRetirada(Contexto);
        await DbInitializer.SemearAgendaDeRetirada(Contexto);
        Contexto.ChangeTracker.Clear();

        var agenda = await new AgendaDeRetiradaRepository(Contexto).Obter();

        Assert.Single(Contexto.AgendasDeRetirada);
        Assert.Equal(60, agenda.DuracaoDaFaixaEmMinutos);
        Assert.Equal(60, agenda.PreparoEmMinutos);
        Assert.False(agenda.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Sunday).Aberto);
        Assert.All(agenda.Horarios.Where(h => h.DiaDaSemana != DayOfWeek.Sunday), h =>
        {
            Assert.Equal(new TimeOnly(9, 0), h.Abertura);
            Assert.Equal(new TimeOnly(18, 0), h.Fechamento);
        });
    }
}
