using DocesCabana.Infrastructure.Identity;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.Infrastructure.DependencyInjections;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ContaDeAcesso, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<DocesCabanaDbContext>()
        .AddDefaultTokenProviders();

        // Admin desativado com sessão aberta perde o acesso em até 1 minuto, não nos 30 do padrão (spec 039, D5).
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(1));

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Autenticacao/Login";
            options.LogoutPath = "/Autenticacao/Logout";
            options.AccessDeniedPath = "/Home/AcessoNegado";
        });

        return services;
    }
}
