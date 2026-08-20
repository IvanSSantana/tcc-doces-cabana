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
        builder.Property(a => a.DataCriacao).IsRequired();

        builder.HasOne(a => a.Produto)
            .WithMany()
            .HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // RF-15/RN-01 (spec 014): uma pessoa avalia um mesmo produto no
        // máximo uma vez. É a barreira de dado — não há barreira de entrada
        // porque a tela de escrever avaliação ainda não existe (plano §10).
        builder.HasIndex(a => new { a.UsuarioId, a.ProdutoId }).IsUnique();
    }
}
