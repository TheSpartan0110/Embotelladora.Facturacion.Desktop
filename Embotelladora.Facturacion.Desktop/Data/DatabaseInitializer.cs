using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Data;

internal static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS Cliente (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    Nombre TEXT NOT NULL,
    Nit TEXT NOT NULL UNIQUE,
    Direccion TEXT,
    Telefono TEXT,
    Email TEXT,
    Activo INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS MetodoPago (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL UNIQUE,
    Activo INTEGER NOT NULL DEFAULT 1
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS ProductoExt (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    Nombre TEXT NOT NULL,
    Unidad TEXT NOT NULL,
    PrecioBase REAL NOT NULL,
    StockActual REAL NOT NULL DEFAULT 0,
    StockMinimo REAL NOT NULL DEFAULT 0,
    Activo INTEGER NOT NULL DEFAULT 1
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS Factura (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Numero TEXT NOT NULL UNIQUE,
    Fecha TEXT NOT NULL,
    ClienteId INTEGER NOT NULL,
    MetodoPagoId INTEGER,
    Estado TEXT NOT NULL,
    Subtotal REAL NOT NULL,
    Retencion REAL NOT NULL DEFAULT 0,
    Total REAL NOT NULL,
    Saldo REAL NOT NULL,
    Notas TEXT,
    FOREIGN KEY(ClienteId) REFERENCES Cliente(Id),
    FOREIGN KEY(MetodoPagoId) REFERENCES MetodoPago(Id)
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS ItemFactura (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FacturaId INTEGER NOT NULL,
    ProductoId INTEGER,
    Descripcion TEXT NOT NULL,
    Cantidad REAL NOT NULL,
    PrecioUnitario REAL NOT NULL,
    TotalLinea REAL NOT NULL,
    FOREIGN KEY(FacturaId) REFERENCES Factura(Id),
    FOREIGN KEY(ProductoId) REFERENCES ProductoExt(Id)
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS Pago (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FacturaId INTEGER NOT NULL,
    Fecha TEXT NOT NULL,
    Valor REAL NOT NULL,
    MetodoPagoId INTEGER,
    Referencia TEXT,
    Notas TEXT,
    FOREIGN KEY(FacturaId) REFERENCES Factura(Id),
    FOREIGN KEY(MetodoPagoId) REFERENCES MetodoPago(Id)
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS MovimientoInventarioExt (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductoId INTEGER NOT NULL,
    Fecha TEXT NOT NULL,
    Tipo TEXT NOT NULL,
    Cantidad REAL NOT NULL,
    ReferenciaFacturaId INTEGER,
    Nota TEXT,
    FOREIGN KEY(ProductoId) REFERENCES ProductoExt(Id),
    FOREIGN KEY(ReferenciaFacturaId) REFERENCES Factura(Id)
);");

        SeedDefaults(connection, transaction);
        transaction.Commit();
    }

    private static void SeedDefaults(SqliteConnection connection, SqliteTransaction transaction)
    {
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Efectivo', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Transferencia', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Crédito', 1);");
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
