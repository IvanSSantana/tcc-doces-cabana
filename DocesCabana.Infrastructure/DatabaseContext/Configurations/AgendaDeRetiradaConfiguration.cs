using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class AgendaDeRetiradaConfiguration : IEntityTypeConfiguration<AgendaDeRetirada>
{
    public void Configure(EntityTypeBuilder<AgendaDeRetirada> builder)
    {
        builder.ToTable("AgendaDeRetirada");

        builder.HasKey(a => a.AgendaDeRetiradaId);

        builder.Property(a => a.DuracaoDaFaixaEmMinutos).IsRequired();
        builder.Property(a => a.PreparoEmMinutos).IsRequired();

        builder.OwnsMany(a => a.Horarios, horario =>
        {
            horario.ToTable("HorarioDeRetirada");
            horario.WithOwner().HasForeignKey("AgendaDeRetiradaId");
            horario.HasKey("AgendaDeRetiradaId", nameof(HorarioDeRetirada.DiaDaSemana));
            horario.Property(h => h.Abertura);
            horario.Property(h => h.Fechamento);
            horario.Ignore(h => h.Aberto);
        });

        builder.Navigation(a => a.Horarios)
            .HasField("_horarios")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(a => a.DiasBloqueados, dia =>
        {
            dia.ToTable("DiaSemRetirada");
            dia.WithOwner().HasForeignKey("AgendaDeRetiradaId");
            dia.HasKey("AgendaDeRetiradaId", nameof(DiaSemRetirada.Data));
            dia.Property(d => d.Motivo).HasMaxLength(DiaSemRetirada.TamanhoMaximoDoMotivo);
        });

        builder.Navigation(a => a.DiasBloqueados)
            .HasField("_diasBloqueados")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
