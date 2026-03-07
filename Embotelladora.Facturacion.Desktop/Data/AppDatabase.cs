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

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection CreateConnection()
    {
        EnsureDatabaseLocation();
        return new SqliteConnection(ConnectionString);
    }

    private static void EnsureDatabaseLocation()
    {
        Directory.CreateDirectory(DataDirectory);

        if (File.Exists(DatabasePath) || !File.Exists(LegacyDatabasePath))
        {
            return;
        }

        File.Copy(LegacyDatabasePath, DatabasePath, overwrite: false);
    }
}
