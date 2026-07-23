using System.Security.Cryptography;
using System.Text;

using Konscious.Security.Cryptography;

using MealPlanner.Modules.Identity.Domain;

using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Modules.Identity.Authentication;

/// <summary>
/// Hachage des mots de passe avec Argon2id (recommandation OWASP), en remplacement du
/// <see cref="PasswordHasher{TUser}"/> PBKDF2 par défaut d'ASP.NET Core Identity. Le mot de passe
/// n'est jamais stocké ni journalisé en clair ; seul le condensat auto-descriptif est persisté.
/// </summary>
internal sealed class Argon2PasswordHasher : IPasswordHasher<AppUser>
{
    // Paramètres OWASP (Argon2id) : 19 Mio de mémoire, 2 itérations, 1 voie parallèle.
    private const int MemorySizeKib = 19_456;
    private const int Iterations = 2;
    private const int DegreeOfParallelism = 1;
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const string Prefix = "$argon2id$";

    public string HashPassword(AppUser user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = ComputeHash(password, salt);

        // Format PHC auto-descriptif : $argon2id$v=19$m=..,t=..,p=..$<salt b64>$<hash b64>
        return $"{Prefix}v=19$m={MemorySizeKib},t={Iterations},p={DegreeOfParallelism}$"
            + $"{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword)
    {
        if (!TryParse(hashedPassword, out var parameters, out var salt, out var expectedHash))
        {
            return PasswordVerificationResult.Failed;
        }

        var actualHash = ComputeHash(providedPassword, salt, parameters);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }

    private static byte[] ComputeHash(string password, byte[] salt, Argon2Parameters? parameters = null)
    {
        var effective = parameters ?? new Argon2Parameters(MemorySizeKib, Iterations, DegreeOfParallelism);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = effective.MemorySizeKib,
            Iterations = effective.Iterations,
            DegreeOfParallelism = effective.DegreeOfParallelism,
        };

        return argon2.GetBytes(HashSizeBytes);
    }

    private static bool TryParse(
        string hashedPassword,
        out Argon2Parameters parameters,
        out byte[] salt,
        out byte[] hash)
    {
        parameters = default;
        salt = [];
        hash = [];

        if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        // Segments : ["", "argon2id", "v=19", "m=..,t=..,p=..", "<salt>", "<hash>"]
        var segments = hashedPassword.Split('$');
        if (segments.Length != 6)
        {
            return false;
        }

        try
        {
            var costs = segments[3].Split(',');
            var memory = int.Parse(costs[0].AsSpan(2));
            var iterations = int.Parse(costs[1].AsSpan(2));
            var parallelism = int.Parse(costs[2].AsSpan(2));

            parameters = new Argon2Parameters(memory, iterations, parallelism);
            salt = Convert.FromBase64String(segments[4]);
            hash = Convert.FromBase64String(segments[5]);
            return salt.Length > 0 && hash.Length > 0;
        }
        catch (Exception exception) when (exception is FormatException or IndexOutOfRangeException or OverflowException)
        {
            return false;
        }
    }

    private readonly record struct Argon2Parameters(int MemorySizeKib, int Iterations, int DegreeOfParallelism);
}
