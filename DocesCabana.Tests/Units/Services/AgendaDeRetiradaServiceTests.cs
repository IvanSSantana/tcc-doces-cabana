using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;
using Moq;

namespace DocesCabana.Tests.Units.Services;

public class AgendaDeRetiradaServiceTests
{
    // 23/11/2026 é segunda-feira.
    private static readonly DateOnly Segunda = new(2026, 11, 23);

    private readonly Mock<IAgendaDeRetiradaRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<TimeProvider> _relogioMock = new();
    private readonly AgendaDeRetirada _agenda;
    private readonly AgendaDeRetiradaService _service;

    public AgendaDeRetiradaServiceTests()
    {
        _agenda = new AgendaDeRetirada(60, 60);
        foreach (var dia in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
            _agenda.DefinirHorario(dia, new TimeOnly(9, 0), new TimeOnly(18, 0));

        _repositoryMock.Setup(r => r.Obter()).ReturnsAsync(_agenda);
        RelogioEmBrasilia(Segunda, 10, 20);

        _service = new AgendaDeRetiradaService(_repositoryMock.Object, _relogioMock.Object, _unitOfWorkMock.Object);
    }

    private void RelogioEmBrasilia(DateOnly data, int hora, int minuto = 0) =>
        _relogioMock.Setup(r => r.GetUtcNow())
            .Returns(new DateTimeOffset(data.ToDateTime(new TimeOnly(hora, minuto)), TimeSpan.FromHours(-3)).ToUniversalTime());

    [Fact]
    public async Task Dado_SegundaAs10h20_Quando_ConsultarHoje_Entao_DeveRetornarFaixasAPartirDas12h()
    {
        var retirada = await _service.ConsultarHoje();

        Assert.True(retirada.Disponivel);
        Assert.Equal(Segunda, retirada.Data);
        Assert.Equal(new FaixaDeRetiradaDTO(new TimeOnly(12, 0), new TimeOnly(13, 0)), retirada.Faixas[0]);
        Assert.Equal(6, retirada.Faixas.Count);
    }

    [Fact]
    public async Task Dado_Domingo_Quando_ConsultarHoje_Entao_DeveEstarIndisponivelComProximaAberturaNaSegunda()
    {
        RelogioEmBrasilia(Segunda.AddDays(-1), 10);

        var retirada = await _service.ConsultarHoje();

        Assert.False(retirada.Disponivel);
        Assert.Equal(Segunda.ToDateTime(new TimeOnly(9, 0)), retirada.ProximaAbertura);
    }

    [Fact]
    public async Task Dado_AgendaSalva_Quando_ObterParaGestao_Entao_DeveListarDeSegundaADomingo()
    {
        var dto = await _service.ObterParaGestao();

        Assert.Equal(60, dto.DuracaoDaFaixaEmMinutos);
        Assert.Equal(60, dto.PreparoEmMinutos);
        Assert.Equal(7, dto.Horarios.Count);
        Assert.Equal(DayOfWeek.Monday, dto.Horarios[0].DiaDaSemana);
        Assert.Equal(DayOfWeek.Sunday, dto.Horarios[^1].DiaDaSemana);
        Assert.False(dto.Horarios[^1].Aberto);
    }

    [Fact]
    public async Task Dado_NovoHorario_Quando_SalvarHorarios_Entao_DeveAlterarAgendaESalvar()
    {
        var dto = await _service.ObterParaGestao();
        dto.DuracaoDaFaixaEmMinutos = 30;
        dto.PreparoEmMinutos = 0;
        dto.Horarios[0].Abertura = new TimeOnly(10, 0);
        dto.Horarios[^1] = new HorarioDeRetiradaDTO
        {
            DiaDaSemana = DayOfWeek.Sunday, Aberto = true, Abertura = new TimeOnly(8, 0), Fechamento = new TimeOnly(12, 0)
        };

        await _service.SalvarHorarios(dto);

        Assert.Equal(30, _agenda.DuracaoDaFaixaEmMinutos);
        Assert.Equal(0, _agenda.PreparoEmMinutos);
        Assert.Equal(new TimeOnly(10, 0), _agenda.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Monday).Abertura);
        Assert.True(_agenda.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Sunday).Aberto);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_DiaMarcadoFechadoComHorariosPreenchidos_Quando_SalvarHorarios_Entao_DeveFicarFechado()
    {
        var dto = await _service.ObterParaGestao();
        dto.Horarios[0].Aberto = false;

        await _service.SalvarHorarios(dto);

        Assert.False(_agenda.Horarios.Single(h => h.DiaDaSemana == DayOfWeek.Monday).Aberto);
    }

    [Fact]
    public async Task Dado_DataNova_Quando_BloquearDia_Entao_DeveBloquearESalvar()
    {
        await _service.BloquearDia(new DiaSemRetiradaDTO { Data = Segunda.AddDays(3), Motivo = "Feriado" });

        Assert.Contains(_agenda.DiasBloqueados, d => d.Data == Segunda.AddDays(3) && d.Motivo == "Feriado");
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dado_DataBloqueada_Quando_DiaJaBloqueado_Entao_DeveRetornarVerdadeiro()
    {
        _agenda.BloquearDia(Segunda, null, Segunda);

        Assert.True(await _service.DiaJaBloqueado(Segunda));
        Assert.False(await _service.DiaJaBloqueado(Segunda.AddDays(1)));
    }

    [Fact]
    public async Task Dado_DataBloqueada_Quando_DesbloquearDia_Entao_DeveRemoverESalvar()
    {
        _agenda.BloquearDia(Segunda, null, Segunda);

        await _service.DesbloquearDia(Segunda);

        Assert.Empty(_agenda.DiasBloqueados);
        _unitOfWorkMock.Verify(u => u.SalvarAlteracoes(It.IsAny<CancellationToken>()), Times.Once);
    }
}
