using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");

        // A chave é o próprio UsuarioId (1:1 por chave compartilhada com
        // ContaDeAcesso) — Usuario é o dependente, ContaDeAcesso a principal.
        builder.HasKey(u => u.UsuarioId);

        builder.Property(u => u.Nome)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.CPF)
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(u => u.Celular)
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(u => u.DataNascimento)
            .HasColumnType("date");

        builder.HasIndex(u => u.CPF)
            .IsUnique();

        builder.HasOne<ContaDeAcesso>()
            .WithOne(c => c.Usuario)
            .HasForeignKey<Usuario>(u => u.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
