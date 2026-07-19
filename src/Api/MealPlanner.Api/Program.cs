using System.Text.Json.Serialization;

using MealPlanner.Modules.Meals;

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

// Modules (modular monolith) — chaque module se compose lui-même
builder.Services.AddMealsModule(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Migration + seed automatiques en dev ; l'API démarre même si la base est indisponible.
    try
    {
        await app.Services.InitializeMealsModuleAsync();
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception, "Initialisation du module Meals impossible (base indisponible ?).");
    }
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("System");

// Endpoints des modules
app.MapMealsModule();

app.Run();

// Exposé pour WebApplicationFactory dans les tests fonctionnels
public partial class Program;
