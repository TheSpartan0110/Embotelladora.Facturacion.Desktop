using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Data;

internal static class DatabaseInitializer
{
    public static void Initialize()
    {
        EnsureDatabaseWithoutIvaSchema();

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
        SeedMassiveData(connection, transaction, 50);
        transaction.Commit();
    }

    private static void EnsureDatabaseWithoutIvaSchema()
    {
        if (!File.Exists(AppDatabase.DatabasePath))
        {
            return;
        }

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        if (!HasTable(connection, "ProductoExt"))
        {
            return;
        }

        if (NeedsRebuildForNoIva(connection))
        {
            connection.Close();
            try
            {
                File.Delete(AppDatabase.DatabasePath);
            }
            catch
            {
            }
        }
    }

    private static bool NeedsRebuildForNoIva(SqliteConnection connection)
    {
        if (HasTable(connection, "ParametroIva"))
        {
            return true;
        }

        return HasColumn(connection, "Cliente", "TipoIva")
            || HasColumn(connection, "Factura", "IvaPorcentaje")
            || HasColumn(connection, "Factura", "IvaValor")
            || HasColumn(connection, "ItemFactura", "AplicaIva");
    }

    private static bool HasTable(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name;";
        command.Parameters.AddWithValue("@name", tableName);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool HasColumn(SqliteConnection connection, string tableName, string columnName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void SeedDefaults(SqliteConnection connection, SqliteTransaction transaction)
    {
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Efectivo', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Transferencia', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Crédito', 1);");

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
INSERT OR IGNORE INTO Cliente(Codigo, Nombre, Nit, Direccion, Telefono, Email, Activo)
VALUES
('CLI-0001', 'Aceites del Sur', '800123456-7', 'Av. Siempre Viva 123, Ciudad', '+57 3101111111', 'contacto@aceitesdelsur.com', 1),
('CLI-0002', 'Olivares SA', '900987654-3', 'Calle Falsa 456, Pueblo', '+57 3102222222', 'ventas@olivares.com', 1),
('CLI-0003', 'Aceite Natural', '800112233-5', 'Plaza Central 789, Ciudad', '+57 3103333333', 'info@aceitenatural.com', 1),
('CLI-0004', 'Oleo Plus', '900556677-9', 'Calle Nueva 202, Pueblo', '+57 3104444444', 'ventas@oleoplus.com', 1),
('CLI-0005', 'La Oliva', '800445566-8', 'Av. Libertad 101, Ciudad', '+57 3105555555', 'contacto@laoliva.com', 1),
('CLI-0006', 'Distribuciones Norte', '901234567-1', 'Cra 15 #10-20, Bogotá', '+57 3106666666', 'norte@distribuciones.com', 1),
('CLI-0007', 'Mercado Central', '1020304050', 'Carrera 8 #45-21, Medellín', '+57 3107777777', 'compras@mercadocentral.com', 1),
('CLI-0008', 'Comercial Andina', '901112223-4', 'Calle 72 #14-10, Bogotá', '+57 3108888888', 'andina@comercial.com', 1),
('CLI-0009', 'Supertienda El Ahorro', '900765432-2', 'Av. 30 de Agosto #56-90, Pereira', '+57 3109999999', 'admin@elahorro.com', 1),
('CLI-0010', 'Minimarket San José', '1098765432', 'Barrio San José, Manizales', '+57 3110000000', 'sanjose@minimarket.com', 1);");
    }

    private static void SeedMassiveData(SqliteConnection connection, SqliteTransaction transaction, int minRows)
    {
        var random = new Random(2026);

        EnsureMinimumMetodoPago(connection, transaction, minRows);
        EnsureMinimumClientes(connection, transaction, minRows);
        EnsureMinimumProductos(connection, transaction, minRows, random);
        EnsureMinimumFacturas(connection, transaction, minRows, random);
        EnsureMinimumItemsFactura(connection, transaction, minRows, random);
        EnsureMinimumPagos(connection, transaction, minRows, random);
        EnsureMinimumMovimientos(connection, transaction, minRows, random);
    }

    private static void EnsureMinimumMetodoPago(SqliteConnection connection, SqliteTransaction transaction, int minRows)
    {
        var count = CountRows(connection, transaction, "MetodoPago");
        for (var i = count + 1; i <= minRows; i++)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES (@nombre, 1);";
            command.Parameters.AddWithValue("@nombre", $"Método {i:000}");
            command.ExecuteNonQuery();
        }
    }

    private static void EnsureMinimumClientes(SqliteConnection connection, SqliteTransaction transaction, int minRows)
    {
        var count = CountRows(connection, transaction, "Cliente");
        for (var i = count + 1; i <= minRows; i++)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT OR IGNORE INTO Cliente(Codigo, Nombre, Nit, Direccion, Telefono, Email, Activo)
VALUES(@codigo, @nombre, @nit, @direccion, @telefono, @email, 1);";
            command.Parameters.AddWithValue("@codigo", $"CLI-{i:0000}");
            command.Parameters.AddWithValue("@nombre", $"Cliente Demo {i:000}");
            command.Parameters.AddWithValue("@nit", $"90{i:0000000}-{(i % 9) + 1}");
            command.Parameters.AddWithValue("@direccion", $"Dirección #{i}, Ciudad");
            command.Parameters.AddWithValue("@telefono", $"+57 31{i % 10}{(i * 13) % 10000000:0000000}");
            command.Parameters.AddWithValue("@email", $"cliente{i:000}@demo.com");
            command.ExecuteNonQuery();
        }
    }

    private static void EnsureMinimumProductos(SqliteConnection connection, SqliteTransaction transaction, int minRows, Random random)
    {
        var count = CountRows(connection, transaction, "ProductoExt");
        for (var i = count + 1; i <= minRows; i++)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES(@codigo, @nombre, 'Und', @precio, @stock, @stockMinimo, 1);";
            command.Parameters.AddWithValue("@codigo", $"ACE-DEM-{i:000}");
            command.Parameters.AddWithValue("@nombre", $"Aceite Demo {i:000}");
            command.Parameters.AddWithValue("@precio", random.Next(9000, 90000));
            command.Parameters.AddWithValue("@stock", random.Next(20, 500));
            command.Parameters.AddWithValue("@stockMinimo", random.Next(5, 80));
            command.ExecuteNonQuery();
        }
    }

    private static void EnsureMinimumFacturas(SqliteConnection connection, SqliteTransaction transaction, int minRows, Random random)
    {
        var count = CountRows(connection, transaction, "Factura");
        if (count >= minRows)
        {
            return;
        }

        var clienteIds = GetIds(connection, transaction, "Cliente");
        var metodoIds = GetIds(connection, transaction, "MetodoPago");
        if (clienteIds.Count == 0 || metodoIds.Count == 0)
        {
            return;
        }

        var nextNumber = GetNextInvoiceNumber(connection, transaction);
        for (var i = count + 1; i <= minRows; i++)
        {
            var subtotal = random.Next(50000, 450000);
            var total = subtotal;
            var saldo = Math.Round(total * (decimal)random.NextDouble(), 0);

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO Factura(Numero, Fecha, ClienteId, MetodoPagoId, Estado, Subtotal, Retencion, Total, Saldo, Notas)
VALUES(@numero, @fecha, @clienteId, @metodoPagoId, @estado, @subtotal, 0, @total, @saldo, @notas);";

            command.Parameters.AddWithValue("@numero", $"FAC-{nextNumber:000000}");
            command.Parameters.AddWithValue("@fecha", DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@clienteId", clienteIds[random.Next(clienteIds.Count)]);
            command.Parameters.AddWithValue("@metodoPagoId", metodoIds[random.Next(metodoIds.Count)]);
            command.Parameters.AddWithValue("@estado", saldo == 0 ? "Pagada" : "Enviada");
            command.Parameters.AddWithValue("@subtotal", subtotal);
            command.Parameters.AddWithValue("@total", total);
            command.Parameters.AddWithValue("@saldo", saldo);
            command.Parameters.AddWithValue("@notas", $"Factura demo {i:000}");
            command.ExecuteNonQuery();

            nextNumber++;
        }
    }

    private static void EnsureMinimumItemsFactura(SqliteConnection connection, SqliteTransaction transaction, int minRows, Random random)
    {
        var count = CountRows(connection, transaction, "ItemFactura");
        if (count >= minRows)
        {
            return;
        }

        var facturaIds = GetIds(connection, transaction, "Factura");
        var productoIds = GetIds(connection, transaction, "ProductoExt");
        if (facturaIds.Count == 0 || productoIds.Count == 0)
        {
            return;
        }

        for (var i = count + 1; i <= minRows; i++)
        {
            var cantidad = random.Next(1, 20);
            var precio = random.Next(9000, 90000);
            var totalLinea = cantidad * precio;

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO ItemFactura(FacturaId, ProductoId, Descripcion, Cantidad, PrecioUnitario, TotalLinea)
VALUES(@facturaId, @productoId, @descripcion, @cantidad, @precio, @totalLinea);";
            command.Parameters.AddWithValue("@facturaId", facturaIds[random.Next(facturaIds.Count)]);
            command.Parameters.AddWithValue("@productoId", productoIds[random.Next(productoIds.Count)]);
            command.Parameters.AddWithValue("@descripcion", $"Ítem demo {i:000}");
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@precio", precio);
            command.Parameters.AddWithValue("@totalLinea", totalLinea);
            command.ExecuteNonQuery();
        }
    }

    private static void EnsureMinimumPagos(SqliteConnection connection, SqliteTransaction transaction, int minRows, Random random)
    {
        var count = CountRows(connection, transaction, "Pago");
        if (count >= minRows)
        {
            return;
        }

        var facturaIds = GetIds(connection, transaction, "Factura");
        var metodoIds = GetIds(connection, transaction, "MetodoPago");
        if (facturaIds.Count == 0 || metodoIds.Count == 0)
        {
            return;
        }

        for (var i = count + 1; i <= minRows; i++)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO Pago(FacturaId, Fecha, Valor, MetodoPagoId, Referencia, Notas)
VALUES(@facturaId, @fecha, @valor, @metodoPagoId, @referencia, @notas);";
            command.Parameters.AddWithValue("@facturaId", facturaIds[random.Next(facturaIds.Count)]);
            command.Parameters.AddWithValue("@fecha", DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@valor", random.Next(10000, 250000));
            command.Parameters.AddWithValue("@metodoPagoId", metodoIds[random.Next(metodoIds.Count)]);
            command.Parameters.AddWithValue("@referencia", $"REF-{i:0000}");
            command.Parameters.AddWithValue("@notas", $"Pago demo {i:000}");
            command.ExecuteNonQuery();
        }
    }

    private static void EnsureMinimumMovimientos(SqliteConnection connection, SqliteTransaction transaction, int minRows, Random random)
    {
        var count = CountRows(connection, transaction, "MovimientoInventarioExt");
        if (count >= minRows)
        {
            return;
        }

        var productoIds = GetIds(connection, transaction, "ProductoExt");
        var facturaIds = GetIds(connection, transaction, "Factura");
        if (productoIds.Count == 0)
        {
            return;
        }

        for (var i = count + 1; i <= minRows; i++)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO MovimientoInventarioExt(ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES(@productoId, @fecha, @tipo, @cantidad, @referenciaFacturaId, @nota);";
            command.Parameters.AddWithValue("@productoId", productoIds[random.Next(productoIds.Count)]);
            command.Parameters.AddWithValue("@fecha", DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@tipo", i % 2 == 0 ? "ENTRADA" : "SALIDA");
            command.Parameters.AddWithValue("@cantidad", random.Next(1, 40));

            if (facturaIds.Count > 0 && i % 3 == 0)
            {
                command.Parameters.AddWithValue("@referenciaFacturaId", facturaIds[random.Next(facturaIds.Count)]);
            }
            else
            {
                command.Parameters.AddWithValue("@referenciaFacturaId", DBNull.Value);
            }

            command.Parameters.AddWithValue("@nota", $"Movimiento demo {i:000}");
            command.ExecuteNonQuery();
        }
    }

    private static int CountRows(SqliteConnection connection, SqliteTransaction transaction, string table)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT COUNT(*) FROM {table};";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static List<long> GetIds(SqliteConnection connection, SqliteTransaction transaction, string table)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT Id FROM {table} ORDER BY Id;";

        var result = new List<long>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(reader.GetInt64(0));
        }

        return result;
    }

    private static int GetNextInvoiceNumber(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT IFNULL(MAX(CAST(SUBSTR(Numero, 5) AS INTEGER)), 0) + 1 FROM Factura WHERE Numero LIKE 'FAC-%';";
        return Convert.ToInt32(command.ExecuteScalar());
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
