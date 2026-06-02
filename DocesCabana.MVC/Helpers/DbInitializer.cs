using System;
using System.Linq;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.MVC.Helpers;

public static class DbInitializer
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        // Certifica de aplicar migrações pendentes se houver
        context.Database.Migrate();

        // Se já houver produtos, não faz nada
        if (context.Produtos.Any())
        {
            return;
        }

        var subcategoriaId = Guid.NewGuid(); // Subcategoria simulada

        var produtosSeed = new[]
        {
            new Produto(subcategoriaId, "Raspa Tacho", 19.99m, "https://drive.google.com/file/d/1q2pScc0aQL8V8w3PeffOQsAfo6_-YxYk/preview"),
            new Produto(subcategoriaId, "Pé de Moleque", 25.00m, "https://drive.google.com/file/d/1nqCmg7DPQQhUhFKQ12b21XMQSVTYWSuT/preview"),
            new Produto(subcategoriaId, "Pé de Moça", 27.00m, "https://drive.google.com/file/d/1YfVBWgDdQ4XVB1tsSY7yDOssljtJlIuZ/preview"),
            new Produto(subcategoriaId, "Doce de Leite", 15.99m, "https://drive.google.com/file/d/1jFKyz7UdjlYL6gRJbzi2N4Pm3IsIKrZ4/preview"),
            new Produto(subcategoriaId, "Raspa Tacho", 19.99m, "https://drive.google.com/file/d/1Hq0GQ6axWc-iRPOheT4vBYa0s6MU-q6C/preview"),
            new Produto(subcategoriaId, "Pé de Moleque", 25.00m, "https://drive.google.com/file/d/1bfDl0VMyHkHzxOxluuho3-7EERjjdDa2/preview"),
        };

        context.Produtos.AddRange(produtosSeed);
        context.SaveChanges();
    }
}
