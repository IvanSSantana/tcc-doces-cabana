using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedido");

        builder.HasKey(p => p.PedidoId);

        builder.Property(p => p.Valor)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Dados da entrega, congelados no fechamento (spec 022, RN-01).
        builder.Property(p => p.ValorDoFrete)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.Transportadora).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Servico).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PrazoMinimoEmDias).IsRequired();
        builder.Property(p => p.PrazoMaximoEmDias).IsRequired();

        builder.Property(p => p.PagamentoAprovado).IsRequired();
        builder.Property(p => p.Data).IsRequired();

        // Pedido é a raiz do agregado (spec 022, plano §3) — a coleção é
        // exposta só como IReadOnlyCollection, mapeada pelo campo de apoio
        // privado (suportado pelo EF Core por convenção de nome: `_itens`
        // para a propriedade `Itens`).
        builder.HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.EnderecoEntrega)
            .WithMany()
            .HasForeignKey(p => p.EnderecoEntregaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Usuario)
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
