using System.Reflection;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocesCabana.Infrastructure.DatabaseContext;

public class DocesCabanaDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>    
{   
    public DocesCabanaDbContext(DbContextOptions<DocesCabanaDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
