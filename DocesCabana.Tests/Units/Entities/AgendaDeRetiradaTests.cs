using DocesCabana.Domain.Entities;

namespace DocesCabana.Tests.Units.Entities;

public class AgendaDeRetiradaTests
{
    // 2026-11-23 é segunda-feira; 2026-11-22, domingo; 2026-11-21, sábado.
    private static readonly DateOnly Segunda = new(2026, 11, 23);
    private static readonly DateOnly Sabado = new(2026, 11, 21);
    private static readonly DateOnly Domingo = new(2026, 11, 22);

    private static readonly TimeOnly Nove = new(9, 0);
    private static readonly TimeOnly Dezoito = new(18, 0);

    private static AgendaDeRetirada CriarAgendaPadrao(int duracao = 60, int preparo = 60)
    {
        var agenda = new AgendaDeRetirada(duracao, preparo);

        foreach (var dia in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
            agenda.DefinirHorario(dia, Nove, Dezoito);

        return agenda;
    }

    private static DateTime Em(DateOnly data, int hora, int minuto = 0) => data.ToDateTime(new TimeOnly(hora, minuto));

    [Fact]
    public void Dado_AgendaNova_Quando_Criar_Entao_DeveTerSeteDiasFechados()
    {
        var agenda = new AgendaDeRetirada(60, 60);

        Assert.Equal(7, agenda.Horarios.Count);
        Assert.All(agenda.Horarios, h => Assert.False(h.Aberto));
        Assert.Empty(agenda.DiasBloqueados);
    }

    [Fact]
    public void Dado_SegundaAs07hComFaixaDe60EPreparoDe60_Quando_FaixasDisponiveis_Entao_DeveRetornarNoveFaixas()
    {
        var faixas = CriarAgendaPadrao().FaixasDisponiveis(Em(Segunda, 7));

        Assert.Equal(9, faixas.Count);
        Assert.Equal(new TimeOnly(9, 0), faixas[0].Inicio);
        Assert.Equal(new TimeOnly(10, 0), faixas[0].Fim);
        Assert.Equal(new TimeOnly(17, 0), faixas[^1].Inicio);
        Assert.Equal(new TimeOnly(18, 0), faixas[^1].Fim);
        Assert.All(faixas, f => Assert.Equal(Segunda, f.Data));
    }

    [Fact]
    public void Dado_FaixaDe90Minutos_Quando_FaixasDisponiveis_Entao_UltimaFaixaDeveTerminarNoFechamento()
    {
        var faixas = CriarAgendaPadrao(duracao: 90, preparo: 0).FaixasDisponiveis(Em(Segunda, 7));

        Assert.Equal(6, faixas.Count);
        Assert.Equal(new TimeOnly(16, 30), faixas[^1].Inicio);
        Assert.Equal(new TimeOnly(18, 0), faixas[^1].Fim);
    }

    [Fact]
    public void Dado_SegundaAs10h20ComPreparoDe60_Quando_FaixasDisponiveis_Entao_PrimeiraFaixaDeveComecarAs12h()
    {
        var faixas = CriarAgendaPadrao().FaixasDisponiveis(Em(Segunda, 10, 20));

        Assert.Equal(new TimeOnly(12, 0), faixas[0].Inicio);
    }

    [Fact]
    public void Dado_SegundaAs11hComPreparoDe60_Quando_FaixasDisponiveis_Entao_FaixaDoMeioDiaDeveAparecer()
    {
        var faixas = CriarAgendaPadrao().FaixasDisponiveis(Em(Segunda, 11));

        Assert.Equal(new TimeOnly(12, 0), faixas[0].Inicio);
    }

    [Fact]
    public void Dado_Domingo_Quando_FaixasDisponiveis_Entao_DeveRetornarVazio()
    {
        Assert.Empty(CriarAgendaPadrao().FaixasDisponiveis(Em(Domingo, 7)));
    }

    [Fact]
    public void Dado_DiaBloqueado_Quando_FaixasDisponiveis_Entao_DeveRetornarVazio()
    {
        var agenda = CriarAgendaPadrao();
        agenda.BloquearDia(Segunda, "Feriado", Segunda);

        Assert.Empty(agenda.FaixasDisponiveis(Em(Segunda, 7)));
    }

    [Fact]
    public void Dado_SegundaAs16h30ComPreparoDe60_Quando_FaixasDisponiveis_Entao_DeveRetornarVazio()
    {
        Assert.Empty(CriarAgendaPadrao().FaixasDisponiveis(Em(Segunda, 16, 30)));
    }

    [Fact]
    public void Dado_SabadoAs18h_Quando_ProximaAbertura_Entao_DeveSerSegundaAs9h()
    {
        var proxima = CriarAgendaPadrao().ProximaAbertura(Em(Sabado, 18));

        Assert.Equal(Em(Segunda, 9), proxima);
    }

    [Fact]
    public void Dado_SegundaSeguinteBloqueada_Quando_ProximaAbertura_Entao_DeveSerTerca()
    {
        var agenda = CriarAgendaPadrao();
        agenda.BloquearDia(Segunda, null, Sabado);

        Assert.Equal(Em(Segunda.AddDays(1), 9), agenda.ProximaAbertura(Em(Sabado, 18)));
    }

    [Fact]
    public void Dado_SemanaTodaFechada_Quando_ProximaAbertura_Entao_DeveSerNula()
    {
        Assert.Null(new AgendaDeRetirada(60, 60).ProximaAbertura(Em(Segunda, 7)));
    }

    [Fact]
    public void Dado_DiaAberto_Quando_DefinirHorarioComAmbosNulos_Entao_DeveFicarFechado()
    {
        var agenda = CriarAgendaPadrao();

        agenda.DefinirHorario(DayOfWeek.Monday, null, null);

        Assert.False(agenda.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Monday).Aberto);
    }

    [Fact]
    public void Dado_AberturaDepoisDoFechamento_Quando_DefinirHorario_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60, 60);

        Assert.Throws<ArgumentException>(() => agenda.DefinirHorario(DayOfWeek.Monday, Dezoito, Nove));
    }

    [Fact]
    public void Dado_SoAbertura_Quando_DefinirHorario_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60, 60);

        Assert.Throws<ArgumentException>(() => agenda.DefinirHorario(DayOfWeek.Monday, Nove, null));
    }

    [Theory]
    [InlineData(14)]
    [InlineData(241)]
    public void Dado_DuracaoForaDoLimite_Quando_Criar_Entao_DeveLancarArgumentException(int duracao)
    {
        Assert.Throws<ArgumentException>(() => new AgendaDeRetirada(duracao, 60));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(481)]
    public void Dado_PreparoForaDoLimite_Quando_AlterarFaixas_Entao_DeveLancarArgumentException(int preparo)
    {
        var agenda = new AgendaDeRetirada(60, 60);

        Assert.Throws<ArgumentException>(() => agenda.AlterarFaixas(60, preparo));
    }

    [Fact]
    public void Dado_LimitesValidos_Quando_AlterarFaixas_Entao_DeveAlterar()
    {
        var agenda = new AgendaDeRetirada(60, 60);

        agenda.AlterarFaixas(15, 480);

        Assert.Equal(15, agenda.DuracaoDaFaixaEmMinutos);
        Assert.Equal(480, agenda.PreparoEmMinutos);
    }

    [Fact]
    public void Dado_DataNoPassado_Quando_BloquearDia_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60, 60);

        Assert.Throws<ArgumentException>(() => agenda.BloquearDia(Sabado, null, Segunda));
    }

    [Fact]
    public void Dado_DataJaBloqueada_Quando_BloquearDia_Entao_DeveLancarInvalidOperationException()
    {
        var agenda = new AgendaDeRetirada(60, 60);
        agenda.BloquearDia(Segunda, null, Segunda);

        Assert.Throws<InvalidOperationException>(() => agenda.BloquearDia(Segunda, "De novo", Segunda));
    }

    [Fact]
    public void Dado_MotivoLongo_Quando_BloquearDia_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60, 60);

        Assert.Throws<ArgumentException>(() => agenda.BloquearDia(Segunda, new string('a', 101), Segunda));
    }

    [Fact]
    public void Dado_DiaBloqueado_Quando_DesbloquearDia_Entao_DeveRemover()
    {
        var agenda = new AgendaDeRetirada(60, 60);
        agenda.BloquearDia(Segunda, "Feriado", Segunda);

        agenda.DesbloquearDia(Segunda);

        Assert.Empty(agenda.DiasBloqueados);
    }
}
