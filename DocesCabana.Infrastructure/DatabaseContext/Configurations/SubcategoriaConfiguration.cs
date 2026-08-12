using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class SubcategoriaConfiguration : IEntityTypeConfiguration<Subcategoria>
{
    public void Configure(EntityTypeBuilder<Subcategoria> builder)
    {
        builder.ToTable("Subcategoria");

        builder.HasKey(s => s.SubcategoriaId);

        builder.Property(s => s.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.CategoriaId)
            .IsRequired();

        builder.HasOne(s => s.Categoria)
            .WithMany()
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
