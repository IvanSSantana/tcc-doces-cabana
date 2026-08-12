using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class FavoritoConfiguration : IEntityTypeConfiguration<Favorito>
{
    public void Configure(EntityTypeBuilder<Favorito> builder)
    {
        builder.ToTable("Favorito");

        // Chave primária composta: é o que impede o mesmo par
        // (produto, usuário) de ser favoritado duas vezes (RN-15).
        builder.HasKey(f => new { f.ProdutoId, f.UsuarioId });

        builder.HasOne(f => f.Produto)
            .WithMany()
            .HasForeignKey(f => f.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
