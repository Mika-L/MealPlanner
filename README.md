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

## Déploiement (Azure — dev & prod)

Stack **« tout gratuit »** : **Azure SQL** serverless (offre gratuite, auto-pause) + **Container App**
scale-to-zero, image publique sur **ghcr.io**, un environnement par *resource group*
(`rg-mealplanner-dev` / `-prod`), région **France Central**. L'API sert le SPA en *same-origin*
(image conteneur unique). Auth SQL en **Managed Identity** (aucun mot de passe). Coût ~0 € pour les
deux environnements ; contrepartie : *cold start* (~30-60 s) au réveil après inactivité.

Les migrations sont appliquées **au démarrage** ; hors `Development` un échec fait **échouer le
démarrage** (fail-fast). En prod, l'API est en `Production` (voir le `Dockerfile`) et lit sa
configuration via variables d'environnement injectées par le Container App.

- **Infra as code** : `infra/main.bicep` (paramétré par `environmentName`), déployé par
  `.github/workflows/deploy.yml`.
- **Connexion SQL** : injectée par le Container App (`ConnectionStrings__MealsDb` / `__IdentityDb`),
  en `Authentication=Active Directory Managed Identity`. La MI reçoit ses droits
  (`db_datareader`/`db_datawriter`/`db_ddladmin`) via une étape T-SQL du pipeline.
- **Secret JWT** : `Jwt__SigningKey` (≥ 32 octets), secret GitHub `JWT_SIGNING_KEY` → secret du
  Container App. Idem `Authentication__Google__*` / `Facebook__*` si utilisés (à ajouter au Bicep).

### Prérequis (une seule fois)

1. **App Entra + fédération OIDC** pour GitHub Actions ; secrets repo/environnement :
   `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `JWT_SIGNING_KEY` ; variables
   `AZURE_ADMIN_LOGIN` (nom d'affichage du principal) et `AZURE_ADMIN_OBJECT_ID` (son *object id*,
   `az ad sp show --id <AZURE_CLIENT_ID> --query id -o tsv`) — ce principal devient l'admin Entra du serveur SQL.
2. **Environnements GitHub** `dev` et `prod` (le `prod` peut exiger une approbation).
3. Après le **premier push d'image**, passer le package ghcr **`mealplanner` en visibilité publique**
   (sinon le Container App ne peut pas le tirer anonymement).

Déclenchement : push sur `main` → **dev** ; `workflow_dispatch` → **dev** ou **prod**.

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
