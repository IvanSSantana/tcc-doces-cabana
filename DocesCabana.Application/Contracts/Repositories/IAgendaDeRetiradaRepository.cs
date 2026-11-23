using DocesCabana.Domain.Contracts;
using DocesCabana.Domain.Entities;

namespace DocesCabana.Application.Contracts.Repositories;

public interface IAgendaDeRetiradaRepository : IRepository<AgendaDeRetirada>
{
    /// <summary>A agenda única, rastreada, com horários e dias bloqueados.</summary>
    Task<AgendaDeRetirada> Obter();
}
