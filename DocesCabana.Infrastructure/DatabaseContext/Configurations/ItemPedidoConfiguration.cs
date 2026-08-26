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

        // A relação com Pedido é configurada do lado de Pedido
        // (PedidoConfiguration.HasMany), que é a raiz do agregado.

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
