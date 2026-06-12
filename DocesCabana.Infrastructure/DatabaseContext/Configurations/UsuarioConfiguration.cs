public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");

        builder.Property(u => u.Nome)
            .HasColumnName("NomeCompleto")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.CPF)
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(u => u.DataNascimento)
            .HasColumnType("date");

        builder.HasIndex(u => u.CPF)
            .IsUnique();
    }
}