namespace Embotelladora.Facturacion.Desktop.Features.Inventario;

internal sealed class InventoryCardDto
{
    public decimal InventoryValue { get; init; }
    public int TotalUnitsInStock { get; init; }
    public int TotalProducts { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
}

internal sealed class ProductGridRowDto
{
    public long Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public decimal PrecioBase { get; init; }
    public decimal StockActual { get; init; }
    public decimal StockMinimo { get; init; }
    public string Unidad { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
}

internal sealed class ProductCreateRequest
{
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public decimal PrecioBase { get; init; }
    public decimal StockActual { get; init; }
    public decimal StockMinimo { get; init; }
    public string Unidad { get; init; } = string.Empty;
}

internal sealed class MovementHistoryDto
{
    public long Id { get; init; }
    public string ProductoCodigo { get; init; } = string.Empty;
    public string ProductoNombre { get; init; } = string.Empty;
    public string TipoMovimiento { get; init; } = string.Empty;
    public decimal Cantidad { get; init; }
    public string Fecha { get; init; } = string.Empty;
    public string Usuario { get; init; } = string.Empty;
    public string Notas { get; init; } = string.Empty;
}
