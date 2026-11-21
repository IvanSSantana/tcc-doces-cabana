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

        builder.Property(p => p.ValorDoFrete)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.Transportadora).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Servico).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PrazoMinimoEmDias).IsRequired();
        builder.Property(p => p.PrazoMaximoEmDias).IsRequired();

        builder.Property(p => p.PagamentoAprovado).IsRequired();
        builder.Property(p => p.Data).IsRequired();

        builder.Property(p => p.CodigoDoCupom)
            .HasMaxLength(Cupom.TamanhoMaximoDoCodigo);

        builder.Property(p => p.DescontoDoCupom)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0m);

        builder.Property(p => p.DescontoNoFrete)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0m);

        builder.HasOne(p => p.Cupom)
            .WithMany()
            .HasForeignKey(p => p.CupomId)
            .OnDelete(DeleteBehavior.Restrict);

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
