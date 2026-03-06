using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Data;

internal static class DatabaseInitializer
{
    public static void Initialize()
    {
        // Verificar si la base de datos existe pero está vacía de productos
        using var testConnection = AppDatabase.CreateConnection();
        testConnection.Open();
        
        using var command = testConnection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ProductoExt';";
        var tableExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
        
        if (tableExists)
        {
            command.CommandText = "SELECT COUNT(*) FROM ProductoExt;";
            var productCount = Convert.ToInt32(command.ExecuteScalar());
            
            // Si la tabla existe pero está vacía, eliminar y reiniciar
            if (productCount == 0)
            {
                testConnection.Close();
                try
                {
                    File.Delete(AppDatabase.DatabasePath);
                }
                catch { }
            }
        }

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
    TipoIva TEXT NOT NULL DEFAULT 'GRAVADO',
    Activo INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);");

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS ParametroIva (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL UNIQUE,
    Valor REAL NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
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
    IvaPorcentaje REAL NOT NULL,
    IvaValor REAL NOT NULL,
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
    AplicaIva INTEGER NOT NULL DEFAULT 1,
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
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO ParametroIva(Nombre, Valor, Activo) VALUES ('IVA_GENERAL', 0.19, 1);");

        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Efectivo', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Transferencia', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Crédito', 1);");

        // 10 productos de aceite
        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-5L', 'Aceite Extra Virgen 5L', 'Und', 85000, 150, 20, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-1L', 'Aceite Extra Virgen 1L', 'Und', 22000, 300, 50, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-250ML', 'Aceite Extra Virgen 250ml', 'Und', 8500, 400, 80, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-PURO-5L', 'Aceite Puro 5L', 'Und', 45000, 200, 30, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-PURO-1L', 'Aceite Puro 1L', 'Und', 12000, 350, 60, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-OLIVA-500ML', 'Aceite de Oliva 500ml', 'Und', 35000, 120, 25, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-GIRASOL-5L', 'Aceite Girasol 5L', 'Und', 32000, 180, 40, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-PALMA-5L', 'Aceite Palma 5L', 'Und', 28000, 220, 35, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-COCO-1L', 'Aceite Coco 1L', 'Und', 18000, 280, 50, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES ('ACE-SESAMO-250ML', 'Aceite Sésamo 250ml', 'Und', 15000, 160, 30, 1);");

        ExecuteNonQuery(connection, transaction, @"
INSERT OR IGNORE INTO Cliente(Codigo, Nombre, Nit, Direccion, Telefono, Email, TipoIva, Activo)
VALUES
('CLI-0001', 'Aceites del Sur', '800123456-7', 'Av. Siempre Viva 123, Ciudad', '+57 3101111111', 'contacto@aceitesdelsur.com', 'GRAVADO', 1),
('CLI-0002', 'Olivares SA', '900987654-3', 'Calle Falsa 456, Pueblo', '+57 3102222222', 'ventas@olivares.com', 'GRAVADO', 1),
('CLI-0003', 'Aceite Natural', '800112233-5', 'Plaza Central 789, Ciudad', '+57 3103333333', 'info@aceitenatural.com', 'EXENTO', 1),
('CLI-0004', 'Oleo Plus', '900556677-9', 'Calle Nueva 202, Pueblo', '+57 3104444444', 'ventas@oleoplus.com', 'GRAVADO', 1),
('CLI-0005', 'La Oliva', '800445566-8', 'Av. Libertad 101, Ciudad', '+57 3105555555', 'contacto@laoliva.com', 'GRAVADO', 1),
('CLI-0006', 'Distribuciones Norte', '901234567-1', 'Cra 15 #10-20, Bogotá', '+57 3106666666', 'norte@distribuciones.com', 'GRAVADO', 1),
('CLI-0007', 'Mercado Central', '1020304050', 'Carrera 8 #45-21, Medellín', '+57 3107777777', 'compras@mercadocentral.com', 'EXENTO', 1),
('CLI-0008', 'Comercial Andina', '901112223-4', 'Calle 72 #14-10, Bogotá', '+57 3108888888', 'andina@comercial.com', 'GRAVADO', 1),
('CLI-0009', 'Supertienda El Ahorro', '900765432-2', 'Av. 30 de Agosto #56-90, Pereira', '+57 3109999999', 'admin@elahorro.com', 'GRAVADO', 1),
('CLI-0010', 'Minimarket San José', '1098765432', 'Barrio San José, Manizales', '+57 3110000000', 'sanjose@minimarket.com', 'EXENTO', 1);");
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public static void SaveChanges()
    {
        // No-op for compatibility
    }
}
