using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class VotoUtilConfiguration : IEntityTypeConfiguration<VotoUtil>
{
    public void Configure(EntityTypeBuilder<VotoUtil> builder)
    {
        builder.ToTable("VotoUtil");

        // Chave composta: uma pessoa vota no máximo uma vez por avaliação
        // (RN-06), garantido no banco, não só no código.
        builder.HasKey(v => new { v.AvaliacaoId, v.UsuarioId });

        builder.HasOne(v => v.Avaliacao)
            .WithMany(a => a.Votos)
            .HasForeignKey(v => v.AvaliacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: apagar um usuário não pode arrastar votos de avaliações
        // de outras pessoas em cascata por engano.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
