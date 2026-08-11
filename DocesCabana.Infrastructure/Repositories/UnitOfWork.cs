using DocesCabana.Domain.Contracts;
using DocesCabana.Infrastructure.DatabaseContext;

namespace DocesCabana.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DocesCabanaDbContext _context;

    public UnitOfWork(DocesCabanaDbContext context)
    {
        _context = context;
    }

    public Task<int> SalvarAlteracoes(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
