using Embotelladora.Facturacion.Desktop.Data;
using Microsoft.Data.Sqlite;

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
ORDER BY Codigo;";

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

    public List<MovimientoInventarioDto> GetMovimientos(int limite = 50, long? productoId = null)
    {
        var result = new List<MovimientoInventarioDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        var whereClause = productoId.HasValue ? "WHERE m.ProductoId = @productoId" : "";

        command.CommandText = $@"
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
{whereClause}
ORDER BY date(m.Fecha) DESC, m.Id DESC
LIMIT @limite;";
        command.Parameters.AddWithValue("@limite", limite);

        if (productoId.HasValue)
        {
            command.Parameters.AddWithValue("@productoId", productoId.Value);
        }

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
ORDER BY Codigo;";

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
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
        }

        var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();
        if (tipoNormalizado is not ("ENTRADA" or "SALIDA"))
        {
            throw new InvalidOperationException("Tipo de movimiento inválido.");
        }

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            decimal stockActual;
            using (var stockCommand = connection.CreateCommand())
            {
                stockCommand.Transaction = transaction;
                stockCommand.CommandText = "SELECT StockActual FROM ProductoExt WHERE Id = @productoId;";
                stockCommand.Parameters.AddWithValue("@productoId", productoId);

                var scalar = stockCommand.ExecuteScalar();
                if (scalar is null || scalar == DBNull.Value)
                {
                    throw new InvalidOperationException("El producto seleccionado no existe.");
                }

                stockActual = Convert.ToDecimal(Convert.ToDouble(scalar));
            }

            if (tipoNormalizado == "SALIDA" && cantidad > stockActual)
            {
                throw new InvalidOperationException("La salida supera el stock disponible.");
            }

            using var movCommand = connection.CreateCommand();
            movCommand.Transaction = transaction;
            movCommand.CommandText = @"
INSERT INTO MovimientoInventarioExt (ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES (@productoId, @fecha, @tipo, @cantidad, NULL, @nota);";

            movCommand.Parameters.AddWithValue("@productoId", productoId);
            movCommand.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd"));
            movCommand.Parameters.AddWithValue("@tipo", tipoNormalizado);
            movCommand.Parameters.AddWithValue("@cantidad", cantidad);
            movCommand.Parameters.AddWithValue("@nota", string.IsNullOrWhiteSpace(nota) ? "Ajuste manual" : nota.Trim());
            movCommand.ExecuteNonQuery();

            using var updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = @"
UPDATE ProductoExt 
SET StockActual = StockActual + @delta 
WHERE Id = @productoId;";

            var delta = tipoNormalizado == "ENTRADA" ? cantidad : -cantidad;
            updateCommand.Parameters.AddWithValue("@delta", delta);
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

    /// <summary>
    /// Elimina el producto si no tiene ítems de factura asociados; en caso contrario lo desactiva.
    /// Retorna <c>true</c> si fue eliminado físicamente, <c>false</c> si fue desactivado.
    /// </summary>
    public bool DeleteOrDeactivate(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(1) FROM ItemFactura WHERE ProductoId = @id;";
        checkCmd.Parameters.AddWithValue("@id", id);
        var tieneFacturas = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

        using var cmd = connection.CreateCommand();
        if (tieneFacturas)
        {
            cmd.CommandText = "UPDATE ProductoExt SET Activo = 0 WHERE Id = @id;";
        }
        else
        {
            cmd.CommandText = "DELETE FROM ProductoExt WHERE Id = @id;";
        }

        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return !tieneFacturas;
    }

    public void CrearProducto(ProductoCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            throw new InvalidOperationException("El código del producto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Unidad))
        {
            throw new InvalidOperationException("La unidad del producto es obligatoria.");
        }

        if (request.PrecioBase < 0 || request.StockActual < 0 || request.StockMinimo < 0)
        {
            throw new InvalidOperationException("Precio y stocks no pueden ser negativos.");
        }

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
INSERT INTO ProductoExt(Codigo, Nombre, Unidad, PrecioBase, StockActual, StockMinimo, Activo)
VALUES (@codigo, @nombre, @unidad, @precioBase, @stockActual, @stockMinimo, 1);
SELECT last_insert_rowid();";

            insertCommand.Parameters.AddWithValue("@codigo", request.Codigo.Trim());
            insertCommand.Parameters.AddWithValue("@nombre", request.Nombre.Trim());
            insertCommand.Parameters.AddWithValue("@unidad", request.Unidad.Trim());
            insertCommand.Parameters.AddWithValue("@precioBase", request.PrecioBase);
            insertCommand.Parameters.AddWithValue("@stockActual", request.StockActual);
            insertCommand.Parameters.AddWithValue("@stockMinimo", request.StockMinimo);

            var newProductId = Convert.ToInt64(insertCommand.ExecuteScalar());

            if (request.StockActual > 0)
            {
                using var movementCommand = connection.CreateCommand();
                movementCommand.Transaction = transaction;
                movementCommand.CommandText = @"
INSERT INTO MovimientoInventarioExt (ProductoId, Fecha, Tipo, Cantidad, ReferenciaFacturaId, Nota)
VALUES (@productoId, @fecha, 'ENTRADA', @cantidad, NULL, @nota);";
                movementCommand.Parameters.AddWithValue("@productoId", newProductId);
                movementCommand.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd"));
                movementCommand.Parameters.AddWithValue("@cantidad", request.StockActual);
                movementCommand.Parameters.AddWithValue("@nota", "Stock inicial");
                movementCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            transaction.Rollback();
            throw new InvalidOperationException("Ya existe un producto con el mismo código.");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
