using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class BalanceRepository
{
    public BalanceResumenDto GetResumen()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    IFNULL(SUM(f.Total), 0) as TotalFacturado,
    IFNULL(SUM(p.Valor), 0) as TotalRecaudado,
    IFNULL(SUM(f.Saldo), 0) as CuentasPorCobrar,
    COUNT(DISTINCT f.Id) as FacturasEmitidas
FROM Factura f
LEFT JOIN Pago p ON p.FacturaId = f.Id
WHERE date(f.Fecha) >= date('now', 'start of month');";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var totalFacturado = Convert.ToDecimal(reader.GetDouble(0));
            var totalRecaudado = Convert.ToDecimal(reader.GetDouble(1));
            var cuentasPorCobrar = Convert.ToDecimal(reader.GetDouble(2));
            var facturasEmitidas = reader.GetInt32(3);
            var balanceNeto = totalRecaudado - (totalFacturado - cuentasPorCobrar);

            return new BalanceResumenDto
            {
                TotalFacturado = totalFacturado,
                TotalRecaudado = totalRecaudado,
                CuentasPorCobrar = cuentasPorCobrar,
                BalanceNeto = balanceNeto,
                FacturasEmitidas = facturasEmitidas
            };
        }

        return new BalanceResumenDto();
    }

    public List<BalanceMensualDto> GetBalanceMensual(int meses = 6)
    {
        var result = new List<BalanceMensualDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
WITH RECURSIVE months(mes) AS (
    SELECT date('now', 'start of month', '-5 months')
    UNION ALL
    SELECT date(mes, '+1 month')
    FROM months
    WHERE mes < date('now', 'start of month')
)
SELECT 
    strftime('%Y-%m', m.mes) as Periodo,
    strftime('%m', m.mes) as MesNumero,
    CASE strftime('%m', m.mes)
        WHEN '01' THEN 'Enero'
        WHEN '02' THEN 'Febrero'
        WHEN '03' THEN 'Marzo'
        WHEN '04' THEN 'Abril'
        WHEN '05' THEN 'Mayo'
        WHEN '06' THEN 'Junio'
        WHEN '07' THEN 'Julio'
        WHEN '08' THEN 'Agosto'
        WHEN '09' THEN 'Septiembre'
        WHEN '10' THEN 'Octubre'
        WHEN '11' THEN 'Noviembre'
        WHEN '12' THEN 'Diciembre'
    END as MesNombre,
    IFNULL(SUM(f.Total), 0) as Facturado,
    IFNULL(SUM(p.Valor), 0) as Recaudado,
    COUNT(f.Id) as NumFacturas
FROM months m
LEFT JOIN Factura f ON strftime('%Y-%m', f.Fecha) = strftime('%Y-%m', m.mes)
LEFT JOIN Pago p ON p.FacturaId = f.Id AND strftime('%Y-%m', p.Fecha) = strftime('%Y-%m', m.mes)
GROUP BY strftime('%Y-%m', m.mes)
ORDER BY m.mes;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new BalanceMensualDto
            {
                Periodo = reader.GetString(0),
                MesNombre = reader.GetString(2),
                TotalFacturado = Convert.ToDecimal(reader.GetDouble(3)),
                TotalRecaudado = Convert.ToDecimal(reader.GetDouble(4)),
                NumeroFacturas = reader.GetInt32(5)
            });
        }

        return result;
    }

    public List<TopClienteDto> GetTopClientes(int limite = 10)
    {
        var result = new List<TopClienteDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    c.Id,
    c.Codigo,
    c.Nombre,
    COUNT(f.Id) as NumeroFacturas,
    IFNULL(SUM(f.Total), 0) as TotalFacturado,
    IFNULL(SUM(f.Saldo), 0) as SaldoPendiente,
    ROUND(CAST(IFNULL(SUM(f.Total), 0) AS REAL) / 
        (SELECT NULLIF(SUM(Total), 0) FROM Factura) * 100, 2) as PorcentajeTotal
FROM Cliente c
INNER JOIN Factura f ON f.ClienteId = c.Id
WHERE date(f.Fecha) >= date('now', 'start of year')
GROUP BY c.Id, c.Codigo, c.Nombre
ORDER BY TotalFacturado DESC
LIMIT @limite;";
        command.Parameters.AddWithValue("@limite", limite);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new TopClienteDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                NumeroFacturas = reader.GetInt32(3),
                TotalFacturado = Convert.ToDecimal(reader.GetDouble(4)),
                SaldoPendiente = Convert.ToDecimal(reader.GetDouble(5)),
                PorcentajeTotal = Convert.ToDecimal(reader.IsDBNull(6) ? 0 : reader.GetDouble(6))
            });
        }

        return result;
    }

    public List<EstadisticaMensualDto> GetEstadisticasMensuales()
    {
        var result = new List<EstadisticaMensualDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    strftime('%Y-%m', Fecha) as Periodo,
    COUNT(*) as TotalFacturas,
    COUNT(DISTINCT ClienteId) as ClientesUnicos,
    IFNULL(AVG(Total), 0) as PromedioFactura,
    IFNULL(MAX(Total), 0) as FacturaMayor,
    IFNULL(MIN(Total), 0) as FacturaMenor
FROM Factura
WHERE date(Fecha) >= date('now', 'start of month', '-5 months')
GROUP BY strftime('%Y-%m', Fecha)
ORDER BY Periodo;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new EstadisticaMensualDto
            {
                Periodo = reader.GetString(0),
                TotalFacturas = reader.GetInt32(1),
                ClientesUnicos = reader.GetInt32(2),
                PromedioFactura = Convert.ToDecimal(reader.GetDouble(3)),
                FacturaMayor = Convert.ToDecimal(reader.GetDouble(4)),
                FacturaMenor = Convert.ToDecimal(reader.GetDouble(5))
            });
        }

        return result;
    }
}
