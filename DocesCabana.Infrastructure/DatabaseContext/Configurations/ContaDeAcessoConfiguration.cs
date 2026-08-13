using DocesCabana.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocesCabana.Infrastructure.DatabaseContext.Configurations;

public class ContaDeAcessoConfiguration : IEntityTypeConfiguration<ContaDeAcesso>
{
    public void Configure(EntityTypeBuilder<ContaDeAcesso> builder)
    {
        builder.ToTable("ContaDeAcesso");
    }
}
