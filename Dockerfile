# syntax=docker/dockerfile:1

# Image unique (same-origin) : l'API .NET sert aussi le SPA React buildé (wwwroot).
# Build reproductible en trois étapes ; l'image finale ne contient que le runtime + l'app.

# --- Étape 1 : build du front (Vite) ---
FROM node:22-alpine AS web
WORKDIR /web
# Client ID OAuth Google (public) : inliné par Vite dans le bundle au build.
# Vide par défaut -> bouton Google inactif (l'auth email/mot de passe reste dispo).
ARG VITE_GOOGLE_CLIENT_ID=""
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
COPY web/meal-planner-web/package.json web/meal-planner-web/package-lock.json ./
RUN npm ci
COPY web/meal-planner-web/ ./
RUN npm run build

# --- Étape 2 : build + publish de l'API (.NET) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
# Fichiers de résolution de versions (Central Package Management) copiés en premier pour le cache.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet restore src/Api/MealPlanner.Api/MealPlanner.Api.csproj
RUN dotnet publish src/Api/MealPlanner.Api/MealPlanner.Api.csproj -c Release -o /app --no-restore

# --- Étape 3 : image runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=api /app ./
# Le SPA buildé est servi en same-origin par l'API (UseStaticFiles + MapFallbackToFile).
COPY --from=web /web/dist ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MealPlanner.Api.dll"]
