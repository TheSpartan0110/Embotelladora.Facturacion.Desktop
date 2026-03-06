using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class CarteraRepository
{
    public CarteraResumenDto GetResumen()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    COUNT(DISTINCT CASE WHEN Saldo > 0 THEN ClienteId END) as ClientesConSaldo,
    COUNT(CASE WHEN Saldo > 0 THEN 1 END) as FacturasPendientes,
    IFNULL(SUM(CASE WHEN Saldo > 0 THEN Saldo END), 0) as TotalPorCobrar,
    IFNULL(SUM(CASE 
        WHEN Saldo > 0 AND julianday('now') - julianday(Fecha) > 30 
        THEN Saldo 
    END), 0) as SaldoVencido,
    COUNT(DISTINCT CASE WHEN Saldo < 0 THEN ClienteId END) as ClientesConSaldoAFavor,
    IFNULL(SUM(CASE WHEN Saldo < 0 THEN ABS(Saldo) END), 0) as TotalSaldoAFavor
FROM Factura;";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new CarteraResumenDto
            {
                ClientesConSaldo = reader.GetInt32(0),
                FacturasPendientes = reader.GetInt32(1),
                TotalPorCobrar = Convert.ToDecimal(reader.GetDouble(2)),
                SaldoVencido = Convert.ToDecimal(reader.GetDouble(3)),
                ClientesConSaldoAFavor = reader.GetInt32(4),
                TotalSaldoAFavor = Convert.ToDecimal(reader.GetDouble(5))
            };
        }

        return new CarteraResumenDto();
    }

    public List<FacturaPendienteDto> GetFacturasPendientes(string? search = null)
    {
        var result = new List<FacturaPendienteDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var whereClause = "f.Saldo > 0";
        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause += " AND (f.Numero LIKE @search OR c.Nombre LIKE @search)";
            command.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
        }

        command.CommandText = $@"
SELECT 
    f.Id,
    f.Numero,
    f.Fecha,
    c.Nombre as Cliente,
    f.Total,
    f.Saldo,
    CAST(julianday('now') - julianday(f.Fecha) AS INTEGER) as DiasTranscurridos,
    CASE 
        WHEN julianday('now') - julianday(f.Fecha) <= 30 THEN 'Al día'
        WHEN julianday('now') - julianday(f.Fecha) <= 60 THEN 'Vencida 30 días'
        WHEN julianday('now') - julianday(f.Fecha) <= 90 THEN 'Vencida 60 días'
        ELSE 'Vencida +90 días'
    END as EstadoVencimiento
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
WHERE {whereClause}
ORDER BY f.Numero;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new FacturaPendienteDto
            {
                Id = reader.GetInt64(0),
                Numero = reader.GetString(1),
                Fecha = reader.GetString(2),
                Cliente = reader.GetString(3),
                Total = Convert.ToDecimal(reader.GetDouble(4)),
                Saldo = Convert.ToDecimal(reader.GetDouble(5)),
                DiasTranscurridos = reader.GetInt32(6),
                EstadoVencimiento = reader.GetString(7)
            });
        }

        return result;
    }

    public List<ClienteSaldoDto> GetClientesConSaldo()
    {
        var result = new List<ClienteSaldoDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    c.Id,
    c.Codigo,
    c.Nombre,
    c.Telefono,
    COUNT(f.Id) as FacturasPendientes,
    IFNULL(SUM(f.Saldo), 0) as SaldoTotal,
    IFNULL(SUM(CASE WHEN julianday('now') - julianday(f.Fecha) > 30 THEN f.Saldo END), 0) as SaldoVencido
FROM Cliente c
INNER JOIN Factura f ON f.ClienteId = c.Id
WHERE f.Saldo > 0
GROUP BY c.Id, c.Codigo, c.Nombre, c.Telefono
ORDER BY c.Codigo;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ClienteSaldoDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Telefono = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                FacturasPendientes = reader.GetInt32(4),
                SaldoTotal = Convert.ToDecimal(reader.GetDouble(5)),
                SaldoVencido = Convert.ToDecimal(reader.GetDouble(6))
            });
        }

        return result;
    }

    public List<EdadSaldoDto> GetEdadSaldos()
    {
        var result = new List<EdadSaldoDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    CASE 
        WHEN julianday('now') - julianday(f.Fecha) <= 30 THEN '0-30 días'
        WHEN julianday('now') - julianday(f.Fecha) <= 60 THEN '31-60 días'
        WHEN julianday('now') - julianday(f.Fecha) <= 90 THEN '61-90 días'
        ELSE 'Más de 90 días'
    END as RangoEdad,
    COUNT(*) as CantidadFacturas,
    IFNULL(SUM(f.Saldo), 0) as TotalSaldo
FROM Factura f
WHERE f.Saldo > 0
GROUP BY RangoEdad
ORDER BY 
    CASE 
        WHEN RangoEdad = '0-30 días' THEN 1
        WHEN RangoEdad = '31-60 días' THEN 2
        WHEN RangoEdad = '61-90 días' THEN 3
        ELSE 4
    END;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new EdadSaldoDto
            {
                RangoEdad = reader.GetString(0),
                CantidadFacturas = reader.GetInt32(1),
                TotalSaldo = Convert.ToDecimal(reader.GetDouble(2))
            });
        }

        var totalGeneral = result.Sum(x => x.TotalSaldo);
        foreach (var item in result)
        {
            item.PorcentajeSaldo = totalGeneral > 0
                ? Math.Round(item.TotalSaldo / totalGeneral * 100, 1)
                : 0;
        }

        return result;
    }

    public List<ClienteSaldoFavorDto> GetClientesConSaldoAFavor()
    {
        var result = new List<ClienteSaldoFavorDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    c.Id,
    c.Codigo,
    c.Nombre,
    c.Telefono,
    COUNT(f.Id) as FacturasConCredito,
    IFNULL(SUM(ABS(f.Saldo)), 0) as SaldoAFavor
FROM Cliente c
INNER JOIN Factura f ON f.ClienteId = c.Id
WHERE f.Saldo < 0
GROUP BY c.Id, c.Codigo, c.Nombre, c.Telefono
ORDER BY SaldoAFavor DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ClienteSaldoFavorDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Telefono = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                FacturasConCredito = reader.GetInt32(4),
                SaldoAFavor = Convert.ToDecimal(reader.GetDouble(5))
            });
        }

        return result;
    }
}
