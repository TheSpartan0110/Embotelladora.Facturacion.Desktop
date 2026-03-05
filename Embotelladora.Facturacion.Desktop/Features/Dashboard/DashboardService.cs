using Embotelladora.Facturacion.Desktop.Data;
using Microsoft.Data.Sqlite;

namespace Embotelladora.Facturacion.Desktop.Features.Dashboard;

internal sealed class DashboardService
{
    public DashboardSnapshot GetSnapshot()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        return new DashboardSnapshot
        {
            TotalFacturado = ExecuteScalarDecimal(connection, "SELECT IFNULL(SUM(Total), 0) FROM Factura WHERE Estado <> 'Anulada';"),
            SaldoPendiente = ExecuteScalarDecimal(connection, "SELECT IFNULL(SUM(Saldo), 0) FROM Factura WHERE Estado IN ('Enviada', 'Parcial');"),
            TotalPagos = ExecuteScalarDecimal(connection, "SELECT IFNULL(SUM(Valor), 0) FROM Pago;"),
            ClientesActivos = ExecuteScalarInt(connection, "SELECT COUNT(1) FROM Cliente WHERE Activo = 1;"),
            FacturasPendientes = ExecuteScalarInt(connection, "SELECT COUNT(1) FROM Factura WHERE Estado IN ('Enviada', 'Parcial');"),
            PagosRegistrados = ExecuteScalarInt(connection, "SELECT COUNT(1) FROM Pago;")
        };
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
