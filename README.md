# MealPlanner

Générateur d'idées de repas selon des critères (saison, style *healthy/réconfortant/...*, temps de réalisation, ingrédients).

## Stack

- **Backend** : .NET 10 / C# 14, ASP.NET Core (Minimal API), EF Core 10, MySQL (provider Oracle `MySql.EntityFrameworkCore`), Serilog.
- **Architecture** : monolithe modulaire + vertical slice léger + CQRS (dispatcher maison, sans MediatR).
- **Front** : React 19 + TypeScript, Vite.
- **Tests** : xUnit v3, FluentAssertions, NSubstitute, AutoFixture, Testcontainers (MySQL réel), WebApplicationFactory, NetArchTest ; Vitest + Testing Library côté front.

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
  *.IntegrationTests     # MySQL réel via Testcontainers (Docker requis)
  *.FunctionalTests      # HTTP bout-en-bout via WebApplicationFactory
  ArchitectureTests      # règles d'architecture (NetArchTest)
web/meal-planner-web/    # front React + Vite + Vitest
```

Chaque module s'auto-compose via `AddMealsModule()` / `MapMealsModule()`. Les endpoints (Minimal API)
vivent dans le module, à côté de leur handler (vertical slice).

## Démarrer

```bash
# 1. Base de données
docker compose up -d

# 2. Lancer l'API (Scalar sur /scalar/v1 en Development)
#    En Development, les migrations sont appliquées ET le catalogue est seedé automatiquement au démarrage.
dotnet run --project src/Api/MealPlanner.Api

# 3. Front
cd web/meal-planner-web && npm install && npm run dev
```

> Hors Development (ou pour appliquer les migrations à la main) :
> ```bash
> dotnet dotnet-ef database update \
>   --project src/Modules/Meals/MealPlanner.Modules.Meals \
>   --startup-project src/Api/MealPlanner.Api
> ```

## Tests

```bash
# Backend (les projets de test se lancent un par un avec VSTest)
dotnet test tests/MealPlanner.SharedKernel.UnitTests/MealPlanner.SharedKernel.UnitTests.csproj
dotnet test tests/MealPlanner.Modules.Meals.UnitTests/MealPlanner.Modules.Meals.UnitTests.csproj
dotnet test tests/MealPlanner.ArchitectureTests/MealPlanner.ArchitectureTests.csproj
dotnet test tests/MealPlanner.Api.FunctionalTests/MealPlanner.Api.FunctionalTests.csproj
dotnet test tests/MealPlanner.Modules.Meals.IntegrationTests/MealPlanner.Modules.Meals.IntegrationTests.csproj  # Docker requis

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
