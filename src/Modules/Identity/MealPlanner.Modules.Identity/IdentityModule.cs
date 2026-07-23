using FluentValidation;

using MealPlanner.Modules.Identity.Authentication;
using MealPlanner.Modules.Identity.Domain;
using MealPlanner.Modules.Identity.Features.FacebookLogin;
using MealPlanner.Modules.Identity.Features.GetCurrentUser;
using MealPlanner.Modules.Identity.Features.GoogleLogin;
using MealPlanner.Modules.Identity.Features.Login;
using MealPlanner.Modules.Identity.Features.Preferences;
using MealPlanner.Modules.Identity.Features.RefreshToken;
using MealPlanner.Modules.Identity.Features.Register;
using MealPlanner.Modules.Identity.Infrastructure;
using MealPlanner.SharedKernel.Cqrs;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MealPlanner.Modules.Identity;

/// <summary>Composition du module Identity : persistance, ASP.NET Core Identity, JWT, auth externe, endpoints.</summary>
public static class IdentityModule
{
    public const string ConnectionStringName = "IdentityDb";

    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Chaîne de connexion '{ConnectionStringName}' introuvable.");

        services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.Configure<FacebookAuthOptions>(configuration.GetSection(FacebookAuthOptions.SectionName));

        services.AddIdentityCore<AppUser>(options =>
            {
                // NIST SP 800-63B : la longueur prime, pas de règles de composition arbitraires.
                // On désactive donc les exigences majuscule/minuscule/chiffre/spécial d'Identity.
                options.Password.RequiredLength = RegisterValidator.MinPasswordLength;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppIdentityDbContext>();

        // Hachage Argon2id (OWASP) en remplacement du PasswordHasher PBKDF2 par défaut d'Identity.
        services.Replace(ServiceDescriptor.Scoped<IPasswordHasher<AppUser>, Argon2PasswordHasher>());

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IFacebookTokenValidator, FacebookTokenValidator>();
        services.AddScoped<UserRegisteredNotifier>();
        services.AddScoped<ExternalLoginService>();

        services.AddHttpClient(FacebookTokenValidator.HttpClientName,
            client => client.BaseAddress = new Uri("https://graph.facebook.com/v21.0/"));

        services.AddDispatcher();
        services.AddCqrsHandlersFromAssembly(typeof(IdentityModule).Assembly);
        services.AddValidatorsFromAssemblyContaining<RegisterValidator>(includeInternalTypes: true);

        AddJwtAuthentication(services, configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints)
    {
        RegisterEndpoint.Map(endpoints);
        LoginEndpoint.Map(endpoints);
        GoogleLoginEndpoint.Map(endpoints);
        FacebookLoginEndpoint.Map(endpoints);
        RefreshTokenEndpoint.Map(endpoints);
        GetCurrentUserEndpoint.Map(endpoints);
        PreferencesEndpoint.Map(endpoints);
        return endpoints;
    }

    /// <summary>Applique les migrations du module. À appeler au démarrage.</summary>
    public static async Task InitializeIdentityModuleAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Claims conservés bruts (sub, email) : pas de remapping vers les ClaimTypes .NET.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = JwtBearerParameters.Create(jwtOptions);
            });

        services.AddAuthorization();
    }
}
