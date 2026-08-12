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

    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Subcategoria> Subcategorias { get; set; }

    public DbSet<Promocao> Promocoes { get; set; }

    public DbSet<Estoque> Estoques { get; set; }

    public DbSet<Endereco> Enderecos { get; set; }

    public DbSet<Favorito> Favoritos { get; set; }

    public DbSet<Avaliacao> Avaliacoes { get; set; }

    public DbSet<Pedido> Pedidos { get; set; }

    public DbSet<ItemPedido> ItensPedido { get; set; }

    public DbSet<Pagamento> Pagamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
