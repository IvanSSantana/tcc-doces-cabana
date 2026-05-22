using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.Infrastructure.DependencyInjections;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<Usuario, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
        })
        .AddEntityFrameworkStores<DocesCabanaDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
