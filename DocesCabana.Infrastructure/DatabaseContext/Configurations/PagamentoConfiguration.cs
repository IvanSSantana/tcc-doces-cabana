using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamento");

        builder.HasKey(p => p.PagamentoId);

        builder.Property(p => p.Valor)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // 1:1 com Pedido: um pedido tem no máximo um pagamento (RN-23).
        builder.HasOne(p => p.Pedido)
            .WithOne()
            .HasForeignKey<Pagamento>(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
