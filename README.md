# MealPlanner

Générateur d'idées de repas selon des critères (saison, style *healthy/réconfortant/...*, temps de réalisation, ingrédients).

## Stack

- **Backend** : .NET 10 / C# 14, ASP.NET Core (Minimal API), EF Core 10, SQL Server / Azure SQL (provider `Microsoft.EntityFrameworkCore.SqlServer`), Serilog.
- **Architecture** : monolithe modulaire + vertical slice léger + CQRS (dispatcher maison, sans MediatR).
- **Front** : React 19 + TypeScript, Vite — servi en *same-origin* par l'API en production (`wwwroot`).
- **Tests** : xUnit v3, FluentAssertions, NSubstitute, AutoFixture, SQL Server via Testcontainers, WebApplicationFactory, NetArchTest ; Vitest + Testing Library côté front.

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
  *.IntegrationTests     # SQL Server éphémère via Testcontainers (Docker requis)
  *.FunctionalTests      # HTTP bout-en-bout via WebApplicationFactory
  ArchitectureTests      # règles d'architecture (NetArchTest)
web/meal-planner-web/    # front React + Vite + Vitest
```

Chaque module s'auto-compose via `AddMealsModule()` / `MapMealsModule()`. Les endpoints (Minimal API)
vivent dans le module, à côté de leur handler (vertical slice).

## Démarrer

```bash
# 1. Lancer SQL Server local (l'API cible localhost,1433 via appsettings.json)
docker compose up -d

# 2. Lancer l'API (Scalar sur /scalar/v1 en Development)
#    En Development, les migrations sont appliquées au démarrage ; l'API crée la base « MealPlanner »
#    à la première exécution. Le catalogue de démarrage est cloné pour chaque utilisateur à l'inscription.
dotnet run --project src/Api/MealPlanner.Api

# 3. Front
cd web/meal-planner-web && npm install && npm run dev
```

> **Docker requis** en local : SQL Server tourne dans un conteneur (`docker-compose.yml`).
> Sur Apple Silicon, l'image `mssql/server` (amd64) est émulée automatiquement.
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

## Déploiement (production / Azure)

Les migrations sont appliquées **au démarrage dans tous les environnements** (SQLite mono-instance) ;
le dossier du fichier de base est créé automatiquement s'il manque. En dehors de `Development`, un
échec d'initialisation fait **échouer le démarrage** (fail-fast) plutôt que de servir une app cassée.

- **Base persistante** : `appsettings.Production.json` pointe par défaut vers `Data Source=/home/data/mealplanner.db`
  (`/home` est le stockage persistant d'Azure App Service Linux). Surchargeable par variables d'environnement :
  `ConnectionStrings__MealsDb` / `ConnectionStrings__IdentityDb` (les deux visent le même fichier).
- **Secret JWT** : jamais commité. À fournir via variable d'environnement `Jwt__SigningKey` (≥ 32 octets)
  ou les App Settings Azure. Idem `Authentication__Google__ClientId`, `Authentication__Facebook__*` si utilisés.
- **Sauvegarde** : la base est un simple fichier — copie de `mealplanner.db` (idéalement à froid, ou via
  réplication continue type Litestream vers un Blob Storage).

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
