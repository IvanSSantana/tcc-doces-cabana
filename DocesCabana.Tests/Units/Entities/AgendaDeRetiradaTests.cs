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

    private static AgendaDeRetirada CriarAgendaPadrao(int preparo = 60)
    {
        var agenda = new AgendaDeRetirada(preparo);

        foreach (var dia in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
            agenda.DefinirHorario(dia, Nove, Dezoito);

        return agenda;
    }

    private static DateTime Em(DateOnly data, int hora, int minuto = 0) => data.ToDateTime(new TimeOnly(hora, minuto));

    [Fact]
    public void Dado_AgendaNova_Quando_Criar_Entao_DeveTerSeteDiasFechados()
    {
        var agenda = new AgendaDeRetirada(60);

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
    public void Dado_PreparoDe90Minutos_Quando_FaixasDisponiveis_Entao_UltimaFaixaDeveTerminarNoFechamento()
    {
        var faixas = CriarAgendaPadrao(preparo: 90).FaixasDisponiveis(Em(Segunda, 7));

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
        Assert.Null(new AgendaDeRetirada(60).ProximaAbertura(Em(Segunda, 7)));
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
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.DefinirHorario(DayOfWeek.Monday, Dezoito, Nove));
    }

    [Fact]
    public void Dado_SoAbertura_Quando_DefinirHorario_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.DefinirHorario(DayOfWeek.Monday, Nove, null));
    }

    [Theory]
    [InlineData(14)]
    [InlineData(241)]
    public void Dado_PreparoForaDoLimite_Quando_Criar_Entao_DeveLancarArgumentException(int preparo)
    {
        Assert.Throws<ArgumentException>(() => new AgendaDeRetirada(preparo));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(241)]
    public void Dado_PreparoForaDoLimite_Quando_AlterarTempoDePreparo_Entao_DeveLancarArgumentException(int preparo)
    {
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.AlterarTempoDePreparo(preparo));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(240)]
    public void Dado_PreparoNoLimite_Quando_AlterarTempoDePreparo_Entao_DeveAlterar(int preparo)
    {
        var agenda = new AgendaDeRetirada(60);

        agenda.AlterarTempoDePreparo(preparo);

        Assert.Equal(preparo, agenda.TempoDePreparoEmMinutos);
    }

    [Fact]
    public void Dado_AlmocoDas12hAs13h_Quando_FaixasDisponiveis_Entao_NaoDeveHaverFaixaNoAlmoco()
    {
        var agenda = CriarAgendaPadrao();
        agenda.DefinirAlmoco(new TimeOnly(12, 0), new TimeOnly(13, 0));

        var faixas = agenda.FaixasDisponiveis(Em(Segunda, 7));

        Assert.Equal(8, faixas.Count);
        Assert.DoesNotContain(faixas, f => f.Inicio < new TimeOnly(13, 0) && f.Fim > new TimeOnly(12, 0));
        Assert.Equal(new TimeOnly(11, 0), faixas[2].Inicio);
        Assert.Equal(new TimeOnly(13, 0), faixas[3].Inicio);
    }

    [Fact]
    public void Dado_AlmocoForaDoPasso_Quando_FaixasDisponiveis_Entao_FaixaAnteriorTerminaNoAlmocoERecomecaNoFim()
    {
        var agenda = CriarAgendaPadrao();
        agenda.DefinirAlmoco(new TimeOnly(12, 30), new TimeOnly(13, 30));

        var faixas = agenda.FaixasDisponiveis(Em(Segunda, 7));

        Assert.Contains(faixas, f => f.Inicio == new TimeOnly(12, 0) && f.Fim == new TimeOnly(12, 30));
        Assert.Contains(faixas, f => f.Inicio == new TimeOnly(13, 30) && f.Fim == new TimeOnly(14, 30));
        Assert.Equal(new TimeOnly(17, 30), faixas[^1].Inicio);
        Assert.Equal(new TimeOnly(18, 0), faixas[^1].Fim);
    }

    [Fact]
    public void Dado_AlmocoForaDoExpediente_Quando_FaixasDisponiveis_Entao_DeveIgnorarOAlmoco()
    {
        var agenda = CriarAgendaPadrao();
        agenda.DefinirAlmoco(new TimeOnly(19, 0), new TimeOnly(20, 0));

        Assert.Equal(9, agenda.FaixasDisponiveis(Em(Segunda, 7)).Count);
    }

    [Fact]
    public void Dado_AlmocoDefinido_Quando_DefinirAlmocoComAmbosNulos_Entao_DeveRemover()
    {
        var agenda = CriarAgendaPadrao();
        agenda.DefinirAlmoco(new TimeOnly(12, 0), new TimeOnly(13, 0));

        agenda.DefinirAlmoco(null, null);

        Assert.False(agenda.TemAlmoco);
        Assert.Equal(9, agenda.FaixasDisponiveis(Em(Segunda, 7)).Count);
    }

    [Fact]
    public void Dado_InicioDoAlmocoDepoisDoFim_Quando_DefinirAlmoco_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.DefinirAlmoco(new TimeOnly(13, 0), new TimeOnly(12, 0)));
    }

    [Fact]
    public void Dado_SoInicioDoAlmoco_Quando_DefinirAlmoco_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.DefinirAlmoco(new TimeOnly(12, 0), null));
    }

    [Fact]
    public void Dado_DataNoPassado_Quando_BloquearDia_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.BloquearDia(Sabado, null, Segunda));
    }

    [Fact]
    public void Dado_DataJaBloqueada_Quando_BloquearDia_Entao_DeveLancarInvalidOperationException()
    {
        var agenda = new AgendaDeRetirada(60);
        agenda.BloquearDia(Segunda, null, Segunda);

        Assert.Throws<InvalidOperationException>(() => agenda.BloquearDia(Segunda, "De novo", Segunda));
    }

    [Fact]
    public void Dado_MotivoLongo_Quando_BloquearDia_Entao_DeveLancarArgumentException()
    {
        var agenda = new AgendaDeRetirada(60);

        Assert.Throws<ArgumentException>(() => agenda.BloquearDia(Segunda, new string('a', 101), Segunda));
    }

    [Fact]
    public void Dado_DiaBloqueado_Quando_DesbloquearDia_Entao_DeveRemover()
    {
        var agenda = new AgendaDeRetirada(60);
        agenda.BloquearDia(Segunda, "Feriado", Segunda);

        agenda.DesbloquearDia(Segunda);

        Assert.Empty(agenda.DiasBloqueados);
    }
}
