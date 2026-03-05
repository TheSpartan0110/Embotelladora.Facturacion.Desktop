using Embotelladora.Facturacion.Desktop.Data;

namespace Embotelladora.Facturacion.Desktop.Features.Inventario;

internal sealed class InventoryRepository
{
    public InventoryCardDto GetInventorySummary()
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        // Obtener valor total del inventario
        using var valueCommand = connection.CreateCommand();
        valueCommand.CommandText = "SELECT IFNULL(SUM(StockActual * PrecioBase), 0) FROM Producto;";
        var inventoryValue = Convert.ToDecimal(valueCommand.ExecuteScalar());

        // Obtener total de unidades en stock
        using var unitsCommand = connection.CreateCommand();
        unitsCommand.CommandText = "SELECT IFNULL(SUM(StockActual), 0) FROM Producto;";
        var totalUnits = Convert.ToInt32(unitsCommand.ExecuteScalar());

        // Obtener total de productos
        using var productsCommand = connection.CreateCommand();
        productsCommand.CommandText = "SELECT COUNT(*) FROM Producto;";
        var totalProducts = Convert.ToInt32(productsCommand.ExecuteScalar());

        // Obtener productos con stock bajo
        using var lowStockCommand = connection.CreateCommand();
        lowStockCommand.CommandText = "SELECT COUNT(*) FROM Producto WHERE StockActual > 0 AND StockActual <= StockMinimo;";
        var lowStockCount = Convert.ToInt32(lowStockCommand.ExecuteScalar());

        // Obtener productos sin stock
        using var outOfStockCommand = connection.CreateCommand();
        outOfStockCommand.CommandText = "SELECT COUNT(*) FROM Producto WHERE StockActual = 0;";
        var outOfStockCount = Convert.ToInt32(outOfStockCommand.ExecuteScalar());

        return new InventoryCardDto
        {
            InventoryValue = inventoryValue,
            TotalUnitsInStock = totalUnits,
            TotalProducts = totalProducts,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount
        };
    }

    public List<ProductGridRowDto> GetProducts(string? search = null)
    {
        var result = new List<ProductGridRowDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        var sql = @"
SELECT Id, Codigo, Nombre, Descripcion, PrecioBase, StockActual, StockMinimo, Unidad
FROM Producto";

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += @" WHERE Codigo LIKE @search OR Nombre LIKE @search OR Descripcion LIKE @search";
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }

        sql += " ORDER BY Nombre ASC;";
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var stockActual = reader.GetInt32(5);
            var stockMinimo = reader.GetInt32(6);
            var estado = stockActual == 0 ? "Agotado" : (stockActual <= stockMinimo ? "Bajo" : "OK");

            result.Add(new ProductGridRowDto
            {
                Id = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nombre = reader.GetString(2),
                Descripcion = reader.GetString(3),
                PrecioBase = Convert.ToDecimal(reader.GetDouble(4)),
                StockActual = stockActual,
                StockMinimo = stockMinimo,
                Unidad = reader.GetString(7),
                Estado = estado
            });
        }

        return result;
    }

    public List<MovementHistoryDto> GetMovementHistory(int limit = 50)
    {
        var result = new List<MovementHistoryDto>();

        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT m.Id, p.Codigo, p.Nombre, m.TipoMovimiento, m.Cantidad, m.Fecha, m.Usuario, m.Notas
FROM Movimiento m
INNER JOIN Producto p ON p.Id = m.ProductoId
ORDER BY m.Fecha DESC
LIMIT @limit;";
        command.Parameters.AddWithValue("@limit", limit);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MovementHistoryDto
            {
                Id = reader.GetInt64(0),
                ProductoCodigo = reader.GetString(1),
                ProductoNombre = reader.GetString(2),
                TipoMovimiento = reader.GetString(3),
                Cantidad = Convert.ToDecimal(reader.GetDouble(4)),
                Fecha = reader.GetString(5),
                Usuario = reader.IsDBNull(6) ? "Sistema" : reader.GetString(6),
                Notas = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
            });
        }

        return result;
    }

    public void UpdateProduct(long id, ProductCreateRequest request)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Producto
SET Codigo = @codigo, Nombre = @nombre, Descripcion = @descripcion,
    PrecioBase = @precio, StockMinimo = @stockMinimo, Unidad = @unidad
WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@codigo", request.Codigo.Trim());
        command.Parameters.AddWithValue("@nombre", request.Nombre.Trim());
        command.Parameters.AddWithValue("@descripcion", request.Descripcion.Trim());
        command.Parameters.AddWithValue("@precio", request.PrecioBase);
        command.Parameters.AddWithValue("@stockMinimo", request.StockMinimo);
        command.Parameters.AddWithValue("@unidad", request.Unidad.Trim());

        command.ExecuteNonQuery();
    }

    public void DeleteProduct(long id)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Producto WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public void AdjustStock(long productId, decimal quantity, string tipoMovimiento, string notas = "")
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // Registrar el movimiento
        using var movementCommand = connection.CreateCommand();
        movementCommand.Transaction = transaction;
        movementCommand.CommandText = @"
INSERT INTO Movimiento(ProductoId, TipoMovimiento, Cantidad, Fecha, Usuario, Notas)
VALUES(@productId, @tipo, @cantidad, @fecha, @usuario, @notas);";
        movementCommand.Parameters.AddWithValue("@productId", productId);
        movementCommand.Parameters.AddWithValue("@tipo", tipoMovimiento);
        movementCommand.Parameters.AddWithValue("@cantidad", quantity);
        movementCommand.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        movementCommand.Parameters.AddWithValue("@usuario", Environment.UserName);
        movementCommand.Parameters.AddWithValue("@notas", notas);
        movementCommand.ExecuteNonQuery();

        // Actualizar el stock del producto
        using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = @"
UPDATE Producto
SET StockActual = StockActual + @cantidad
WHERE Id = @id;";
        updateCommand.Parameters.AddWithValue("@cantidad", quantity);
        updateCommand.Parameters.AddWithValue("@id", productId);
        updateCommand.ExecuteNonQuery();

        transaction.Commit();
    }
}
