using System.Reflection;
using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.DatabaseContext;

public class DocesCabanaDbContext : DbContext    
{   
    public DocesCabanaDbContext(DbContextOptions<DocesCabanaDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
