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

    public async Task<T?> BuscarPorId(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> BuscarTodos()
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task Adicionar(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task<T> Atualizar(T entity)
    {
        _context.Set<T>().Update(entity);
        return entity;
    }

    public void Remover(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}
