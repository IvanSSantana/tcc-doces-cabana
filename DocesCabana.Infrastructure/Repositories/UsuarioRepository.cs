using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(DocesCabanaDbContext context)
        : base(context)
    {
    }

    public async Task<Usuario?> BuscarPorCpf(string cpf) =>
        await _context.Set<Usuario>().FirstOrDefaultAsync(u => u.CPF == cpf);
}
