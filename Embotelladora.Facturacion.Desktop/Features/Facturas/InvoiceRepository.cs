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

    public List<ProductLookupDto> GetProducts()
    {
        var result = new List<ProductLookupDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT Id, Codigo, Nombre, PrecioBase, StockActual
FROM ProductoExt
WHERE Activo = 1
ORDER BY Nombre;";

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

    public List<InvoiceSummaryDto> GetRecentInvoices(int limit = 30)
    {
        var result = new List<InvoiceSummaryDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT f.Id, f.Numero, f.Fecha, c.Nombre, f.Total, f.Saldo, f.Estado
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
ORDER BY date(f.Fecha) DESC, f.Id DESC
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

        if (InvoiceNumberExists(connection, transaction, request.Numero))
        {
            throw new InvalidOperationException("La numeración de factura ya existe.");
        }

        using var invoiceCommand = connection.CreateCommand();
        invoiceCommand.Transaction = transaction;
        invoiceCommand.CommandText = @"INSERT INTO Factura
(Numero, Fecha, ClienteId, MetodoPagoId, Estado, Subtotal, IvaPorcentaje, IvaValor, Retencion, Total, Saldo, Notas)
VALUES
(@numero, @fecha, @clienteId, @metodoPagoId, @estado, @subtotal, @ivaPorcentaje, @ivaValor, @retencion, @total, @saldo, @notas);
SELECT last_insert_rowid();";

        invoiceCommand.Parameters.AddWithValue("@numero", request.Numero.Trim());
        invoiceCommand.Parameters.AddWithValue("@fecha", request.Fecha.ToString("yyyy-MM-dd"));
        invoiceCommand.Parameters.AddWithValue("@clienteId", request.ClienteId);
        invoiceCommand.Parameters.AddWithValue("@metodoPagoId", request.MetodoPagoId.HasValue ? request.MetodoPagoId.Value : DBNull.Value);
        invoiceCommand.Parameters.AddWithValue("@estado", request.Estado);
        invoiceCommand.Parameters.AddWithValue("@subtotal", request.Subtotal);
        invoiceCommand.Parameters.AddWithValue("@ivaPorcentaje", request.IvaPorcentaje);
        invoiceCommand.Parameters.AddWithValue("@ivaValor", request.IvaValor);
        invoiceCommand.Parameters.AddWithValue("@retencion", request.Retencion);
        invoiceCommand.Parameters.AddWithValue("@total", request.Total);
        invoiceCommand.Parameters.AddWithValue("@saldo", request.Saldo);
        invoiceCommand.Parameters.AddWithValue("@notas", request.Notas.Trim());

        var invoiceId = Convert.ToInt64(invoiceCommand.ExecuteScalar());

        foreach (var item in request.Items)
        {
            EnsureStock(connection, transaction, item.ProductId, item.Cantidad);
            ApplyStockOutput(connection, transaction, item.ProductId, item.Cantidad);
            InsertInventoryMovement(connection, transaction, item.ProductId, item.Cantidad, invoiceId);

            using var itemCommand = connection.CreateCommand();
            itemCommand.Transaction = transaction;
            itemCommand.CommandText = @"INSERT INTO ItemFactura
(FacturaId, ProductoId, Descripcion, Cantidad, PrecioUnitario, AplicaIva, TotalLinea)
VALUES
(@facturaId, @productoId, @descripcion, @cantidad, @precioUnitario, @aplicaIva, @totalLinea);";

            itemCommand.Parameters.AddWithValue("@facturaId", invoiceId);
            itemCommand.Parameters.AddWithValue("@productoId", item.ProductId);
            itemCommand.Parameters.AddWithValue("@descripcion", item.Descripcion.Trim());
            itemCommand.Parameters.AddWithValue("@cantidad", item.Cantidad);
            itemCommand.Parameters.AddWithValue("@precioUnitario", item.PrecioUnitario);
            itemCommand.Parameters.AddWithValue("@aplicaIva", item.AplicaIva ? 1 : 0);
            itemCommand.Parameters.AddWithValue("@totalLinea", item.TotalLinea);
            itemCommand.ExecuteNonQuery();
        }

        transaction.Commit();
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
