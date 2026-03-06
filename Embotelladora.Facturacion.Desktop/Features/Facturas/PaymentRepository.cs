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
WHERE f.Saldo > 0 AND f.Estado = 'Enviada'
ORDER BY f.Fecha DESC;";

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

    public List<PaymentGridRowDto> GetPaymentHistory(int limit = 30)
    {
        var result = new List<PaymentGridRowDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT p.Id, f.Numero, c.Nombre, p.Fecha, p.Valor, mp.Nombre, p.Referencia, p.Notas
FROM Pago p
INNER JOIN Factura f ON f.Id = p.FacturaId
INNER JOIN Cliente c ON c.Id = f.ClienteId
LEFT JOIN MetodoPago mp ON mp.Id = p.MetodoPagoId
ORDER BY p.Fecha DESC
LIMIT @limit;";
        command.Parameters.AddWithValue("@limit", limit);

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

        // Actualizar el saldo de la factura
        using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = @"
UPDATE Factura
SET Saldo = Saldo - @valor
WHERE Id = @facturaId;";
        updateCommand.Parameters.AddWithValue("@valor", request.Valor);
        updateCommand.Parameters.AddWithValue("@facturaId", request.FacturaId);
        updateCommand.ExecuteNonQuery();

        // Verificar si la factura está pagada completamente
        using var checkCommand = connection.CreateCommand();
        checkCommand.Transaction = transaction;
        checkCommand.CommandText = "SELECT Saldo FROM Factura WHERE Id = @id;";
        checkCommand.Parameters.AddWithValue("@id", request.FacturaId);
        var newSaldo = Convert.ToDecimal(checkCommand.ExecuteScalar());

        if (newSaldo <= 0)
        {
            using var statusCommand = connection.CreateCommand();
            statusCommand.Transaction = transaction;
            statusCommand.CommandText = "UPDATE Factura SET Estado = 'Pagada' WHERE Id = @id;";
            statusCommand.Parameters.AddWithValue("@id", request.FacturaId);
            statusCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
