using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produto");

        builder.HasKey(p => p.ProdutoId);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Preco)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasColumnType("INTEGER");

        builder.Property(p => p.ImagemUrl)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(p => p.SubcategoriaId)
            .IsRequired();
            
        builder.Property(p => p.PromocaoId);
    }
}
