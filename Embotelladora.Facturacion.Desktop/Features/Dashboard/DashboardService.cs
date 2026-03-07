using Embotelladora.Facturacion.Desktop.Data;
using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Features.Dashboard;

internal sealed class DashboardService
{
    private const string EstadoActualSql = @"CASE
    WHEN Estado = 'Anulada' THEN 'Anulada'
    WHEN Saldo <= 0 THEN 'Pagada'
    WHEN Saldo < Total THEN 'Parcial'
    WHEN Estado = 'Vencida' THEN 'Vencida'
    ELSE 'Enviada'
END";

    public DashboardSnapshot GetSnapshot()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        return new DashboardSnapshot
        {
            TotalFacturado = ExecuteScalarDecimal(connection, "SELECT IFNULL(SUM(Total), 0) FROM Factura WHERE Estado <> 'Anulada';"),
            SaldoPendiente = ExecuteScalarDecimal(connection, $"SELECT IFNULL(SUM(Saldo), 0) FROM Factura WHERE Saldo > 0 AND ({EstadoActualSql}) IN ('Enviada', 'Parcial', 'Vencida');"),
            TotalPagos = ExecuteScalarDecimal(connection, "SELECT IFNULL(SUM(Valor), 0) FROM Pago;"),
            ClientesActivos = ExecuteScalarInt(connection, "SELECT COUNT(1) FROM Cliente WHERE Activo = 1;"),
            FacturasPendientes = ExecuteScalarInt(connection, $"SELECT COUNT(1) FROM Factura WHERE Saldo > 0 AND ({EstadoActualSql}) IN ('Enviada', 'Parcial', 'Vencida');"),
            PagosRegistrados = ExecuteScalarInt(connection, "SELECT COUNT(1) FROM Pago;")
        };
    }

    public List<DashboardStatusDto> GetInvoiceStatusDistribution()
    {
        var result = new List<DashboardStatusDto>();
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT {EstadoActualSql} as Estado, COUNT(*) as Cantidad, IFNULL(SUM(Total), 0) as Total
FROM Factura
GROUP BY {EstadoActualSql}
ORDER BY Total DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new DashboardStatusDto
            {
                Estado = reader.GetString(0),
                Cantidad = reader.GetInt32(1),
                Total = Convert.ToDecimal(reader.GetValue(2))
            });
        }

        return result;
    }

    public List<DashboardPaymentMethodDto> GetPaymentMethodDistribution()
    {
        var result = new List<DashboardPaymentMethodDto>();
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT IFNULL(mp.Nombre, 'Sin método') as MetodoPago,
       COUNT(*) as Cantidad,
       IFNULL(SUM(p.Valor), 0) as Total
FROM Pago p
LEFT JOIN MetodoPago mp ON mp.Id = p.MetodoPagoId
GROUP BY mp.Nombre
ORDER BY Total DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new DashboardPaymentMethodDto
            {
                MetodoPago = reader.GetString(0),
                Cantidad = reader.GetInt32(1),
                Total = Convert.ToDecimal(reader.GetValue(2))
            });
        }

        return result;
    }

    private static decimal ExecuteScalarDecimal(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = command.ExecuteScalar();
        return Convert.ToDecimal(result);
    }

    private static int ExecuteScalarInt(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }
}
