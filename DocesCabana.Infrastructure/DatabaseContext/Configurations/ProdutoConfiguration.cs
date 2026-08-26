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

        // Derivado de Nome (spec 016) — sem acento, sem caixa. Linhas
        // gravadas antes desta migration nascem com '' (plano §6); é o
        // DbInitializer.PreencherNomesNormalizados que as corrige, não o
        // banco: SQLite não tem função para remover acento.
        builder.Property(p => p.NomeNormalizado)
            .IsRequired()
            .HasMaxLength(255)
            .HasDefaultValue("");

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

        builder.Property(p => p.Descricao)
            .HasMaxLength(4000);

        builder.Property(p => p.SemAcucar)
            .IsRequired()
            .HasDefaultValue(false);

        // Peso e dimensões (spec 020, RF-01/RN-01) — decimal(10,3): três
        // casas bastam para grama e milímetro, e sobra faixa para qualquer
        // produto físico da loja.
        builder.Property(p => p.Peso)
            .IsRequired()
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.Altura)
            .IsRequired()
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.Largura)
            .IsRequired()
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.Comprimento)
            .IsRequired()
            .HasColumnType("decimal(10,3)");


        builder.Property(p => p.SubcategoriaId)
            .IsRequired();

        builder.Property(p => p.PromocaoId);

        builder.HasOne(p => p.Subcategoria)
            .WithMany()
            .HasForeignKey(p => p.SubcategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Promocao)
            .WithMany()
            .HasForeignKey(p => p.PromocaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
