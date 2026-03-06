using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class BalanceRepository
{
    public BalanceResumenDto GetResumen(BalancePeriodo periodo = BalancePeriodo.Mensual)
        => GetResumenForRange(GetDateRange(periodo));

    public BalanceResumenDto GetResumen(DateTime fecha)
        => GetResumenForRange((fecha, fecha));

    private BalanceResumenDto GetResumenForRange((DateTime? Inicio, DateTime? Fin) rango)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        var whereFacturas = BuildDateWhereClause("f.Fecha", rango);
        var wherePagos = BuildDateWhereClause("p.Fecha", rango);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT
    (SELECT IFNULL(SUM(f.Total), 0) FROM Factura f WHERE {whereFacturas}) as TotalFacturado,
    (SELECT IFNULL(SUM(p.Valor), 0) FROM Pago p WHERE {wherePagos}) as TotalRecaudado,
    (SELECT IFNULL(SUM(f.Saldo), 0) FROM Factura f WHERE {whereFacturas}) as CuentasPorCobrar,
    (SELECT COUNT(*) FROM Factura f WHERE {whereFacturas}) as FacturasEmitidas;";

        AddDateParameters(command, rango);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var totalFacturado = ToDecimal(reader, 0);
            var totalRecaudado = ToDecimal(reader, 1);
            var cuentasPorCobrar = ToDecimal(reader, 2);
            var facturasEmitidas = Convert.ToInt32(reader.GetValue(3));
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

    public List<BalanceMensualDto> GetBalanceDetalle(BalancePeriodo periodo)
    {
        return periodo switch
        {
            BalancePeriodo.Diario => GetBalanceDiario(),
            BalancePeriodo.Quincenal => GetBalanceQuincenal(),
            BalancePeriodo.Mensual => GetBalanceMensual(),
            BalancePeriodo.Anual => GetBalanceAnual(),
            BalancePeriodo.Total => GetBalanceTotal(),
            _ => GetBalanceMensual()
        };
    }

    public List<BalanceMensualDto> GetBalanceMensual(int meses = 6)
    {
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
),
facturas AS (
    SELECT strftime('%Y-%m', Fecha) as Periodo,
           IFNULL(SUM(Total), 0) as TotalFacturado,
           COUNT(*) as NumeroFacturas
    FROM Factura
    GROUP BY strftime('%Y-%m', Fecha)
),
pagos AS (
    SELECT strftime('%Y-%m', Fecha) as Periodo,
           IFNULL(SUM(Valor), 0) as TotalRecaudado
    FROM Pago
    GROUP BY strftime('%Y-%m', Fecha)
)
SELECT
    strftime('%Y-%m', m.mes) as Periodo,
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
    END || ' ' || strftime('%Y', m.mes) as MesNombre,
    IFNULL(f.TotalFacturado, 0) as TotalFacturado,
    IFNULL(p.TotalRecaudado, 0) as TotalRecaudado,
    IFNULL(f.NumeroFacturas, 0) as NumeroFacturas
FROM months m
LEFT JOIN facturas f ON f.Periodo = strftime('%Y-%m', m.mes)
LEFT JOIN pagos p ON p.Periodo = strftime('%Y-%m', m.mes)
ORDER BY m.mes;";

        return ExecuteBalanceDetalle(command);
    }

    public List<TopClienteDto> GetTopClientes(int limite = 10, BalancePeriodo periodo = BalancePeriodo.Mensual)
    {
        var result = new List<TopClienteDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        var rango = GetDateRange(periodo);
        var whereFacturas = BuildDateWhereClause("f.Fecha", rango);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
WITH FacturasFiltradas AS (
    SELECT *
    FROM Factura f
    WHERE {whereFacturas}
),
TotalGeneral AS (
    SELECT NULLIF(SUM(Total), 0) as TotalFacturado
    FROM FacturasFiltradas
)
SELECT
    c.Id,
    c.Codigo,
    c.Nombre,
    COUNT(f.Id) as NumeroFacturas,
    IFNULL(SUM(f.Total), 0) as TotalFacturado,
    IFNULL(SUM(f.Saldo), 0) as SaldoPendiente,
    ROUND(CAST(IFNULL(SUM(f.Total), 0) AS REAL) / IFNULL(tg.TotalFacturado, 1) * 100, 2) as PorcentajeTotal
FROM Cliente c
INNER JOIN FacturasFiltradas f ON f.ClienteId = c.Id
CROSS JOIN TotalGeneral tg
GROUP BY c.Id, c.Codigo, c.Nombre, tg.TotalFacturado
ORDER BY TotalFacturado DESC
LIMIT @limite;";
        command.Parameters.AddWithValue("@limite", limite);
        AddDateParameters(command, rango);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new TopClienteDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                NumeroFacturas = reader.GetInt32(3),
                TotalFacturado = ToDecimal(reader, 4),
                SaldoPendiente = ToDecimal(reader, 5),
                PorcentajeTotal = ToDecimal(reader, 6)
            });
        }

        return result;
    }

    public List<BalanceFacturaDto> GetFacturas(BalancePeriodo periodo)
        => GetFacturasForRange(GetDateRange(periodo));

    public List<BalanceFacturaDto> GetFacturas(DateTime fecha)
        => GetFacturasForRange((fecha, fecha));

    public List<BalanceFacturaDto> GetFacturas(DateTime inicio, DateTime fin)
        => GetFacturasForRange((inicio, fin));

    private List<BalanceFacturaDto> GetFacturasForRange((DateTime? Inicio, DateTime? Fin) rango)
    {
        var result = new List<BalanceFacturaDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        var where = BuildDateWhereClause("f.Fecha", rango);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT
    f.Id,
    f.Numero,
    f.Fecha,
    c.Nombre as Cliente,
    f.Total,
    f.Saldo,
    f.Estado
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
WHERE {where}
ORDER BY f.Numero;";

        AddDateParameters(command, rango);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new BalanceFacturaDto
            {
                Id = reader.GetInt64(0),
                Numero = reader.GetString(1),
                Fecha = reader.GetString(2),
                Cliente = reader.GetString(3),
                Total = ToDecimal(reader, 4),
                Saldo = ToDecimal(reader, 5),
                Estado = reader.GetString(6)
            });
        }

        return result;
    }

    public List<BalancePagoDto> GetPagosFactura(long facturaId)
    {
        var result = new List<BalancePagoDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    p.Id,
    p.Fecha,
    p.Valor,
    IFNULL(mp.Nombre, '-') as MetodoPago,
    IFNULL(p.Referencia, '') as Referencia,
    IFNULL(p.Notas, '') as Notas
FROM Pago p
LEFT JOIN MetodoPago mp ON mp.Id = p.MetodoPagoId
WHERE p.FacturaId = @facturaId
ORDER BY p.Id;";

        var param = command.CreateParameter();
        param.ParameterName = "@facturaId";
        param.Value = facturaId;
        command.Parameters.Add(param);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new BalancePagoDto
            {
                Id = reader.GetInt64(0),
                Fecha = reader.GetString(1),
                Valor = ToDecimal(reader, 2),
                MetodoPago = reader.GetString(3),
                Referencia = reader.GetString(4),
                Notas = reader.GetString(5)
            });
        }

        return result;
    }

    public List<BalanceMensualDto> GetBalanceDetalleFecha(DateTime fecha)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
WITH RECURSIVE dias(dia) AS (
    SELECT date(@fecha, '-6 days')
    UNION ALL
    SELECT date(dia, '+1 day')
    FROM dias
    WHERE dia < date(@fecha)
)
SELECT
    dia as Periodo,
    CASE WHEN dia = date(@fecha)
        THEN '▸ ' || strftime('%d/%m/%Y', dia)
        ELSE strftime('%d/%m/%Y', dia)
    END as MesNombre,
    (SELECT IFNULL(SUM(f.Total), 0) FROM Factura f WHERE date(f.Fecha) = dia) as TotalFacturado,
    (SELECT IFNULL(SUM(p.Valor), 0) FROM Pago p WHERE date(p.Fecha) = dia) as TotalRecaudado,
    (SELECT COUNT(*) FROM Factura f WHERE date(f.Fecha) = dia) as NumeroFacturas
FROM dias
ORDER BY dia;";

        var param = command.CreateParameter();
        param.ParameterName = "@fecha";
        param.Value = fecha.ToString("yyyy-MM-dd");
        command.Parameters.Add(param);

        return ExecuteBalanceDetalle(command);
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
                PromedioFactura = ToDecimal(reader, 3),
                FacturaMayor = ToDecimal(reader, 4),
                FacturaMenor = ToDecimal(reader, 5)
            });
        }

        return result;
    }

    private static List<BalanceMensualDto> GetBalanceDiario()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
WITH RECURSIVE dias(dia) AS (
    SELECT date('now', '-6 days')
    UNION ALL
    SELECT date(dia, '+1 day')
    FROM dias
    WHERE dia < date('now')
)
SELECT
    dia as Periodo,
    strftime('%d/%m', dia) as MesNombre,
    (SELECT IFNULL(SUM(f.Total), 0) FROM Factura f WHERE date(f.Fecha) = dia) as TotalFacturado,
    (SELECT IFNULL(SUM(p.Valor), 0) FROM Pago p WHERE date(p.Fecha) = dia) as TotalRecaudado,
    (SELECT COUNT(*) FROM Factura f WHERE date(f.Fecha) = dia) as NumeroFacturas
FROM dias
ORDER BY dia;";

        return ExecuteBalanceDetalle(command);
    }

    private static List<BalanceMensualDto> GetBalanceQuincenal()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
WITH quincenas AS (
    SELECT
        date('now', 'start of month') as Inicio,
        date('now', 'start of month', '+14 days') as Fin,
        '1ra Quincena' as Nombre,
        1 as Orden
    UNION ALL
    SELECT
        date('now', 'start of month', '+15 days') as Inicio,
        date('now', 'start of month', '+1 month', '-1 day') as Fin,
        '2da Quincena' as Nombre,
        2 as Orden
)
SELECT
    strftime('%Y-%m', Inicio) || '-Q' || Orden as Periodo,
    Nombre || ' ' || CASE strftime('%m', Inicio)
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
    END || ' ' || strftime('%Y', Inicio) as MesNombre,
    (SELECT IFNULL(SUM(f.Total), 0) FROM Factura f WHERE date(f.Fecha) BETWEEN Inicio AND Fin) as TotalFacturado,
    (SELECT IFNULL(SUM(p.Valor), 0) FROM Pago p WHERE date(p.Fecha) BETWEEN Inicio AND Fin) as TotalRecaudado,
    (SELECT COUNT(*) FROM Factura f WHERE date(f.Fecha) BETWEEN Inicio AND Fin) as NumeroFacturas
FROM quincenas
ORDER BY Orden;";

        return ExecuteBalanceDetalle(command);
    }

    private static List<BalanceMensualDto> GetBalanceAnual()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
WITH RECURSIVE anios(anio) AS (
    SELECT CAST(strftime('%Y', 'now') AS INTEGER) - 4
    UNION ALL
    SELECT anio + 1
    FROM anios
    WHERE anio < CAST(strftime('%Y', 'now') AS INTEGER)
)
SELECT
    CAST(anio AS TEXT) as Periodo,
    CAST(anio AS TEXT) as MesNombre,
    (SELECT IFNULL(SUM(f.Total), 0) FROM Factura f WHERE CAST(strftime('%Y', f.Fecha) AS INTEGER) = anio) as TotalFacturado,
    (SELECT IFNULL(SUM(p.Valor), 0) FROM Pago p WHERE CAST(strftime('%Y', p.Fecha) AS INTEGER) = anio) as TotalRecaudado,
    (SELECT COUNT(*) FROM Factura f WHERE CAST(strftime('%Y', f.Fecha) AS INTEGER) = anio) as NumeroFacturas
FROM anios
ORDER BY anio;";

        return ExecuteBalanceDetalle(command);
    }

    private static List<BalanceMensualDto> GetBalanceTotal()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
WITH anios AS (
    SELECT DISTINCT CAST(strftime('%Y', Fecha) AS INTEGER) as Anio
    FROM Factura
    UNION
    SELECT DISTINCT CAST(strftime('%Y', Fecha) AS INTEGER) as Anio
    FROM Pago
)
SELECT
    CAST(Anio AS TEXT) as Periodo,
    CAST(Anio AS TEXT) as MesNombre,
    (SELECT IFNULL(SUM(f.Total), 0) FROM Factura f WHERE CAST(strftime('%Y', f.Fecha) AS INTEGER) = Anio) as TotalFacturado,
    (SELECT IFNULL(SUM(p.Valor), 0) FROM Pago p WHERE CAST(strftime('%Y', p.Fecha) AS INTEGER) = Anio) as TotalRecaudado,
    (SELECT COUNT(*) FROM Factura f WHERE CAST(strftime('%Y', f.Fecha) AS INTEGER) = Anio) as NumeroFacturas
FROM anios
ORDER BY Anio;";

        return ExecuteBalanceDetalle(command);
    }

    private static List<BalanceMensualDto> ExecuteBalanceDetalle(System.Data.Common.DbCommand command)
    {
        var result = new List<BalanceMensualDto>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new BalanceMensualDto
            {
                Periodo = reader.GetString(0),
                MesNombre = reader.GetString(1),
                TotalFacturado = ToDecimal(reader, 2),
                TotalRecaudado = ToDecimal(reader, 3),
                NumeroFacturas = Convert.ToInt32(reader.GetValue(4))
            });
        }

        return result;
    }

    private static decimal ToDecimal(System.Data.Common.DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return 0m;
        }

        return Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static string BuildDateWhereClause(string fieldName, (DateTime? Inicio, DateTime? Fin) rango)
    {
        if (rango.Inicio is null || rango.Fin is null)
        {
            return "1=1";
        }

        return $"date({fieldName}) BETWEEN date(@fechaInicio) AND date(@fechaFin)";
    }

    private static void AddDateParameters(System.Data.Common.DbCommand command, (DateTime? Inicio, DateTime? Fin) rango)
    {
        if (rango.Inicio is null || rango.Fin is null)
        {
            return;
        }

        var inicio = command.CreateParameter();
        inicio.ParameterName = "@fechaInicio";
        inicio.Value = rango.Inicio.Value.ToString("yyyy-MM-dd");
        command.Parameters.Add(inicio);

        var fin = command.CreateParameter();
        fin.ParameterName = "@fechaFin";
        fin.Value = rango.Fin.Value.ToString("yyyy-MM-dd");
        command.Parameters.Add(fin);
    }

    private static (DateTime? Inicio, DateTime? Fin) GetDateRange(BalancePeriodo periodo)
    {
        var hoy = DateTime.Today;

        return periodo switch
        {
            BalancePeriodo.Diario => (hoy, hoy),
            BalancePeriodo.Quincenal => GetCurrentFortnight(hoy),
            BalancePeriodo.Mensual => (new DateTime(hoy.Year, hoy.Month, 1), new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month))),
            BalancePeriodo.Anual => (new DateTime(hoy.Year, 1, 1), new DateTime(hoy.Year, 12, 31)),
            BalancePeriodo.Total => (null, null),
            _ => (new DateTime(hoy.Year, hoy.Month, 1), new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month)))
        };
    }

    private static (DateTime? Inicio, DateTime? Fin) GetCurrentFortnight(DateTime hoy)
    {
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        if (hoy.Day <= 15)
        {
            return (inicioMes, new DateTime(hoy.Year, hoy.Month, 15));
        }

        var finMes = new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month));
        return (new DateTime(hoy.Year, hoy.Month, 16), finMes);
    }
}
