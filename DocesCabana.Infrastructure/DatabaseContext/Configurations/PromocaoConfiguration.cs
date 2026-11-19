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

        builder.Property(p => p.Valor)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.DataInicio)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(p => p.DataFim)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(p => p.Ativa)
            .IsRequired();

        builder.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Subcategoria)
            .WithMany()
            .HasForeignKey(p => p.SubcategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Produtos)
            .WithMany()
            .UsingEntity("PromocaoProduto");

        builder.Navigation(p => p.Produtos)
            .HasField("_produtos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
