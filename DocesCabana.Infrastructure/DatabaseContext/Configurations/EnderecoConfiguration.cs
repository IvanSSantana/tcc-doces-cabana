using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("Endereco");

        builder.HasKey(e => e.EnderecoId);

        builder.Property(e => e.Estado).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Cidade).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Bairro).IsRequired().HasMaxLength(255);
        builder.Property(e => e.CEP).IsRequired().HasMaxLength(8);
        builder.Property(e => e.Rua).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Numero).IsRequired();
        builder.Property(e => e.Complemento).HasMaxLength(100);

        // Spec 018: Padrao é o que a RN-01 exige; DataCadastro dá à lista
        // uma ordem estável e à RN-04 um critério de promoção.
        builder.Property(e => e.Padrao).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.DataCadastro).IsRequired();

        builder.HasOne(e => e.Usuario)
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
