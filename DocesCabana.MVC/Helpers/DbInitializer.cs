using System;
using System.Linq;
using System.Threading.Tasks;
using DocesCabana.Domain.Entities;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    public const string PapelAdministrador = "Administrador";
    public const string EmailAdministrador = "admin@docescabana.com.br";

    public static void Migrar(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        context.Database.Migrate();
    }

    public static async Task Semear(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        // Se já houver produtos, não faz nada
        if (!context.Produtos.Any())
        {
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

            await context.SaveChangesAsync();
        }

        await SemearAdministrador(scope.ServiceProvider);
    }

    private static async Task SemearAdministrador(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ContaDeAcesso>>();
        var context = serviceProvider.GetRequiredService<DocesCabanaDbContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        if (!await roleManager.RoleExistsAsync(PapelAdministrador))
            await roleManager.CreateAsync(new IdentityRole<Guid>(PapelAdministrador));

        if (await userManager.FindByEmailAsync(EmailAdministrador) is not null)
            return;

        // A senha do administrador semeado vem de user secret, nunca literal
        // no código. Sem ela configurada, nenhum admin é criado — a aplicação
        // sobe do mesmo jeito, só sem conta administrativa pronta.
        var senha = configuration["Admin:SenhaInicial"];
        if (string.IsNullOrWhiteSpace(senha))
            return;

        var conta = new ContaDeAcesso(EmailAdministrador);
        var resultado = await userManager.CreateAsync(conta, senha);
        if (!resultado.Succeeded)
            return;

        var administrador = new Usuario(
            conta.Id,
            "Administrador Doces Cabana",
            "52998224725",
            "14999999999",
            new DateTime(1990, 1, 1));

        context.Usuarios.Add(administrador);
        await context.SaveChangesAsync();

        await userManager.AddToRoleAsync(conta, PapelAdministrador);
    }
}
