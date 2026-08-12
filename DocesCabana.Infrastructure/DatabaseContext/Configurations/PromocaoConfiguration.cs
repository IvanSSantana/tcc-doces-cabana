using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class PromocaoConfiguration : IEntityTypeConfiguration<Promocao>
{
    public void Configure(EntityTypeBuilder<Promocao> builder)
    {
        builder.ToTable("Promocao");

        builder.HasKey(p => p.PromocaoId);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Descricao)
            .HasMaxLength(255);

        // Sem HasColumnType em Tipo: o provider mapeia o enum de byte sozinho.
        builder.Property(p => p.Valor)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.DataInicio)
            .IsRequired();

        builder.Property(p => p.DataFim)
            .IsRequired();

        builder.Property(p => p.Ativa)
            .IsRequired();
    }
}
