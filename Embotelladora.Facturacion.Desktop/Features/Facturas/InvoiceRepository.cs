using Embotelladora.Facturacion.Desktop.Data;
using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class InvoiceRepository
{
    public string GenerateNextInvoiceNumber()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT IFNULL(MAX(CAST(SUBSTR(Numero, 5) AS INTEGER)), 0) + 1
FROM Factura
WHERE Numero LIKE 'FAC-%';";

        var next = Convert.ToInt32(command.ExecuteScalar());
        return $"FAC-{next:000000}";
    }

    public bool InvoiceNumberExists(string invoiceNumber)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();
        return InvoiceNumberExists(connection, null, invoiceNumber);
    }

    public List<InvoiceCustomerLookupDto> GetActiveCustomers()
    {
        var result = new List<InvoiceCustomerLookupDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Nit FROM Cliente WHERE Activo = 1 ORDER BY Nombre;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new InvoiceCustomerLookupDto
            {
                Id = reader.GetInt64(0),
                Nombre = reader.GetString(1),
                Nit = reader.GetString(2)
            });
        }

        return result;
    }

    public List<PaymentMethodLookupDto> GetPaymentMethods()
    {
        var result = new List<PaymentMethodLookupDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre FROM MetodoPago WHERE Activo = 1 ORDER BY Nombre;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new PaymentMethodLookupDto
            {
                Id = reader.GetInt64(0),
                Nombre = reader.GetString(1)
            });
        }

        return result;
    }

    public List<ProductLookupDto> GetProducts(string? search = null)
    {
        var result = new List<ProductLookupDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var sql = @"SELECT Id, Codigo, Nombre, PrecioBase, StockActual FROM ProductoExt WHERE Activo = 1";
        
        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (Nombre LIKE @search OR Codigo LIKE @search)";
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }
        
        sql += " ORDER BY Codigo;";
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ProductLookupDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                PrecioBase = Convert.ToDecimal(reader.GetDouble(3)),
                StockActual = Convert.ToDecimal(reader.GetDouble(4))
            });
        }

        return result;
    }

    public List<InvoiceGridRowDto> GetGridRows(string? search = null, string? estado = null)
    {
        var result = new List<InvoiceGridRowDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var sql = @"SELECT f.Id,
       f.Numero,
       f.Fecha,
       c.Nombre,
       CASE
           WHEN f.Estado = 'Anulada' THEN 'Anulada'
           WHEN f.Saldo <= 0 THEN 'Pagada'
           WHEN f.Saldo < f.Total THEN 'Parcial'
           WHEN f.Estado = 'Vencida' THEN 'Vencida'
           ELSE 'Enviada'
       END,
       f.Total
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
WHERE 1=1";

        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (f.Numero LIKE @search OR c.Nombre LIKE @search)";
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }

        if (!string.IsNullOrEmpty(estado) && estado != "Todos los estados")
        {
            sql += " AND f.Estado = @estado";
            command.Parameters.AddWithValue("@estado", estado);
        }

        sql += " ORDER BY f.Numero;";
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new InvoiceGridRowDto
            {
                Id = reader.GetInt64(0),
                Numero = reader.GetString(1),
                Fecha = DateTime.Parse(reader.GetString(2)),
                Cliente = reader.GetString(3),
                Estado = reader.GetString(4),
                Total = Convert.ToDecimal(reader.GetDouble(5))
            });
        }

        return result;
    }

    public List<InvoiceGridRowDto> GetRows()
    {
        return GetGridRows();
    }

    public List<InvoiceSummaryDto> GetRecentInvoices(int limit = 100, string? search = null, string? estado = null)
    {
        var result = new List<InvoiceSummaryDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var sql = @"
SELECT f.Id,
       f.Numero,
       f.Fecha,
       c.Nombre,
       f.Total,
       f.Saldo,
       CASE
           WHEN f.Estado = 'Anulada' THEN 'Anulada'
           WHEN f.Saldo <= 0 THEN 'Pagada'
           WHEN f.Saldo < f.Total THEN 'Parcial'
           WHEN f.Estado = 'Vencida' THEN 'Vencida'
           ELSE 'Enviada'
       END
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
WHERE 1=1";

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (f.Numero LIKE @search OR c.Nombre LIKE @search)";
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }

        if (!string.IsNullOrEmpty(estado) && estado != "Todos los estados")
        {
            sql += " AND f.Estado = @estado";
            command.Parameters.AddWithValue("@estado", estado);
        }

        sql += " ORDER BY f.Id DESC LIMIT @limit;";
        command.Parameters.AddWithValue("@limit", limit);
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new InvoiceSummaryDto
            {
                Id = reader.GetInt64(0),
                Numero = reader.GetString(1),
                Fecha = reader.GetString(2),
                Cliente = reader.GetString(3),
                Total = Convert.ToDecimal(reader.GetDouble(4)),
                Saldo = Convert.ToDecimal(reader.GetDouble(5)),
                Estado = reader.GetString(6)
            });
        }

        return result;
    }

    public InvoiceListResumenDto GetListResumen()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    COUNT(*),
    IFNULL(SUM(Total), 0),
    IFNULL(SUM(CASE WHEN Saldo > 0 THEN Saldo ELSE 0 END), 0),
    IFNULL(SUM(CASE
        WHEN Estado <> 'Anulada' AND Saldo <= 0 THEN 1
        ELSE 0
    END), 0)
FROM Factura;";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new InvoiceListResumenDto
            {
                TotalFacturas = Convert.ToInt32(reader.GetValue(0)),
                TotalFacturado = Convert.ToDecimal(reader.GetValue(1)),
                SaldoPendiente = Convert.ToDecimal(reader.GetValue(2)),
                FacturasPagadas = Convert.ToInt32(reader.GetValue(3))
            };
        }

        return new InvoiceListResumenDto();
    }

    public string GetInvoiceDetailText(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT f.Numero,
       f.Fecha,
       c.Nombre,
       f.Total,
       f.Saldo,
       CASE
           WHEN f.Estado = 'Anulada' THEN 'Anulada'
           WHEN f.Saldo <= 0 THEN 'Pagada'
           WHEN f.Saldo < f.Total THEN 'Parcial'
           WHEN f.Estado = 'Vencida' THEN 'Vencida'
           ELSE 'Enviada'
       END,
       f.Notas,
       IFNULL(mp.Nombre, '-') AS MetodoPago, f.Subtotal
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
LEFT JOIN MetodoPago mp ON mp.Id = f.MetodoPagoId
WHERE f.Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return "Factura no encontrada.";

        var numero = reader.GetString(0);
        var fecha = reader.GetString(1);
        var cliente = reader.GetString(2);
        var total = Convert.ToDecimal(reader.GetDouble(3));
        var saldo = Convert.ToDecimal(reader.GetDouble(4));
        var estado = reader.GetString(5);
        var notas = reader.IsDBNull(6) ? "" : reader.GetString(6);
        var metodoPago = reader.GetString(7);
        var subtotal = Convert.ToDecimal(reader.GetDouble(8));
        reader.Close();

        using var itemsCmd = connection.CreateCommand();
        itemsCmd.CommandText = "SELECT Descripcion, Cantidad, PrecioUnitario, TotalLinea FROM ItemFactura WHERE FacturaId = @id;";
        itemsCmd.Parameters.AddWithValue("@id", id);

        var itemsText = "";
        using (var itemReader = itemsCmd.ExecuteReader())
        {
            var idx = 1;
            while (itemReader.Read())
            {
                var desc = itemReader.GetString(0);
                var cant = Convert.ToDecimal(itemReader.GetDouble(1));
                var precio = Convert.ToDecimal(itemReader.GetDouble(2));
                var totalLinea = Convert.ToDecimal(itemReader.GetDouble(3));
                itemsText += $"  {idx}. {desc} — {cant:N0} x {precio:C0} = {totalLinea:C0}\n";
                idx++;
            }
        }

        using var paymentsCmd = connection.CreateCommand();
        paymentsCmd.CommandText = "SELECT Fecha, Valor, IFNULL(Referencia, '-') FROM Pago WHERE FacturaId = @id ORDER BY Fecha;";
        paymentsCmd.Parameters.AddWithValue("@id", id);

        var paymentsText = "";
        using (var payReader = paymentsCmd.ExecuteReader())
        {
            while (payReader.Read())
            {
                var pFecha = payReader.GetString(0);
                var pValor = Convert.ToDecimal(payReader.GetDouble(1));
                var pRef = payReader.GetString(2);
                paymentsText += $"  • {pFecha} — {pValor:C0} ({pRef})\n";
            }
        }

        var estadoSaldo = saldo < 0 ? $"A favor: {Math.Abs(saldo):C0}" : saldo == 0 ? "Pagado" : $"Pendiente: {saldo:C0}";

        var result = $"Factura: {numero}\n" +
                     $"Fecha: {fecha}\n" +
                     $"Cliente: {cliente}\n" +
                     $"Método de Pago: {metodoPago}\n" +
                     $"Estado: {estado}\n\n" +
                     $"─── Productos ───\n" +
                     (string.IsNullOrEmpty(itemsText) ? "  (sin productos)\n" : itemsText) +
                     $"\nSubtotal: {subtotal:C0}\n" +
                     $"Total: {total:C0}\n" +
                     $"Saldo: {estadoSaldo}\n";

        if (!string.IsNullOrEmpty(paymentsText))
            result += $"\n─── Pagos ───\n{paymentsText}";

        if (!string.IsNullOrWhiteSpace(notas))
            result += $"\n─── Notas ───\n  {notas}\n";

        return result;
    }

    public InvoicePrintDetailDto? GetInvoicePrintDetail(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT f.Id,
       f.Numero,
       f.Fecha,
       IFNULL(c.Nombre, ''),
       IFNULL(c.Nit, ''),
       IFNULL(c.Direccion, ''),
       IFNULL(mp.Nombre, '-'),
       CASE
           WHEN f.Estado = 'Anulada' THEN 'Anulada'
           WHEN f.Saldo <= 0 THEN 'Pagada'
           WHEN f.Saldo < f.Total THEN 'Parcial'
           WHEN f.Estado = 'Vencida' THEN 'Vencida'
           ELSE 'Enviada'
       END,
       IFNULL(f.Subtotal, 0),
       IFNULL(f.Retencion, 0),
       IFNULL(f.Total, 0),
       IFNULL(f.Saldo, 0),
       IFNULL(f.Notas, '')
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
LEFT JOIN MetodoPago mp ON mp.Id = f.MetodoPagoId
WHERE f.Id = @id;";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var detail = new InvoicePrintDetailDto
        {
            Id = reader.GetInt64(0),
            Numero = reader.GetString(1),
            Fecha = DateTime.TryParse(reader.GetString(2), out var fecha) ? fecha : DateTime.Today,
            Cliente = reader.GetString(3),
            Nit = reader.GetString(4),
            Direccion = reader.GetString(5),
            MetodoPago = reader.GetString(6),
            Estado = reader.GetString(7),
            Subtotal = Convert.ToDecimal(reader.GetValue(8)),
            Retencion = Convert.ToDecimal(reader.GetValue(9)),
            Total = Convert.ToDecimal(reader.GetValue(10)),
            Saldo = Convert.ToDecimal(reader.GetValue(11)),
            Notas = reader.GetString(12)
        };
        reader.Close();

        using var itemsCommand = connection.CreateCommand();
        itemsCommand.CommandText = @"
SELECT IFNULL(p.Codigo, ''),
       IFNULL(i.Descripcion, ''),
       IFNULL(i.Cantidad, 0),
       IFNULL(i.PrecioUnitario, 0),
       IFNULL(i.TotalLinea, 0)
FROM ItemFactura i
LEFT JOIN ProductoExt p ON p.Id = i.ProductoId
WHERE i.FacturaId = @id
ORDER BY i.Id;";
        itemsCommand.Parameters.AddWithValue("@id", id);

        using var itemReader = itemsCommand.ExecuteReader();
        while (itemReader.Read())
        {
            detail.Items.Add(new InvoicePrintItemDto
            {
                Codigo = itemReader.GetString(0),
                Descripcion = itemReader.GetString(1),
                Cantidad = Convert.ToDecimal(itemReader.GetValue(2)),
                PrecioUnitario = Convert.ToDecimal(itemReader.GetValue(3)),
                TotalLinea = Convert.ToDecimal(itemReader.GetValue(4))
            });
        }

        return detail;
    }

    public void VoidInvoice(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using var checkCmd = connection.CreateCommand();
        checkCmd.Transaction = transaction;
        checkCmd.CommandText = "SELECT Estado FROM Factura WHERE Id = @id;";
        checkCmd.Parameters.AddWithValue("@id", id);
        var estado = checkCmd.ExecuteScalar()?.ToString();
        if (estado == "Anulada")
            throw new InvalidOperationException("La factura ya está anulada.");

        using var itemsCmd = connection.CreateCommand();
        itemsCmd.Transaction = transaction;
        itemsCmd.CommandText = "SELECT ProductoId, Cantidad FROM ItemFactura WHERE FacturaId = @id AND ProductoId IS NOT NULL;";
        itemsCmd.Parameters.AddWithValue("@id", id);

        var items = new List<(long ProductoId, decimal Cantidad)>();
        using (var reader = itemsCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                items.Add((reader.GetInt64(0), Convert.ToDecimal(reader.GetDouble(1))));
            }
        }

        foreach (var (productoId, cantidad) in items)
        {
            using var stockCmd = connection.CreateCommand();
            stockCmd.Transaction = transaction;
            stockCmd.CommandText = "UPDATE ProductoExt SET StockActual = StockActual + @cantidad WHERE Id = @id;";
            stockCmd.Parameters.AddWithValue("@cantidad", cantidad);
            stockCmd.Parameters.AddWithValue("@id", productoId);
            stockCmd.ExecuteNonQuery();
        }

        using var updateCmd = connection.CreateCommand();
        updateCmd.Transaction = transaction;
        updateCmd.CommandText = "UPDATE Factura SET Estado = 'Anulada', Saldo = 0 WHERE Id = @id;";
        updateCmd.Parameters.AddWithValue("@id", id);
        updateCmd.ExecuteNonQuery();

        transaction.Commit();
    }

    public void DeleteInvoice(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using var checkCmd = connection.CreateCommand();
        checkCmd.Transaction = transaction;
        checkCmd.CommandText = "SELECT Estado FROM Factura WHERE Id = @id;";
        checkCmd.Parameters.AddWithValue("@id", id);
        var estado = checkCmd.ExecuteScalar()?.ToString();

        if (estado != "Anulada")
        {
            using var itemsCmd = connection.CreateCommand();
            itemsCmd.Transaction = transaction;
            itemsCmd.CommandText = "SELECT ProductoId, Cantidad FROM ItemFactura WHERE FacturaId = @id AND ProductoId IS NOT NULL;";
            itemsCmd.Parameters.AddWithValue("@id", id);

            var items = new List<(long ProductoId, decimal Cantidad)>();
            using (var reader = itemsCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    items.Add((reader.GetInt64(0), Convert.ToDecimal(reader.GetDouble(1))));
                }
            }

            foreach (var (productoId, cantidad) in items)
            {
                using var stockCmd = connection.CreateCommand();
                stockCmd.Transaction = transaction;
                stockCmd.CommandText = "UPDATE ProductoExt SET StockActual = StockActual + @cantidad WHERE Id = @id;";
                stockCmd.Parameters.AddWithValue("@cantidad", cantidad);
                stockCmd.Parameters.AddWithValue("@id", productoId);
                stockCmd.ExecuteNonQuery();
            }
        }

        using var movCmd = connection.CreateCommand();
        movCmd.Transaction = transaction;
        movCmd.CommandText = "DELETE FROM MovimientoInventarioExt WHERE ReferenciaFacturaId = @id;";
        movCmd.Parameters.AddWithValue("@id", id);
        movCmd.ExecuteNonQuery();

        using var payCmd = connection.CreateCommand();
        payCmd.Transaction = transaction;
        payCmd.CommandText = "DELETE FROM Pago WHERE FacturaId = @id;";
        payCmd.Parameters.AddWithValue("@id", id);
        payCmd.ExecuteNonQuery();

        using var delItemsCmd = connection.CreateCommand();
        delItemsCmd.Transaction = transaction;
        delItemsCmd.CommandText = "DELETE FROM ItemFactura WHERE FacturaId = @id;";
        delItemsCmd.Parameters.AddWithValue("@id", id);
        delItemsCmd.ExecuteNonQuery();

        using var delInvCmd = connection.CreateCommand();
        delInvCmd.Transaction = transaction;
        delInvCmd.CommandText = "DELETE FROM Factura WHERE Id = @id;";
        delInvCmd.Parameters.AddWithValue("@id", id);
        delInvCmd.ExecuteNonQuery();

        transaction.Commit();
    }

    public void CreateInvoice(InvoiceCreateRequest request)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // Verificar stock antes de procesar
        foreach (var item in request.Items)
        {
            EnsureStock(connection, transaction, item.ProductId, item.Cantidad);
        }

        // Insertar factura
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
INSERT INTO Factura(Numero, Fecha, ClienteId, MetodoPagoId, Estado, Subtotal, Retencion, Total, Saldo, Notas)
VALUES(@numero, @fecha, @clienteId, @metodoPagoId, @estado, @subtotal, @retencion, @total, @saldo, @notas);
SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@numero", request.Numero);
        command.Parameters.AddWithValue("@fecha", request.Fecha.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@clienteId", request.ClienteId);
        command.Parameters.AddWithValue("@metodoPagoId", request.MetodoPagoId);
        command.Parameters.AddWithValue("@estado", request.Estado);
        command.Parameters.AddWithValue("@subtotal", request.Subtotal);
        command.Parameters.AddWithValue("@retencion", request.Retencion);
        command.Parameters.AddWithValue("@total", request.Total);
        command.Parameters.AddWithValue("@saldo", request.Saldo);
        command.Parameters.AddWithValue("@notas", request.Notas);

        var invoiceId = Convert.ToInt64(command.ExecuteScalar());

        // Insertar ítems
        foreach (var item in request.Items)
        {
            using var itemCommand = connection.CreateCommand();
            itemCommand.Transaction = transaction;
            itemCommand.CommandText = @"
INSERT INTO ItemFactura(FacturaId, ProductoId, Descripcion, Cantidad, PrecioUnitario, TotalLinea)
VALUES(@facturaId, @productoId, @descripcion, @cantidad, @precioUnitario, @totalLinea);";

            itemCommand.Parameters.AddWithValue("@facturaId", invoiceId);
            itemCommand.Parameters.AddWithValue("@productoId", item.ProductId);
            itemCommand.Parameters.AddWithValue("@descripcion", item.Descripcion);
            itemCommand.Parameters.AddWithValue("@cantidad", item.Cantidad);
            itemCommand.Parameters.AddWithValue("@precioUnitario", item.PrecioUnitario);
            itemCommand.Parameters.AddWithValue("@totalLinea", item.TotalLinea);
            itemCommand.ExecuteNonQuery();

            // Aplicar salida de stock
            ApplyStockOutput(connection, transaction, item.ProductId, item.Cantidad);

            // Registrar movimiento de inventario
            InsertInventoryMovement(connection, transaction, item.ProductId, item.Cantidad, invoiceId);
        }

        transaction.Commit();
    }

    public Invoice? GetById(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT Id, ClienteId, MetodoPagoId, Fecha, Notas 
FROM Factura WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        var invoice = new Invoice
        {
            Id = reader.GetInt64(0),
            ClienteId = reader.GetInt64(1),
            MetodoPagoId = reader.GetInt64(2),
            Fecha = DateTime.Parse(reader.GetString(3)),
            Notas = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Items = []
        };

        reader.Close();

        // Get items
        using var itemCommand = connection.CreateCommand();
        itemCommand.CommandText = @"SELECT Id, Descripcion, Cantidad, Precio 
FROM FacturaDetalle WHERE FacturaId = @id;";
        itemCommand.Parameters.AddWithValue("@id", id);

        using var itemReader = itemCommand.ExecuteReader();
        while (itemReader.Read())
        {
            invoice.Items.Add(new InvoiceItemDetail
            {
                Id = itemReader.GetInt64(0),
                Descripcion = itemReader.GetString(1),
                Cantidad = Convert.ToDecimal(itemReader.GetDouble(2)),
                Precio = Convert.ToDecimal(itemReader.GetDouble(3))
            });
        }

        return invoice;
    }

    public void Add(Invoice invoice)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var numero = GenerateNextInvoiceNumber();
        var fecha = invoice.Fecha.ToString("yyyy-MM-dd");
        var total = invoice.Items.Sum(i => i.Subtotal);
        var saldo = total;

        command.CommandText = @"INSERT INTO Factura (Numero, ClienteId, MetodoPagoId, Fecha, Total, Saldo, Estado, Notas)
VALUES (@numero, @clienteId, @metodoPagoId, @fecha, @total, @saldo, 'Enviada', @notas);";

        command.Parameters.AddWithValue("@numero", numero);
        command.Parameters.AddWithValue("@clienteId", invoice.ClienteId);
        command.Parameters.AddWithValue("@metodoPagoId", invoice.MetodoPagoId);
        command.Parameters.AddWithValue("@fecha", fecha);
        command.Parameters.AddWithValue("@total", total);
        command.Parameters.AddWithValue("@saldo", saldo);
        command.Parameters.AddWithValue("@notas", invoice.Notas ?? string.Empty);

        command.ExecuteNonQuery();

        // Get invoice ID
        using var getIdCommand = connection.CreateCommand();
        getIdCommand.CommandText = "SELECT last_insert_rowid();";
        var invoiceId = Convert.ToInt64(getIdCommand.ExecuteScalar());

        // Insert items
        foreach (var item in invoice.Items)
        {
            using var itemCommand = connection.CreateCommand();
            itemCommand.CommandText = @"INSERT INTO FacturaDetalle (FacturaId, Descripcion, Cantidad, Precio)
VALUES (@facturaId, @descripcion, @cantidad, @precio);";

            itemCommand.Parameters.AddWithValue("@facturaId", invoiceId);
            itemCommand.Parameters.AddWithValue("@descripcion", item.Descripcion);
            itemCommand.Parameters.AddWithValue("@cantidad", item.Cantidad);
            itemCommand.Parameters.AddWithValue("@precio", item.Precio);

            itemCommand.ExecuteNonQuery();
        }
    }

    private static bool InvoiceNumberExists(SqliteConnection connection, SqliteTransaction? transaction, string invoiceNumber)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(1) FROM Factura WHERE Numero = @numero;";
        command.Parameters.AddWithValue("@numero", invoiceNumber.Trim());
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void EnsureStock(SqliteConnection connection, SqliteTransaction transaction, long productId, decimal quantity)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT Nombre, StockActual FROM ProductoExt WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", productId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException("Producto no encontrado para uno de los ítems.");
        }

        var productName = reader.GetString(0);
        var currentStock = Convert.ToDecimal(reader.GetDouble(1));

        if (currentStock < quantity)
        {
            throw new InvalidOperationException($"Stock insuficiente para {productName}. Disponible: {currentStock:N2}");
        }
    }

    private static void ApplyStockOutput(SqliteConnection connection, SqliteTransaction transaction, long productId, decimal quantity)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"UPDATE ProductoExt
SET StockActual = StockActual - @cantidad
WHERE Id = @id;";
        command.Parameters.AddWithValue("@cantidad", quantity);
        command.Parameters.AddWithValue("@id", productId);
        command.ExecuteNonQuery();
    }

    private static void InsertInventoryMovement(SqliteConnection connection, SqliteTransaction transaction, long productId, decimal quantity, long invoiceId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"INSERT INTO MovimientoInventarioExt
(ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES
(@productoId, @fecha, 'SALIDA', @cantidad, @facturaId, 'Salida automática por facturación');";

        command.Parameters.AddWithValue("@productoId", productId);
        command.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@cantidad", quantity);
        command.Parameters.AddWithValue("@facturaId", invoiceId);
        command.ExecuteNonQuery();
    }
}
