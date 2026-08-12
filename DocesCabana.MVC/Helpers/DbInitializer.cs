using System;
using System.Linq;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.MVC.Helpers;

public static class DbInitializer
{
    // Identificadores fixos (não Guid.NewGuid()), para que testes e E2E
    // possam referenciar uma categoria/subcategoria conhecida.
    public static readonly Guid CategoriaSalgadosId = new("11111111-0000-0000-0000-000000000001");
    public static readonly Guid CategoriaDocesId = new("11111111-0000-0000-0000-000000000002");
    public static readonly Guid CategoriaAdegaId = new("11111111-0000-0000-0000-000000000003");

    public static readonly Guid SubcategoriaSalgadosAssadosId = new("22222222-0000-0000-0000-000000000001");
    public static readonly Guid SubcategoriaSalgadosFritosId = new("22222222-0000-0000-0000-000000000002");
    public static readonly Guid SubcategoriaDocesDeTachoId = new("22222222-0000-0000-0000-000000000003");
    public static readonly Guid SubcategoriaDocesCaseirosId = new("22222222-0000-0000-0000-000000000004");
    public static readonly Guid SubcategoriaVinhosId = new("22222222-0000-0000-0000-000000000005");
    public static readonly Guid SubcategoriaDestiladosId = new("22222222-0000-0000-0000-000000000006");

    public static void Migrar(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        context.Database.Migrate();
    }

    public static void Semear(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        // Se já houver produtos, não faz nada
        if (context.Produtos.Any())
        {
            return;
        }

        // Ordem obrigatória: categoria -> subcategoria -> produto. A FK de
        // Produto.SubcategoriaId é enforçada desde a spec 003; semear fora
        // dessa ordem falha.
        var categorias = new[]
        {
            new Categoria("Salgados", CategoriaSalgadosId),
            new Categoria("Doces", CategoriaDocesId),
            new Categoria("Adega", CategoriaAdegaId),
        };
        context.Categorias.AddRange(categorias);

        var subcategorias = new[]
        {
            new Subcategoria(CategoriaSalgadosId, "Salgados Assados", SubcategoriaSalgadosAssadosId),
            new Subcategoria(CategoriaSalgadosId, "Salgados Fritos", SubcategoriaSalgadosFritosId),
            new Subcategoria(CategoriaDocesId, "Doces de Tacho", SubcategoriaDocesDeTachoId),
            new Subcategoria(CategoriaDocesId, "Doces Caseiros", SubcategoriaDocesCaseirosId),
            new Subcategoria(CategoriaAdegaId, "Vinhos", SubcategoriaVinhosId),
            new Subcategoria(CategoriaAdegaId, "Destilados", SubcategoriaDestiladosId),
        };
        context.Subcategorias.AddRange(subcategorias);

        var produtosSeed = new[]
        {
            new Produto(SubcategoriaDocesDeTachoId, "Raspa Tacho", 19.99m, "https://drive.google.com/file/d/1q2pScc0aQL8V8w3PeffOQsAfo6_-YxYk/preview"),
            new Produto(SubcategoriaDocesDeTachoId, "Pé de Moleque", 25.00m, "https://drive.google.com/file/d/1nqCmg7DPQQhUhFKQ12b21XMQSVTYWSuT/preview"),
            new Produto(SubcategoriaDocesDeTachoId, "Pé de Moça", 27.00m, "https://drive.google.com/file/d/1YfVBWgDdQ4XVB1tsSY7yDOssljtJlIuZ/preview"),
            new Produto(SubcategoriaDocesDeTachoId, "Doce de Leite", 15.99m, "https://drive.google.com/file/d/1jFKyz7UdjlYL6gRJbzi2N4Pm3IsIKrZ4/preview"),
            new Produto(SubcategoriaDocesDeTachoId, "Raspa Tacho", 19.99m, "https://drive.google.com/file/d/1Hq0GQ6axWc-iRPOheT4vBYa0s6MU-q6C/preview"),
            new Produto(SubcategoriaDocesDeTachoId, "Pé de Moleque", 25.00m, "https://drive.google.com/file/d/1bfDl0VMyHkHzxOxluuho3-7EERjjdDa2/preview"),
        };
        context.Produtos.AddRange(produtosSeed);

        context.SaveChanges();
    }
}
