using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class InventarioRepository
{
    public InventarioResumenDto GetResumen()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    COUNT(*) as TotalProductos,
    IFNULL(SUM(StockActual * PrecioBase), 0) as ValorInventario,
    COUNT(CASE WHEN StockActual <= StockMinimo THEN 1 END) as ProductosStockBajo,
    COUNT(CASE WHEN StockActual = 0 THEN 1 END) as ProductosAgotados
FROM ProductoExt;";

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new InventarioResumenDto
            {
                TotalProductos = reader.GetInt32(0),
                ValorInventario = Convert.ToDecimal(reader.GetDouble(1)),
                ProductosStockBajo = reader.GetInt32(2),
                ProductosAgotados = reader.GetInt32(3)
            };
        }

        return new InventarioResumenDto();
    }

    public List<ProductoInventarioDto> GetProductos(string? search = null)
    {
        var result = new List<ProductoInventarioDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        
        var whereClause = string.IsNullOrWhiteSpace(search)
            ? ""
            : "WHERE Codigo LIKE @search OR Nombre LIKE @search";

        command.CommandText = $@"
SELECT 
    Id,
    Codigo,
    Nombre,
    Unidad,
    StockActual,
    StockMinimo,
    PrecioBase,
    (StockActual * PrecioBase) as ValorStock,
    CASE 
        WHEN StockActual = 0 THEN 'Agotado'
        WHEN StockActual <= StockMinimo THEN 'Bajo'
        WHEN StockActual <= StockMinimo * 2 THEN 'Medio'
        ELSE 'Óptimo'
    END as EstadoStock
FROM ProductoExt
{whereClause}
ORDER BY 
    CASE 
        WHEN StockActual = 0 THEN 1
        WHEN StockActual <= StockMinimo THEN 2
        ELSE 3
    END,
    Nombre;";

        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ProductoInventarioDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Unidad = reader.GetString(3),
                StockActual = Convert.ToDecimal(reader.GetDouble(4)),
                StockMinimo = Convert.ToDecimal(reader.GetDouble(5)),
                PrecioBase = Convert.ToDecimal(reader.GetDouble(6)),
                ValorStock = Convert.ToDecimal(reader.GetDouble(7)),
                EstadoStock = reader.GetString(8)
            });
        }

        return result;
    }

    public List<MovimientoInventarioDto> GetMovimientos(int limite = 50)
    {
        var result = new List<MovimientoInventarioDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    m.Id,
    m.Fecha,
    p.Codigo,
    p.Nombre as Producto,
    m.Tipo,
    m.Cantidad,
    CASE 
        WHEN m.ReferenciaFacturaId IS NOT NULL THEN 'Factura #' || (SELECT Numero FROM Factura WHERE Id = m.ReferenciaFacturaId)
        ELSE m.Nota
    END as Referencia
FROM MovimientoInventarioExt m
INNER JOIN ProductoExt p ON p.Id = m.ProductoId
ORDER BY date(m.Fecha) DESC, m.Id DESC
LIMIT @limite;";
        command.Parameters.AddWithValue("@limite", limite);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MovimientoInventarioDto
            {
                Id = reader.GetInt64(0),
                Fecha = reader.GetString(1),
                Codigo = reader.GetString(2),
                Producto = reader.GetString(3),
                Tipo = reader.GetString(4),
                Cantidad = Convert.ToDecimal(reader.GetDouble(5)),
                Referencia = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            });
        }

        return result;
    }

    public List<ProductoStockBajoDto> GetProductosStockBajo()
    {
        var result = new List<ProductoStockBajoDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT 
    Id,
    Codigo,
    Nombre,
    Unidad,
    StockActual,
    StockMinimo,
    (StockMinimo - StockActual) as Faltante,
    PrecioBase,
    ((StockMinimo - StockActual) * PrecioBase) as ValorFaltante
FROM ProductoExt
WHERE StockActual <= StockMinimo
ORDER BY Faltante DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ProductoStockBajoDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Unidad = reader.GetString(3),
                StockActual = Convert.ToDecimal(reader.GetDouble(4)),
                StockMinimo = Convert.ToDecimal(reader.GetDouble(5)),
                Faltante = Convert.ToDecimal(reader.GetDouble(6)),
                PrecioBase = Convert.ToDecimal(reader.GetDouble(7)),
                ValorFaltante = Convert.ToDecimal(reader.GetDouble(8))
            });
        }

        return result;
    }

    public void AjustarInventario(long productoId, decimal cantidad, string tipo, string nota)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            // Insertar movimiento
            using var movCommand = connection.CreateCommand();
            movCommand.Transaction = transaction;
            movCommand.CommandText = @"
INSERT INTO MovimientoInventarioExt (ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES (@productoId, @fecha, @tipo, @cantidad, NULL, @nota);";

            movCommand.Parameters.AddWithValue("@productoId", productoId);
            movCommand.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd"));
            movCommand.Parameters.AddWithValue("@tipo", tipo);
            movCommand.Parameters.AddWithValue("@cantidad", cantidad);
            movCommand.Parameters.AddWithValue("@nota", nota);
            movCommand.ExecuteNonQuery();

            // Actualizar stock
            using var updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;

            if (tipo == "ENTRADA")
            {
                updateCommand.CommandText = @"
UPDATE ProductoExt 
SET StockActual = StockActual + @cantidad 
WHERE Id = @productoId;";
            }
            else
            {
                updateCommand.CommandText = @"
UPDATE ProductoExt 
SET StockActual = StockActual - @cantidad 
WHERE Id = @productoId;";
            }

            updateCommand.Parameters.AddWithValue("@cantidad", cantidad);
            updateCommand.Parameters.AddWithValue("@productoId", productoId);
            updateCommand.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
