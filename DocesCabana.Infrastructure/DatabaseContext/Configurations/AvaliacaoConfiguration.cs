using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("Avaliacao");

        builder.HasKey(a => a.AvaliacaoId);

        builder.Property(a => a.Comentario).HasMaxLength(255);
        builder.Property(a => a.Nota).IsRequired();
        builder.Property(a => a.UpVote).IsRequired();

        builder.HasOne(a => a.Produto)
            .WithMany()
            .HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
