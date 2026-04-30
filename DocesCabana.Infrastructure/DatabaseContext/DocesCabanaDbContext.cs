using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.DatabaseContext;

// A revisar
public class DocesCabanaDbContext : DbContext    
{   
    public DocesCabanaDbContext(DbContextOptions<DocesCabanaDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
}
