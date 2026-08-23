using DocesCabana.Application.DependencyInjections;
using DocesCabana.Infrastructure.DependencyInjections;
using DocesCabana.MVC.Filters;
using DocesCabana.MVC.Helpers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<FilterException>();
    // Funde o carrinho de visitante ao de conta no primeiro request
    // autenticado (spec 017, Fase 7) — global porque o login não redireciona
    // necessariamente para /Carrinho.
    options.Filters.Add<FiltroFusaoDeCarrinho>();
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, propertyName) =>
    {
        if (propertyName == "DataNascimento" || propertyName == "Data de Nascimento")
        {
            return "Data de nascimento inválida.";
        }
        return $"O valor '{value}' é inválido.";
    });
});

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddApplicationServicesAndRepositories(builder.Configuration);
builder.Services.AddFluentValidationConfiguration();

// Carrinho do visitante (spec 017, Fase 6) — em memória, por processo; some
// ao reiniciar a aplicação, consequência aceita e registrada na spec §10.
builder.Services.AddSession();

var app = builder.Build();

DbInitializer.Migrar(app.Services);

// Corrige, em toda base (inclusive produção), a coluna que a migration
// AddProdutoNomeNormalizado deixou vazia em linhas gravadas antes dela
// (spec 016, plano §6) — não é dado de demonstração, então não fica atrás
// do mesmo gate de "fora de produção" do seed abaixo.
await DbInitializer.PreencherNomesNormalizados(app.Services);

// Massa inicial de dados só fora de produção
if (!app.Environment.IsProduction())
    await DbInitializer.Semear(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Qualquer 404 (produto inexistente ou inativo, rota sem correspondência)
// reexecuta em /Home/NaoEncontrado — CA-04/CA-05 da spec 008. O NotFoundResult
// que o FilterException devolve para KeyNotFoundException é justamente o
// gatilho: sem isso, o visitante veria um 404 em branco do servidor.
app.UseStatusCodePagesWithReExecute("/Home/NaoEncontrado");

app.UseRouting();

// Precisa vir depois de UseRouting e antes de UseAuthentication (plano da
// 017, §9, risco 1): lida antes de o middleware rodar, a sessão devolve
// vazio sem erro nenhum — o carrinho do visitante pareceria sempre vazio,
// em silêncio.
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[] { "pt-BR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.MapStaticAssets();

// Rota de area — precisa vir antes da padrão, senão "/Admin/Produto" seria
// interpretado pela padrão como controller "Admin", ação "Produto" (spec 011).
app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// O segundo segmento é o apelido da categoria, não uma ação (spec 012,
// RF-02) — sem esta rota, a padrão abaixo interpretaria "/Catalogo/doces"
// como controller "Catalogo", ação "doces".
app.MapControllerRoute(
    name: "catalogo",
    pattern: "Catalogo/{apelido?}",
    defaults: new { controller = "Catalogo", action = "Index" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
