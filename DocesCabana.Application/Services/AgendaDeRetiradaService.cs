using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Helpers;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Contracts;

namespace DocesCabana.Application.Services;

public class AgendaDeRetiradaService : IAgendaDeRetiradaService
{
    private readonly IAgendaDeRetiradaRepository _agendaDeRetiradaRepository;
    private readonly TimeProvider _relogio;
    private readonly IUnitOfWork _unitOfWork;

    public AgendaDeRetiradaService(
        IAgendaDeRetiradaRepository agendaDeRetiradaRepository, TimeProvider relogio, IUnitOfWork unitOfWork)
    {
        _agendaDeRetiradaRepository = agendaDeRetiradaRepository;
        _relogio = relogio;
        _unitOfWork = unitOfWork;
    }

    public async Task<RetiradaDisponivelDTO> ConsultarHoje()
    {
        var agenda = await _agendaDeRetiradaRepository.Obter();
        var agora = HorarioDeBrasilia.Agora(_relogio);

        var faixas = agenda.FaixasDisponiveis(agora);

        return new RetiradaDisponivelDTO
        {
            Data = DateOnly.FromDateTime(agora),
            Faixas = faixas.Select(AgendaDeRetiradaMapper.ToDTO).ToList(),
            ProximaAbertura = faixas.Count == 0 ? agenda.ProximaAbertura(agora) : null
        };
    }

    public async Task<AgendaDeRetiradaDTO> ObterParaGestao() =>
        AgendaDeRetiradaMapper.ToDTO(await _agendaDeRetiradaRepository.Obter());

    public async Task SalvarHorarios(AgendaDeRetiradaDTO dto)
    {
        var agenda = await _agendaDeRetiradaRepository.Obter();

        agenda.AlterarFaixas(dto.DuracaoDaFaixaEmMinutos!.Value, dto.PreparoEmMinutos!.Value);

        foreach (var horario in dto.Horarios)
        {
            if (horario.Aberto)
                agenda.DefinirHorario(horario.DiaDaSemana, horario.Abertura, horario.Fechamento);
            else
                agenda.DefinirHorario(horario.DiaDaSemana, null, null);
        }

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task<bool> DiaJaBloqueado(DateOnly data)
    {
        var agenda = await _agendaDeRetiradaRepository.Obter();
        return agenda.DiasBloqueados.Any(d => d.Data == data);
    }

    public async Task BloquearDia(DiaSemRetiradaDTO dto)
    {
        var agenda = await _agendaDeRetiradaRepository.Obter();

        agenda.BloquearDia(dto.Data!.Value, dto.Motivo, HorarioDeBrasilia.Hoje(_relogio));

        await _unitOfWork.SalvarAlteracoes();
    }

    public async Task DesbloquearDia(DateOnly data)
    {
        var agenda = await _agendaDeRetiradaRepository.Obter();

        agenda.DesbloquearDia(data);

        await _unitOfWork.SalvarAlteracoes();
    }
}
