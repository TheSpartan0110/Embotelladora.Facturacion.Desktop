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

        ExecuteNonQuery(connection, transaction, @"
CREATE TABLE IF NOT EXISTS AppSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL
);");

        SeedDefaults(connection, transaction);
        transaction.Commit();
    }

    private static void SeedDefaults(SqliteConnection connection, SqliteTransaction transaction)
    {
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Efectivo', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Transferencia', 1);");
        ExecuteNonQuery(connection, transaction, "INSERT OR IGNORE INTO MetodoPago(Nombre, Activo) VALUES ('Crédito', 1);");

        SeedDemoData(connection, transaction);
    }

    private static void SeedDemoData(SqliteConnection connection, SqliteTransaction transaction)
    {
        // Check if demo data has already been inserted
        using var checkCmd = connection.CreateCommand();
        checkCmd.Transaction = transaction;
        checkCmd.CommandText = "SELECT Value FROM AppSettings WHERE Key = 'DemoInserted';";
        var result = checkCmd.ExecuteScalar();
        if (result != null && result.ToString() == "1")
            return;

        // --- Productos (10) ---
        var productos = new (string Codigo, string Nombre, string Unidad, double Precio, double StockInicial, double StockMinimo)[]
        {
            ("PRD-001", "Aceite de Girasol 1L",     "Unidad",  8500,  500, 50),
            ("PRD-002", "Aceite de Oliva 500ml",     "Unidad", 18500,  300, 30),
            ("PRD-003", "Aceite de Coco 250ml",      "Unidad", 12000,  200, 20),
            ("PRD-004", "Aceite Vegetal 3L",         "Unidad", 22000,  400, 40),
            ("PRD-005", "Aceite de Canola 1L",       "Unidad",  9500,  350, 35),
            ("PRD-006", "Aceite de Aguacate 250ml",  "Unidad", 15000,  150, 15),
            ("PRD-007", "Aceite de Soya 1L",         "Unidad",  7800,  450, 45),
            ("PRD-008", "Vinagre de Manzana 500ml",  "Unidad",  6500,  250, 25),
            ("PRD-009", "Aceite de Ajonjolí 250ml",  "Unidad", 11000,  180, 18),
            ("PRD-010", "Aceite de Maíz 1L",         "Unidad",  8800,  380, 38),
        };

        var productoIds = new long[productos.Length];
        for (int i = 0; i < productos.Length; i++)
        {
            var p = productos[i];
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT OR IGNORE INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES (@codigo, @nombre, @unidad, @precio, @stock, @stockMin, 1);";
            cmd.Parameters.AddWithValue("@codigo", p.Codigo);
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@unidad", p.Unidad);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.StockInicial);
            cmd.Parameters.AddWithValue("@stockMin", p.StockMinimo);
            cmd.ExecuteNonQuery();

            // Get the id
            using var idCmd = connection.CreateCommand();
            idCmd.Transaction = transaction;
            idCmd.CommandText = "SELECT Id FROM ProductoExt WHERE Codigo = @codigo;";
            idCmd.Parameters.AddWithValue("@codigo", p.Codigo);
            productoIds[i] = Convert.ToInt64(idCmd.ExecuteScalar());

            // Movimiento de inventario: entrada inicial
            using var movCmd = connection.CreateCommand();
            movCmd.Transaction = transaction;
            movCmd.CommandText = @"
INSERT INTO MovimientoInventarioExt(ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES (@pid, @fecha, 'ENTRADA', @cant, NULL, 'Stock inicial de demostración');";
            movCmd.Parameters.AddWithValue("@pid", productoIds[i]);
            movCmd.Parameters.AddWithValue("@fecha", DateTime.Today.AddMonths(-6).ToString("yyyy-MM-dd"));
            movCmd.Parameters.AddWithValue("@cant", p.StockInicial);
            movCmd.ExecuteNonQuery();
        }

        // --- Clientes (10) ---
        var clientes = new (string Codigo, string Nombre, string Nit, string Direccion, string Telefono, string Email)[]
        {
            ("CLI-001", "Distribuidora El Sol S.A.S.",       "900123456-1", "Cra 15 #45-20, Bogotá",      "3101234567", "ventas@distribuidoraelsol.co"),
            ("CLI-002", "Supermercados La Cosecha Ltda.",     "800987654-2", "Cll 72 #10-35, Medellín",    "3209876543", "compras@lacosecha.co"),
            ("CLI-003", "Tiendas Don Julio & Cía.",          "901456789-3", "Av 6N #25-10, Cali",         "3157894561", "pedidos@donjulio.co"),
            ("CLI-004", "Restaurante Sabor Criollo S.A.",    "800654321-4", "Cll 100 #15-80, Barranquilla","3001236547", "admin@saborcriollo.co"),
            ("CLI-005", "Autoservicio Mi Barrio E.U.",       "900741852-5", "Cra 7 #32-15, Bucaramanga",  "3174589632", "info@mibarrio.co"),
            ("CLI-006", "Panadería y Pastelería Aurora",     "901369258-6", "Cll 50 #8-45, Cartagena",    "3123698521", "pedidos@aurora.co"),
            ("CLI-007", "Hotel Plaza Real S.A.S.",           "800258147-7", "Av El Dorado #68-51, Bogotá","3006541239", "compras@plazareal.co"),
            ("CLI-008", "Cadena Express Market S.A.",        "900963852-8", "Cra 43A #1-50, Medellín",    "3148527413", "logistica@expressmarket.co"),
            ("CLI-009", "Alimentos del Campo Ltda.",         "901753951-9", "Cll 23 #5-67, Pereira",      "3167531594", "compras@alimentosdelcampo.co"),
            ("CLI-010", "Comercializadora Andes S.A.S.",     "800159357-0", "Cra 30 #12-40, Manizales",   "3195173582", "ventas@comandes.co"),
        };

        var clienteIds = new long[clientes.Length];
        for (int i = 0; i < clientes.Length; i++)
        {
            var c = clientes[i];
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT OR IGNORE INTO Cliente(Codigo, Nombre, Nit, Direccion, Telefono, Email, Activo)
VALUES (@codigo, @nombre, @nit, @direccion, @telefono, @email, 1);";
            cmd.Parameters.AddWithValue("@codigo", c.Codigo);
            cmd.Parameters.AddWithValue("@nombre", c.Nombre);
            cmd.Parameters.AddWithValue("@nit", c.Nit);
            cmd.Parameters.AddWithValue("@direccion", c.Direccion);
            cmd.Parameters.AddWithValue("@telefono", c.Telefono);
            cmd.Parameters.AddWithValue("@email", c.Email);
            cmd.ExecuteNonQuery();

            // Get the id
            using var idCmd = connection.CreateCommand();
            idCmd.Transaction = transaction;
            idCmd.CommandText = "SELECT Id FROM Cliente WHERE Codigo = @codigo;";
            idCmd.Parameters.AddWithValue("@codigo", c.Codigo);
            clienteIds[i] = Convert.ToInt64(idCmd.ExecuteScalar());
        }

        // --- Facturas: 5 por cliente (50 total) con items, pagos y movimientos ---
        // Distribución de fechas: últimos 5 meses + mes actual
        var hoy = DateTime.Today;
        var rng = new Random(42); // seed fijo para reproducibilidad
        long facturaNum = 0;

        // Definición de cuántos pagos tendrá cada factura del cliente (patron: 0,1,2,1,0 por cliente rotando)
        int[] pagosPorFactura = [0, 1, 2, 1, 0];

        for (int ci = 0; ci < clienteIds.Length; ci++)
        {
            for (int fi = 0; fi < 5; fi++)
            {
                facturaNum++;
                // Fecha: distribuir entre -5 meses y hoy
                var mesesAtras = 5 - (fi + ci % 3);
                if (mesesAtras < 0) mesesAtras = 0;
                var fechaBase = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-mesesAtras);
                var diaMax = DateTime.DaysInMonth(fechaBase.Year, fechaBase.Month);
                var dia = rng.Next(1, diaMax + 1);
                var fechaFactura = new DateTime(fechaBase.Year, fechaBase.Month, dia);
                if (fechaFactura > hoy) fechaFactura = hoy;
                var fechaStr = fechaFactura.ToString("yyyy-MM-dd");

                // Seleccionar 2-4 productos aleatorios para esta factura
                var numItems = rng.Next(2, 5);
                var productosSeleccionados = new HashSet<int>();
                while (productosSeleccionados.Count < numItems)
                    productosSeleccionados.Add(rng.Next(0, productos.Length));

                double subtotal = 0;
                var items = new List<(int prodIdx, double cantidad, double precioUnit, double totalLinea)>();

                foreach (var pi in productosSeleccionados)
                {
                    var cantidad = rng.Next(1, 21);
                    var precioUnit = productos[pi].Precio;
                    var totalLinea = cantidad * precioUnit;
                    subtotal += totalLinea;
                    items.Add((pi, cantidad, precioUnit, totalLinea));
                }

                var retencion = 0.0;
                var total = subtotal - retencion;

                // Determinar pagos
                var numPagos = pagosPorFactura[(ci + fi) % pagosPorFactura.Length];
                double totalPagado = 0;

                if (numPagos == 2)
                    totalPagado = total; // pagada completa
                else if (numPagos == 1)
                    totalPagado = Math.Round(total * (0.4 + rng.NextDouble() * 0.3), 0); // pago parcial 40%-70%

                var saldo = total - totalPagado;
                var estado = saldo <= 0 ? "Pagada" : "Pendiente";
                var metodoPagoId = rng.Next(1, 4); // 1=Efectivo, 2=Transferencia, 3=Crédito

                // Insertar factura
                long facturaId;
                {
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
INSERT OR IGNORE INTO Factura(Numero, Fecha, ClienteId, MetodoPagoId, Estado, Subtotal, Retencion, Total, Saldo, Notas)
VALUES (@numero, @fecha, @clienteId, @metodoPagoId, @estado, @subtotal, @retencion, @total, @saldo, @notas);";
                    cmd.Parameters.AddWithValue("@numero", $"FAC-{facturaNum:000000}");
                    cmd.Parameters.AddWithValue("@fecha", fechaStr);
                    cmd.Parameters.AddWithValue("@clienteId", clienteIds[ci]);
                    cmd.Parameters.AddWithValue("@metodoPagoId", metodoPagoId);
                    cmd.Parameters.AddWithValue("@estado", estado);
                    cmd.Parameters.AddWithValue("@subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@retencion", retencion);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@saldo", saldo);
                    cmd.Parameters.AddWithValue("@notas", "");
                    cmd.ExecuteNonQuery();

                    // Get the id
                    using var idCmd = connection.CreateCommand();
                    idCmd.Transaction = transaction;
                    idCmd.CommandText = "SELECT Id FROM Factura WHERE Numero = @numero;";
                    idCmd.Parameters.AddWithValue("@numero", $"FAC-{facturaNum:000000}");
                    facturaId = Convert.ToInt64(idCmd.ExecuteScalar());
                }

                // Insertar items y movimientos de inventario
                foreach (var (prodIdx, cantidad, precioUnit, totalLinea) in items)
                {
                    using var itemCmd = connection.CreateCommand();
                    itemCmd.Transaction = transaction;
                    itemCmd.CommandText = @"
INSERT INTO ItemFactura(FacturaId, ProductoId, Descripcion, Cantidad, PrecioUnitario, TotalLinea)
VALUES (@facturaId, @productoId, @descripcion, @cantidad, @precioUnitario, @totalLinea);";
                    itemCmd.Parameters.AddWithValue("@facturaId", facturaId);
                    itemCmd.Parameters.AddWithValue("@productoId", productoIds[prodIdx]);
                    itemCmd.Parameters.AddWithValue("@descripcion", productos[prodIdx].Nombre);
                    itemCmd.Parameters.AddWithValue("@cantidad", cantidad);
                    itemCmd.Parameters.AddWithValue("@precioUnitario", precioUnit);
                    itemCmd.Parameters.AddWithValue("@totalLinea", totalLinea);
                    itemCmd.ExecuteNonQuery();

                    // Salida de inventario
                    using var movCmd = connection.CreateCommand();
                    movCmd.Transaction = transaction;
                    movCmd.CommandText = @"
INSERT INTO MovimientoInventarioExt(ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES (@pid, @fecha, 'SALIDA', @cant, @fid, 'Salida automática por facturación');";
                    movCmd.Parameters.AddWithValue("@pid", productoIds[prodIdx]);
                    movCmd.Parameters.AddWithValue("@fecha", fechaStr);
                    movCmd.Parameters.AddWithValue("@cant", cantidad);
                    movCmd.Parameters.AddWithValue("@fid", facturaId);
                    movCmd.ExecuteNonQuery();

                    // Descontar stock
                    using var stockCmd = connection.CreateCommand();
                    stockCmd.Transaction = transaction;
                    stockCmd.CommandText = "UPDATE ProductoExt SET StockActual = StockActual - @cant WHERE Id = @pid;";
                    stockCmd.Parameters.AddWithValue("@cant", cantidad);
                    stockCmd.Parameters.AddWithValue("@pid", productoIds[prodIdx]);
                    stockCmd.ExecuteNonQuery();
                }

                // Insertar pagos
                if (numPagos == 1)
                {
                    var fechaPago = fechaFactura.AddDays(rng.Next(1, 15));
                    if (fechaPago > hoy) fechaPago = hoy;

                    using var pagoCmd = connection.CreateCommand();
                    pagoCmd.Transaction = transaction;
                    pagoCmd.CommandText = @"
INSERT INTO Pago(FacturaId, Fecha, Valor, MetodoPagoId, Referencia, Notas)
VALUES (@fid, @fecha, @valor, @mpId, @ref, @notas);";
                    pagoCmd.Parameters.AddWithValue("@fid", facturaId);
                    pagoCmd.Parameters.AddWithValue("@fecha", fechaPago.ToString("yyyy-MM-dd"));
                    pagoCmd.Parameters.AddWithValue("@valor", totalPagado);
                    pagoCmd.Parameters.AddWithValue("@mpId", rng.Next(1, 3));
                    pagoCmd.Parameters.AddWithValue("@ref", $"REF-{facturaNum:0000}-1");
                    pagoCmd.Parameters.AddWithValue("@notas", "Abono parcial");
                    pagoCmd.ExecuteNonQuery();
                }
                else if (numPagos == 2)
                {
                    var pago1 = Math.Round(total * 0.5, 0);
                    var pago2 = total - pago1;

                    var fechaPago1 = fechaFactura.AddDays(rng.Next(1, 10));
                    if (fechaPago1 > hoy) fechaPago1 = hoy;
                    var fechaPago2 = fechaPago1.AddDays(rng.Next(3, 15));
                    if (fechaPago2 > hoy) fechaPago2 = hoy;

                    using var pago1Cmd = connection.CreateCommand();
                    pago1Cmd.Transaction = transaction;
                    pago1Cmd.CommandText = @"
INSERT INTO Pago(FacturaId, Fecha, Valor, MetodoPagoId, Referencia, Notas)
VALUES (@fid, @fecha, @valor, @mpId, @ref, @notas);";
                    pago1Cmd.Parameters.AddWithValue("@fid", facturaId);
                    pago1Cmd.Parameters.AddWithValue("@fecha", fechaPago1.ToString("yyyy-MM-dd"));
                    pago1Cmd.Parameters.AddWithValue("@valor", pago1);
                    pago1Cmd.Parameters.AddWithValue("@mpId", 1);
                    pago1Cmd.Parameters.AddWithValue("@ref", $"REF-{facturaNum:0000}-1");
                    pago1Cmd.Parameters.AddWithValue("@notas", "Primer abono");
                    pago1Cmd.ExecuteNonQuery();

                    using var pago2Cmd = connection.CreateCommand();
                    pago2Cmd.Transaction = transaction;
                    pago2Cmd.CommandText = @"
INSERT INTO Pago(FacturaId, Fecha, Valor, MetodoPagoId, Referencia, Notas)
VALUES (@fid, @fecha, @valor, @mpId, @ref, @notas);";
                    pago2Cmd.Parameters.AddWithValue("@fid", facturaId);
                    pago2Cmd.Parameters.AddWithValue("@fecha", fechaPago2.ToString("yyyy-MM-dd"));
                    pago2Cmd.Parameters.AddWithValue("@valor", pago2);
                    pago2Cmd.Parameters.AddWithValue("@mpId", 2);
                    pago2Cmd.Parameters.AddWithValue("@ref", $"REF-{facturaNum:0000}-2");
                    pago2Cmd.Parameters.AddWithValue("@notas", "Pago final");
                    pago2Cmd.ExecuteNonQuery();
                }
            }
        }

        // Mark demo data as inserted
        using var flagCmd = connection.CreateCommand();
        flagCmd.Transaction = transaction;
        flagCmd.CommandText = "INSERT OR REPLACE INTO AppSettings(Key, Value) VALUES ('DemoInserted', '1');";
        flagCmd.ExecuteNonQuery();
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
