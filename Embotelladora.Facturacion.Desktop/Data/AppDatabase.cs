using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Data;

internal static class AppDatabase
{
    private const string AppFolderName = "AceitesPro Facturacion";
    private static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName,
        "data");
    private static readonly string LegacyDataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
    private static readonly string LegacyDatabasePath = Path.Combine(LegacyDataDirectory, "aceitespro.db");
    public static readonly string DatabasePath = Path.Combine(DataDirectory, "aceitespro.db");
    private static readonly string ImportPendingPath = Path.Combine(DataDirectory, "import_pending.db");
    public static string ImportPendingFilePath => ImportPendingPath;

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection CreateConnection()
    {
        EnsureDatabaseLocation();
        return new SqliteConnection(ConnectionString);
    }

    private static void EnsureDatabaseLocation()
    {
        Directory.CreateDirectory(DataDirectory);

        // If an import was scheduled by the running application (to avoid replacing a locked DB file),
        // apply it now before any connections are created.
        try
        {
            if (File.Exists(ImportPendingPath))
            {
                // Overwrite existing database with the imported one
                File.Copy(ImportPendingPath, DatabasePath, overwrite: true);
                File.Delete(ImportPendingPath);
            }
        }
        catch
        {
            // Ignore errors here; fallback logic below will attempt to ensure DB exists.
        }

        // If database already exists in user data folder, nothing more to do.
        if (File.Exists(DatabasePath))
        {
            return;
        }

        // If there's a legacy DB bundled with the app, copy it to the user data folder on first run.
        if (File.Exists(LegacyDatabasePath))
        {
            File.Copy(LegacyDatabasePath, DatabasePath, overwrite: false);
        }
    }
}
