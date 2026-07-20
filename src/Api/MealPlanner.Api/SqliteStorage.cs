using Microsoft.Data.Sqlite;

namespace MealPlanner.Api;

/// <summary>Préparation du stockage SQLite côté hôte.</summary>
public static class SqliteStorage
{
    /// <summary>Crée le dossier parent du fichier SQLite si besoin : SQLite ouvre le fichier mais ne
    /// crée pas les dossiers manquants (ex. <c>/home/data/</c> sur Azure App Service).</summary>
    public static void EnsureDirectoryExists(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
