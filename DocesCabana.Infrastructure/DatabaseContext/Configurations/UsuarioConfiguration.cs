using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");

        builder.HasKey(u => u.UsuarioId);

        builder.Property(u => u.Nome)
            .HasColumnName("NomeCompleto")
            .HasMaxLength(255);

        builder.Property(u => u.Email)
            .HasMaxLength(255);

        builder.Property(u => u.Celular)
            .HasMaxLength(20);

        builder.Property(u => u.DataNascimento)
            .HasColumnType("date");

        builder.Property(u => u.CPF)
            .HasMaxLength(11);

        builder.Property(u => u.Senha)
            .HasMaxLength(255);
            
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.CPF).IsUnique();
    }
}
