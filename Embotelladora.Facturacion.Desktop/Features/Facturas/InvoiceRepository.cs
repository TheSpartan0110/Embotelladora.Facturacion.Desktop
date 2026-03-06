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

    public decimal GetIvaRate()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT IFNULL(Valor, 0.19) FROM ParametroIva WHERE Nombre = 'IVA_GENERAL' AND Activo = 1 LIMIT 1;";
        var result = command.ExecuteScalar();

        return Convert.ToDecimal(result);
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
            sql += " AND (Nombre LIKE @search OR Codigo LIKE @search OR Categoria LIKE @search)";
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }
        
        sql += " ORDER BY Nombre;";
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
        var sql = @"SELECT f.Id, f.Numero, f.Fecha, c.Nombre, f.Estado, f.Total
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

        sql += " ORDER BY f.Fecha DESC, f.Id DESC;";
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

    public List<InvoiceSummaryDto> GetRecentInvoices(int limit = 30)
    {
        var result = new List<InvoiceSummaryDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT f.Id, f.Numero, f.Fecha, c.Nombre, f.Total, f.Saldo, f.Estado
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
ORDER BY f.Fecha DESC, f.Id DESC
LIMIT @limit;";
        command.Parameters.AddWithValue("@limit", limit);

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
INSERT INTO Factura(Numero, Fecha, ClienteId, MetodoPagoId, Estado, Subtotal, IvaPorcentaje, IvaValor, Retencion, Total, Saldo, Notas)
VALUES(@numero, @fecha, @clienteId, @metodoPagoId, @estado, @subtotal, @ivaPorcentaje, @ivaValor, @retencion, @total, @saldo, @notas);
SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@numero", request.Numero);
        command.Parameters.AddWithValue("@fecha", request.Fecha.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@clienteId", request.ClienteId);
        command.Parameters.AddWithValue("@metodoPagoId", request.MetodoPagoId);
        command.Parameters.AddWithValue("@estado", request.Estado);
        command.Parameters.AddWithValue("@subtotal", request.Subtotal);
        command.Parameters.AddWithValue("@ivaPorcentaje", request.IvaPorcentaje);
        command.Parameters.AddWithValue("@ivaValor", request.IvaValor);
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
INSERT INTO ItemFactura(FacturaId, ProductoId, Descripcion, Cantidad, PrecioUnitario, AplicaIva, TotalLinea)
VALUES(@facturaId, @productoId, @descripcion, @cantidad, @precioUnitario, @aplicaIva, @totalLinea);";

            itemCommand.Parameters.AddWithValue("@facturaId", invoiceId);
            itemCommand.Parameters.AddWithValue("@productoId", item.ProductId);
            itemCommand.Parameters.AddWithValue("@descripcion", item.Descripcion);
            itemCommand.Parameters.AddWithValue("@cantidad", item.Cantidad);
            itemCommand.Parameters.AddWithValue("@precioUnitario", item.PrecioUnitario);
            itemCommand.Parameters.AddWithValue("@aplicaIva", item.AplicaIva ? 1 : 0);
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
        itemCommand.CommandText = @"SELECT Id, Descripcion, Cantidad, Precio, Iva 
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
                Precio = Convert.ToDecimal(itemReader.GetDouble(3)),
                Iva = Convert.ToDecimal(itemReader.GetDouble(4))
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
        var total = invoice.Items.Sum(i => i.Subtotal + i.Iva);
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
            itemCommand.CommandText = @"INSERT INTO FacturaDetalle (FacturaId, Descripcion, Cantidad, Precio, Iva)
VALUES (@facturaId, @descripcion, @cantidad, @precio, @iva);";

            itemCommand.Parameters.AddWithValue("@facturaId", invoiceId);
            itemCommand.Parameters.AddWithValue("@descripcion", item.Descripcion);
            itemCommand.Parameters.AddWithValue("@cantidad", item.Cantidad);
            itemCommand.Parameters.AddWithValue("@precio", item.Precio);
            itemCommand.Parameters.AddWithValue("@iva", item.Iva);

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
