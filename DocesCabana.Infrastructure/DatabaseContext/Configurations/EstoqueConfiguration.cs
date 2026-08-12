using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoque");

        // Chave compartilhada: o ProdutoId é ao mesmo tempo PK de Estoque e FK
        // para Produto, garantindo o 1:1 da RN-04.
        builder.HasKey(e => e.ProdutoId);

        builder.Property(e => e.Quantidade)
            .IsRequired();

        builder.HasOne(e => e.Produto)
            .WithOne()
            .HasForeignKey<Estoque>(e => e.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
