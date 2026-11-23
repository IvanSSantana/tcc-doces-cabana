using DocesCabana.Application.DTOs;

namespace DocesCabana.Application.Contracts.Services;

public interface IAgendaDeRetiradaService
{
    Task<RetiradaDisponivelDTO> ConsultarHoje();

    Task<AgendaDeRetiradaDTO> ObterParaGestao();

    Task SalvarHorarios(AgendaDeRetiradaDTO dto);

    Task<bool> DiaJaBloqueado(DateOnly data);

    Task BloquearDia(DiaSemRetiradaDTO dto);

    Task DesbloquearDia(DateOnly data);
}
