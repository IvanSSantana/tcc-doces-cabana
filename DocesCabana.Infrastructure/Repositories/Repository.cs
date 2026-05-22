using DocesCabana.Domain.Contracts;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly DocesCabanaDbContext _context;

    public Repository(DocesCabanaDbContext context)
    {
        _context = context;
    }

    public async Task<T?> BuscarPorIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> BuscarTodosAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task AdicionarAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public void Atualizar(T entity)
    {
        _context.Set<T>().Update(entity);

        _context.SaveChanges();
    }

    public void Remover(T entity)
    {
        _context.Set<T>().Remove(entity);

        _context.SaveChanges();
    }
}
