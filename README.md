# MealPlanner

Générateur d'idées de repas selon des critères (saison, style *healthy/réconfortant/...*, temps de réalisation, ingrédients).

## Stack

- **Backend** : .NET 10 / C# 14, ASP.NET Core (Minimal API), EF Core 10, SQLite (provider `Microsoft.EntityFrameworkCore.Sqlite`, mode WAL), Serilog.
- **Architecture** : monolithe modulaire + vertical slice léger + CQRS (dispatcher maison, sans MediatR).
- **Front** : React 19 + TypeScript, Vite.
- **Tests** : xUnit v3, FluentAssertions, NSubstitute, AutoFixture, SQLite (fichier temporaire, sans Docker), WebApplicationFactory, NetArchTest ; Vitest + Testing Library côté front.

## Structure

```
src/
  Api/MealPlanner.Api                       # Host ASP.NET Core, composition root, Serilog, OpenAPI/Scalar
  BuildingBlocks/MealPlanner.SharedKernel   # CQRS (ICommand/IQuery + dispatcher), Result, Error
  Modules/Meals/MealPlanner.Modules.Meals   # Module Meals (vertical slice)
    Domain/                                 #   entités + enums (persistence-ignorant)
    Infrastructure/                         #   DbContext, configurations EF, migrations
    Features/GenerateMealIdeas/             #   1 slice = Query + Validator + Handler + Endpoint
tests/
  *.UnitTests            # tests unitaires (xUnit v3 + FluentAssertions + NSubstitute + AutoFixture)
  *.IntegrationTests     # SQLite sur fichier temporaire (aucun service externe)
  *.FunctionalTests      # HTTP bout-en-bout via WebApplicationFactory
  ArchitectureTests      # règles d'architecture (NetArchTest)
web/meal-planner-web/    # front React + Vite + Vitest
```

Chaque module s'auto-compose via `AddMealsModule()` / `MapMealsModule()`. Les endpoints (Minimal API)
vivent dans le module, à côté de leur handler (vertical slice).

## Démarrer

```bash
# 1. Lancer l'API (Scalar sur /scalar/v1 en Development)
#    En Development, les migrations sont appliquées au démarrage ; SQLite crée le fichier
#    mealplanner.db (+ .db-wal/.db-shm) dans le dossier de l'API à la première exécution.
#    Le catalogue de démarrage est cloné pour chaque utilisateur à son inscription.
dotnet run --project src/Api/MealPlanner.Api

# 2. Front
cd web/meal-planner-web && npm install && npm run dev
```

> Aucun service externe requis : SQLite est une base fichier embarquée.
>
> Hors Development (ou pour appliquer les migrations à la main) — un contexte par module :
> ```bash
> dotnet dotnet-ef database update --context MealsDbContext \
>   --project src/Modules/Meals/MealPlanner.Modules.Meals \
>   --startup-project src/Api/MealPlanner.Api
> dotnet dotnet-ef database update --context AppIdentityDbContext \
>   --project src/Modules/Identity/MealPlanner.Modules.Identity \
>   --startup-project src/Api/MealPlanner.Api
> ```

## Tests

```bash
# Backend (les projets de test se lancent un par un avec VSTest)
dotnet test tests/MealPlanner.SharedKernel.UnitTests/MealPlanner.SharedKernel.UnitTests.csproj
dotnet test tests/MealPlanner.Modules.Meals.UnitTests/MealPlanner.Modules.Meals.UnitTests.csproj
dotnet test tests/MealPlanner.ArchitectureTests/MealPlanner.ArchitectureTests.csproj
dotnet test tests/MealPlanner.Api.FunctionalTests/MealPlanner.Api.FunctionalTests.csproj
dotnet test tests/MealPlanner.Modules.Meals.IntegrationTests/MealPlanner.Modules.Meals.IntegrationTests.csproj
dotnet test tests/MealPlanner.Modules.Identity.IntegrationTests/MealPlanner.Modules.Identity.IntegrationTests.csproj

# Front
cd web/meal-planner-web && npm test
```

## Endpoint disponible

`POST /api/meals/ideas`

```json
{
  "season": "Winter",
  "styles": "Comforting",
  "maxPrepTimeMinutes": 45,
  "includeIngredients": ["carotte"],
  "count": 10
}
```
