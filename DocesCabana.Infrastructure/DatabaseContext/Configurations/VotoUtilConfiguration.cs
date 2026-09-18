using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class VotoUtilConfiguration : IEntityTypeConfiguration<VotoUtil>
{
    public void Configure(EntityTypeBuilder<VotoUtil> builder)
    {
        builder.ToTable("VotoUtil");

        builder.HasKey(v => new { v.AvaliacaoId, v.UsuarioId });

        builder.HasOne(v => v.Avaliacao)
            .WithMany(a => a.Votos)
            .HasForeignKey(v => v.AvaliacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
