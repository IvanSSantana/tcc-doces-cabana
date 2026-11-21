using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class CupomConfiguration : IEntityTypeConfiguration<Cupom>
{
    public void Configure(EntityTypeBuilder<Cupom> builder)
    {
        builder.ToTable("Cupom");

        builder.HasKey(c => c.CupomId);

        builder.Property(c => c.Codigo)
            .IsRequired()
            .HasMaxLength(Cupom.TamanhoMaximoDoCodigo);

        builder.HasIndex(c => c.Codigo)
            .IsUnique();

        builder.Property(c => c.Descricao)
            .HasMaxLength(255);

        builder.Property(c => c.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(c => c.ValorMinimo)
            .HasColumnType("decimal(10,2)");

        builder.Property(c => c.DataInicio)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(c => c.DataFim)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(c => c.UmUsoPorCliente)
            .IsRequired();

        builder.Property(c => c.Ativo)
            .IsRequired();

        builder.HasOne(c => c.Categoria)
            .WithMany()
            .HasForeignKey(c => c.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Subcategoria)
            .WithMany()
            .HasForeignKey(c => c.SubcategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
