using DocesCabana.Application.DependencyInjections;
using DocesCabana.Infrastructure.DependencyInjections;
using DocesCabana.MVC.Filters;
using DocesCabana.MVC.Helpers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<FilterException>();
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

var app = builder.Build();

// Seeding the database
DocesCabana.MVC.Helpers.DbInitializer.Seed(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
