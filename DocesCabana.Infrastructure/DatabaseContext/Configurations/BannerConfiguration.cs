using DocesCabana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banner", tabela => tabela.HasCheckConstraint(
            "CK_Banner_UmDestino",
            """(("CategoriaId" IS NOT NULL)::int + ("SubcategoriaId" IS NOT NULL)::int + ("ProdutoId" IS NOT NULL)::int) = 1"""));

        builder.HasKey(b => b.BannerId);

        builder.Property(b => b.ImagemUrl)
            .IsRequired()
            .HasMaxLength(Banner.TamanhoMaximoDaImagem);

        builder.Property(b => b.TextoAlternativo)
            .IsRequired()
            .HasMaxLength(Banner.TamanhoMaximoDoTextoAlternativo);

        builder.Property(b => b.Ordem)
            .IsRequired();

        builder.Property(b => b.DataInicio)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(b => b.DataFim)
            .HasColumnType("date");

        builder.Property(b => b.Ativo)
            .IsRequired();

        builder.Ignore(b => b.TipoDeDestino);

        builder.HasIndex(b => new { b.Ativo, b.DataInicio });

        builder.HasOne(b => b.Categoria)
            .WithMany()
            .HasForeignKey(b => b.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Subcategoria)
            .WithMany()
            .HasForeignKey(b => b.SubcategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Produto)
            .WithMany()
            .HasForeignKey(b => b.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
