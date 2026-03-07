using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class PaymentRepository
{
    public List<PaymentLookupDto> GetPendingInvoices()
    {
        var result = new List<PaymentLookupDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT f.Id, f.Numero, c.Nombre, f.Saldo
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
WHERE f.Saldo > 0 AND f.Estado <> 'Anulada'
ORDER BY f.Fecha DESC, f.Numero DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new PaymentLookupDto
            {
                Id = reader.GetInt64(0),
                Numero = reader.GetString(1),
                Cliente = reader.GetString(2),
                Saldo = Convert.ToDecimal(reader.GetDouble(3))
            });
        }

        return result;
    }

    public int GetPaymentCount(long facturaId)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Pago WHERE FacturaId = @facturaId;";
        command.Parameters.AddWithValue("@facturaId", facturaId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public PaymentResumenDto GetResumen()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    COUNT(*) as TotalPagos,
    IFNULL(SUM(p.Valor), 0) as MontoTotal,
    COUNT(DISTINCT f.ClienteId) as ClientesPagaron,
    COUNT(DISTINCT p.FacturaId) as FacturasPagadas
FROM Pago p
INNER JOIN Factura f ON f.Id = p.FacturaId;";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new PaymentResumenDto
            {
                TotalPagos = reader.GetInt32(0),
                MontoTotal = Convert.ToDecimal(reader.GetDouble(1)),
                ClientesPagaron = reader.GetInt32(2),
                FacturasPagadas = reader.GetInt32(3)
            };
        }

        return new PaymentResumenDto();
    }

    public List<PaymentGridRowDto> GetPaymentHistory(string? search = null)
    {
        var result = new List<PaymentGridRowDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var whereClause = "1=1";
        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause = "(f.Numero LIKE @search OR c.Nombre LIKE @search OR mp.Nombre LIKE @search)";
            command.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
        }

        command.CommandText = $@"
SELECT p.Id, f.Numero, c.Nombre, p.Fecha, p.Valor, mp.Nombre, p.Referencia, p.Notas
FROM Pago p
INNER JOIN Factura f ON f.Id = p.FacturaId
INNER JOIN Cliente c ON c.Id = f.ClienteId
LEFT JOIN MetodoPago mp ON mp.Id = p.MetodoPagoId
WHERE {whereClause}
ORDER BY p.Fecha DESC, p.Id DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new PaymentGridRowDto
            {
                Id = reader.GetInt64(0),
                NumeroFactura = reader.GetString(1),
                Cliente = reader.GetString(2),
                Fecha = reader.GetString(3),
                Valor = Convert.ToDecimal(reader.GetDouble(4)),
                MetodoPago = reader.IsDBNull(5) ? "N/A" : reader.GetString(5),
                Referencia = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Notas = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
            });
        }

        return result;
    }

    public void Add(Payment payment)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var fecha = payment.Fecha.ToString("yyyy-MM-dd");

        command.CommandText = @"
INSERT INTO Pago (FacturaId, Fecha, Valor, MetodoPagoId, Referencia, Notas)
VALUES (@facturaId, @fecha, @valor, @metodoPagoId, @referencia, @notas);";

        command.Parameters.AddWithValue("@facturaId", payment.FacturaId);
        command.Parameters.AddWithValue("@fecha", fecha);
        command.Parameters.AddWithValue("@valor", payment.Monto);
        command.Parameters.AddWithValue("@metodoPagoId", payment.MetodoPagoId);
        command.Parameters.AddWithValue("@referencia", payment.Referencia ?? string.Empty);
        command.Parameters.AddWithValue("@notas", payment.Notas ?? string.Empty);

        command.ExecuteNonQuery();
    }

    public void CreatePayment(PaymentCreateRequest request)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        using var invoiceCommand = connection.CreateCommand();
        invoiceCommand.Transaction = transaction;
        invoiceCommand.CommandText = @"
SELECT Total, Saldo, Estado
FROM Factura
WHERE Id = @facturaId;";
        invoiceCommand.Parameters.AddWithValue("@facturaId", request.FacturaId);

        using var invoiceReader = invoiceCommand.ExecuteReader();
        if (!invoiceReader.Read())
        {
            throw new InvalidOperationException("La factura no fue encontrada.");
        }

        var totalFactura = Convert.ToDecimal(invoiceReader.GetValue(0));
        var saldoActual = Convert.ToDecimal(invoiceReader.GetValue(1));
        var estadoActual = invoiceReader.GetString(2);
        invoiceReader.Close();

        if (estadoActual == "Anulada")
        {
            throw new InvalidOperationException("No se pueden registrar pagos sobre una factura anulada.");
        }

        // Insertar el pago
        using var paymentCommand = connection.CreateCommand();
        paymentCommand.Transaction = transaction;
        paymentCommand.CommandText = @"
INSERT INTO Pago(FacturaId, Fecha, Valor, MetodoPagoId, Referencia, Notas)
VALUES(@facturaId, @fecha, @valor, @metodoPagoId, @referencia, @notas);
SELECT last_insert_rowid();";

        paymentCommand.Parameters.AddWithValue("@facturaId", request.FacturaId);
        paymentCommand.Parameters.AddWithValue("@fecha", request.Fecha.ToString("yyyy-MM-dd"));
        paymentCommand.Parameters.AddWithValue("@valor", request.Valor);
        paymentCommand.Parameters.AddWithValue("@metodoPagoId", request.MetodoPagoId.HasValue ? request.MetodoPagoId.Value : DBNull.Value);
        paymentCommand.Parameters.AddWithValue("@referencia", request.Referencia.Trim());
        paymentCommand.Parameters.AddWithValue("@notas", request.Notas.Trim());

        var paymentId = Convert.ToInt64(paymentCommand.ExecuteScalar());

        var nuevoSaldo = saldoActual - request.Valor;

        // Actualizar el saldo y estado de la factura
        using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = @"
UPDATE Factura
SET Saldo = @saldo,
    Estado = @estado
WHERE Id = @facturaId;";
        updateCommand.Parameters.AddWithValue("@saldo", nuevoSaldo);
        updateCommand.Parameters.AddWithValue("@estado", ResolveInvoiceStatus(totalFactura, nuevoSaldo, estadoActual));
        updateCommand.Parameters.AddWithValue("@facturaId", request.FacturaId);
        updateCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    public void VoidPayment(long paymentId)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // Obtener datos del pago
        using var getCommand = connection.CreateCommand();
        getCommand.Transaction = transaction;
        getCommand.CommandText = @"
SELECT p.FacturaId, p.Valor, f.Total, f.Saldo, f.Estado
FROM Pago p
INNER JOIN Factura f ON f.Id = p.FacturaId
WHERE p.Id = @id;";
        getCommand.Parameters.AddWithValue("@id", paymentId);

        using var reader = getCommand.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException("El pago no fue encontrado.");
        }

        var facturaId = reader.GetInt64(0);
        var valor = Convert.ToDecimal(reader.GetValue(1));
        var totalFactura = Convert.ToDecimal(reader.GetValue(2));
        var saldoActual = Convert.ToDecimal(reader.GetValue(3));
        var estadoActual = reader.GetString(4);
        reader.Close();

        var nuevoSaldo = saldoActual + valor;

        // Eliminar el pago
        using var deleteCommand = connection.CreateCommand();
        deleteCommand.Transaction = transaction;
        deleteCommand.CommandText = "DELETE FROM Pago WHERE Id = @id;";
        deleteCommand.Parameters.AddWithValue("@id", paymentId);
        deleteCommand.ExecuteNonQuery();

        // Devolver el saldo y recalcular el estado de la factura
        using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = @"
UPDATE Factura
SET Saldo = @saldo,
    Estado = @estado
WHERE Id = @facturaId;";
        updateCommand.Parameters.AddWithValue("@saldo", nuevoSaldo);
        updateCommand.Parameters.AddWithValue("@estado", ResolveInvoiceStatus(totalFactura, nuevoSaldo, estadoActual));
        updateCommand.Parameters.AddWithValue("@facturaId", facturaId);
        updateCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    private static string ResolveInvoiceStatus(decimal totalFactura, decimal saldo, string? estadoActual)
    {
        if (string.Equals(estadoActual, "Anulada", StringComparison.OrdinalIgnoreCase))
        {
            return "Anulada";
        }

        if (saldo <= 0)
        {
            return "Pagada";
        }

        if (saldo < totalFactura)
        {
            return "Parcial";
        }

        if (string.Equals(estadoActual, "Vencida", StringComparison.OrdinalIgnoreCase))
        {
            return "Vencida";
        }

        return "Enviada";
    }
}
