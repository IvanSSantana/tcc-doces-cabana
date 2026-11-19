using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItemPedido");

        builder.HasKey(i => i.ItemPedidoId);

        builder.Property(i => i.Quantidade).IsRequired();

        builder.Property(i => i.PrecoUnitario)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.DescontoDaPromocao)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0m);

        builder.Property(i => i.RotuloDaPromocao)
            .HasMaxLength(100);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
