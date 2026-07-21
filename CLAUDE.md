# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **Note** : ce dépôt est un projet **perso**. Les conventions Inqom du `~/.claude/CLAUDE.md` global
> (React `@inqom`, Redux, phantom, MCP inqom-shared, prettier, etc.) **ne s'appliquent PAS ici**.

MealPlanner : générateur d'idées de repas + CRUD de recettes, filtré par règles déterministes (saison,
style, temps, ingrédients du frigo) — pas de LLM. Backend .NET 10 (C# 14), front React 19 + Vite.

## Commandes

```bash
# Prérequis local : SQL Server dans un conteneur (l'API cible localhost,1433)
docker compose up -d

# Backend
dotnet build MealPlanner.slnx                              # build (TreatWarningsAsErrors : 0 warning = 0 erreur)
dotnet run --project src/Api/MealPlanner.Api              # API + Scalar sur /scalar/v1 (Development)
dotnet test tests/<Projet>/<Projet>.csproj               # un projet de test (VSTest lance projet par projet)
dotnet test tests/<Projet> --filter "FullyQualifiedName~Should_match_search_term"  # un seul test

# Front
cd web/meal-planner-web && npm install && npm run dev     # Vite :5173, proxy /api -> :5226
npm run lint    # oxlint
npm test        # vitest

# Migrations EF (1 contexte par module ; --output-dir OBLIGATOIRE sinon EF écrit dans Migrations/ racine)
dotnet dotnet-ef migrations add <Nom> --context MealsDbContext \
  --project src/Modules/Meals/MealPlanner.Modules.Meals \
  --startup-project src/Api/MealPlanner.Api --output-dir Infrastructure/Migrations
```

Les tests d'intégration/fonctionnels démarrent un **vrai SQL Server via Testcontainers** (Docker requis ;
image `mssql/server` amd64 émulée sur Apple Silicon). Un conteneur est partagé par collection (tests
séquentiels) ; ne pas paralléliser les projets d'intégration sur Mac (RAM).

## Architecture

**Monolithe modulaire + vertical slice + CQRS**, un projet par module sous `src/Modules/<X>/`.

- **Composition par module** : chaque module expose la triade `AddXModule(services, config)` /
  `MapXModule(endpoints)` / `InitializeXModuleAsync(serviceProvider)` (migrations au démarrage).
  `Program.cs` (`src/Api/MealPlanner.Api`) est le seul composition root : il enchaîne ces appels.
- **Vertical slice** : `Features/<Nom>/` co-localise `Query|Command` + `Validator` + `Handler` +
  `Endpoint` (Minimal API, mappé depuis le module). Les endpoints vivent dans le module, pas dans l'API.
- **CQRS maison** (`SharedKernel/Cqrs`, pas de MediatR — licence) : `ICommand<T>`/`IQuery<T>` +
  `ICommandHandler`/`IQueryHandler` (méthode `HandleAsync`), dispatcher `IDispatcher.SendAsync` (commandes)
  / `QueryAsync` (requêtes). ⚠️ Le dispatcher invoque le handler par **réflexion sur la méthode de
  l'interface publique** — surtout PAS via `dynamic` : les handlers sont `internal`, le binder `dynamic`
  (exécuté depuis SharedKernel) ne les voit pas → `RuntimeBinderException`.
- **Résultats** (`SharedKernel/Results`) : les handlers renvoient `Result` / `Result<T>` (jamais
  d'exceptions pour le flux métier). `Error.Type` (`ErrorType`) est mappé en ProblemDetails par
  `Features/EndpointResults.ToProblem()` côté endpoint.
- **Découplage inter-modules** (vérifié par `tests/MealPlanner.ArchitectureTests`, NetArchTest) : Meals et
  Identity ne se référencent jamais. Communication via abstractions `SharedKernel` : `ICurrentUser`
  (impl `HttpContextCurrentUser` dans l'API, lit le claim `sub`) et `IUserRegisteredListener` (hook
  in-process : Identity le déclenche à l'inscription, `UserCatalogSeeder` de Meals l'implémente pour
  cloner le catalogue de démarrage). `Domain` ne dépend pas d'`Infrastructure`.
- **Données 100 % par utilisateur** : pas de catalogue global ; à l'inscription l'utilisateur reçoit une
  **copie** du template. Tous les handlers Meals filtrent sur `Meal.OwnerId` (via `ICurrentUser`), tous
  les endpoints Meals sont `.RequireAuthorization()`.

## Persistance (SQL Server / Azure SQL)

- Provider `Microsoft.EntityFrameworkCore.SqlServer`. **Deux DbContexts** (`MealsDbContext`,
  `AppIdentityDbContext`) sur **une seule base**, isolés par **préfixe de table** (`Meals_` / `Identity_`),
  `__EFMigrationsHistory` partagé (ids horodatés uniques). Configs EF dans `Infrastructure/Configurations/`.
- **Recherche insensible casse + accents** : collation SQL native `Latin1_General_CI_AI`
  (`MealsDbContext.SearchCollation`, appliquée via `UseCollation` sur `Meal.Name`/`Description` +
  `MealIngredient.Name`). `ListMeals` fait recherche ET pagination en SQL (`.Contains()` → LIKE).
  `SearchText.Normalize` (normalisation applicative) ne sert QUE au matching frigo de `GenerateMealIdeas`
  (logique en mémoire, non traduisible en SQL).
- **Clés Guid v7 assignées par le domaine** (dans le ctor) → configurées `ValueGeneratedNever()`. Sinon EF
  les traite en `ValueGeneratedOnAdd` et émet un UPDATE fantôme lors d'un remplacement de collection
  suivie (`Clear()` + re-`Add`) → `DbUpdateConcurrencyException`.
- Enums `[Flags]` (`Season`, `MealStyle`) sérialisées par nom : `JsonStringEnumConverter` enregistré dans
  `Program.cs`. JWT : `MapInboundClaims = false` (claims bruts `sub`/`email`).

## Conventions & pièges

- **`Directory.Packages.props`** (Central Package Management) : **JAMAIS l'éditer via Edit/Write** — un
  formateur post-édition duplique son contenu → NU1506 (fichier gonflé). Le modifier en **python** puis
  vérifier `wc -l`.
- `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild` : tout `using` inutile casse le build. Tests xUnit v3 :
  passer `TestContext.Current.CancellationToken` (analyseur xUnit1051).
- **Versions figées** (à ne pas bumper sans raison) : FluentAssertions **v7** (v8+ payant), NSubstitute
  **5.3.0** (contrainte AutoFixture.AutoNSubstitute), `Microsoft.OpenApi` pinné (vuln transitive NU1903).
- `dotnet run` en arrière-plan : tuer par port (`lsof -ti :5226 | xargs kill`), pas par PID (wrapper).

## Front

React 19 **vanilla** (pas de Redux/framework UI). Auth maison : `src/auth/tokenStore.ts` (session +
localStorage + refresh single-flight) → `src/api/client.ts` `apiFetch` (injecte le Bearer, rejoue une fois
sur 401 après refresh). Le front tape l'API en **chemins relatifs** (`/api/...`, pas de base URL) →
**same-origin** : en prod l'API sert le SPA depuis `wwwroot` (`UseStaticFiles` + `MapFallbackToFile`).

## Déploiement

Stack Azure « tout gratuit » : Azure SQL serverless (offre gratuite, auto-pause) + Container Apps
scale-to-zero + image publique **ghcr.io**, auth SQL en **Managed Identity** (aucun mot de passe). Infra as
code `infra/main.bicep` (par env), pipeline `.github/workflows/deploy.yml`, image `Dockerfile` multi-stage
(Vite → publish .NET → runtime, port 8080).

> **⚠️ Avant tout `az` de déploiement** : le contexte `az` par défaut (`~/.azure`) peut pointer un **autre
> compte** que celui visé. Toujours travailler dans un répertoire de config **isolé** — `az login` puis
> toutes les commandes préfixées de `AZURE_CONFIG_DIR="$HOME/.azure-perso"` — et vérifier `az account show`
> avant de déployer. Les valeurs concrètes (souscription, tenant, ressources dev déjà en ligne) sont dans
> la **mémoire projet**, pas ici (repo public).

Détails, prérequis one-time : voir `README.md` (section « Déploiement »).
