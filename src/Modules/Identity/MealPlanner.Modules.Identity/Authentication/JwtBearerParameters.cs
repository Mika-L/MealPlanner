using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>
/// Construit les <see cref="TokenValidationParameters"/> à partir des <see cref="JwtOptions"/> : mêmes
/// clé, émetteur et audience que l'émission, pour que le JwtBearer valide nos propres jetons.
/// </summary>
public static class JwtBearerParameters
{
    public static TokenValidationParameters Create(JwtOptions options) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = options.Issuer,
        ValidateAudience = true,
        ValidAudience = options.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
    };
}
