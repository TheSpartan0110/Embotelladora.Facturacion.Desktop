using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Data;

internal static class AppDatabase
{
    private static readonly string DataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
    public static readonly string DatabasePath = Path.Combine(DataDirectory, "aceitespro.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection CreateConnection()
    {
        Directory.CreateDirectory(DataDirectory);
        return new SqliteConnection(ConnectionString);
    }
}
