using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("Endereco");

        builder.HasKey(e => e.EnderecoId);

        builder.Property(e => e.Estado).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Cidade).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Bairro).IsRequired().HasMaxLength(255);
        builder.Property(e => e.CEP).IsRequired().HasMaxLength(8);
        builder.Property(e => e.Rua).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Numero).IsRequired();
        builder.Property(e => e.Complemento).HasMaxLength(100);

        builder.HasOne(e => e.Usuario)
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
