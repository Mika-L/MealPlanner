using System.Text.Json.Serialization;

using MealPlanner.Api;
using MealPlanner.Modules.Identity;
using MealPlanner.Modules.Meals;
using MealPlanner.SharedKernel.Identity;

using Scalar.AspNetCore;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog : lu depuis la configuration, enrichi par défaut
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Les enums (Season, MealStyle) sont sérialisées/désérialisées par leur nom, pas leur valeur numérique
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

// Utilisateur courant : lu depuis le JWT de la requête, exposé aux modules via ICurrentUser
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Modules (modular monolith) — chaque module se compose lui-même
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddMealsModule(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();

// SPA servi en same-origin : le front (Vite) est publié dans wwwroot et sert les fichiers statiques.
// En dev, wwwroot est vide (le front tourne sur Vite via proxy) : ces middlewares sont alors inertes.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Migrations appliquées au démarrage, dans tous les environnements. Hors dev, un échec fait échouer
// le démarrage (fail-fast) plutôt que de servir une application connectée à une base non migrée.
try
{
    await app.Services.InitializeIdentityModuleAsync();
    await app.Services.InitializeMealsModuleAsync();
}
catch (Exception exception) when (app.Environment.IsDevelopment())
{
    // En dev uniquement : l'API démarre même si la base est indisponible.
    // Hors dev, l'exception remonte et fait échouer le démarrage (fail-fast).
    app.Logger.LogWarning(exception, "Initialisation des modules impossible (base indisponible ?).");
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("System");

// Endpoints des modules
app.MapIdentityModule();
app.MapMealsModule();

// Routes côté client (react-router) : tout ce qui n'est pas un endpoint/fichier retombe sur le SPA.
app.MapFallbackToFile("index.html");

app.Run();

// Exposé pour WebApplicationFactory dans les tests fonctionnels
public partial class Program;
