using DocesCabana.Infrastructure.DatabaseContext.Seed;
using DocesCabana.Application.DependencyInjections;
using DocesCabana.Infrastructure.DependencyInjections;
using DocesCabana.MVC.Filters;
using FluentValidation.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<FilterException>();
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
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddSession();

var app = builder.Build();

DbInitializer.Migrar(app.Services);

await DbInitializer.PreencherNomesNormalizados(app.Services);

if (!app.Environment.IsProduction())
    await DbInitializer.Semear(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStatusCodePagesWithReExecute("/Home/NaoEncontrado");

app.UseRouting();

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

app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

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
