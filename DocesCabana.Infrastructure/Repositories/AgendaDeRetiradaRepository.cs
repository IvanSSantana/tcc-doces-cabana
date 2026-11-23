using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class AgendaDeRetiradaRepository : Repository<AgendaDeRetirada>, IAgendaDeRetiradaRepository
{
    public AgendaDeRetiradaRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Horários e dias bloqueados são tipos próprios (owned) e vêm junto sem Include.
    /// </summary>
    public async Task<AgendaDeRetirada> Obter() =>
        await _context.AgendasDeRetirada.FirstOrDefaultAsync(a => a.AgendaDeRetiradaId == AgendaDeRetirada.IdUnico)
            ?? throw new InvalidOperationException(
                "A agenda de retirada não foi semeada. Reinicie a aplicação para o DbInitializer criá-la.");
}
