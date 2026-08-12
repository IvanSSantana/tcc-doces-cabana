using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
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

        // A entidade não navega até Usuario (RQ-02), mas o relacionamento
        // existe no banco: declarado aqui, só do lado da infraestrutura.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
