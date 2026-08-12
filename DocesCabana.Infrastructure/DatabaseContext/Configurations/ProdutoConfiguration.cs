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

        // Sem HasColumnType: ProdutoStatus é enum de byte, e o provider mapeia
        // sozinho para o tipo inteiro nativo em qualquer banco (INTEGER no
        // SQLite, tinyint no SQL Server). Fixar "INTEGER" aqui quebraria a
        // troca de provider planejada para o deploy.
        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.ImagemUrl)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(p => p.SubcategoriaId)
            .IsRequired();

        builder.Property(p => p.PromocaoId);

        builder.HasOne(p => p.Subcategoria)
            .WithMany()
            .HasForeignKey(p => p.SubcategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
