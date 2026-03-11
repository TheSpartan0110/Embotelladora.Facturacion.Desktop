namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

// Invoice DTOs (deprecated - replaced by InvoiceCreateRequest and InvoiceItemInput)
internal sealed class InvoiceItemDetail
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Subtotal => Cantidad * Precio;

    public InvoiceItemDetail Clone() => new()
    {
        Id = Id,
        Descripcion = Descripcion,
        Cantidad = Cantidad,
        Precio = Precio
    };
}

internal sealed class Invoice
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public long MetodoPagoId { get; set; }
    public DateTime Fecha { get; set; }
    public string Notas { get; set; } = string.Empty;
    public List<InvoiceItemDetail> Items { get; set; } = [];
}

internal sealed class InvoiceGridRowDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

internal sealed class InvoiceCustomerLookupDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string DisplayName => $"{Nombre} ({Nit})";
}

internal sealed class InvoiceSummaryDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoSaldo => Saldo < 0
        ? $"A favor ($ {Math.Abs(Saldo):N0})"
        : Saldo == 0
            ? "Pagado ($ 0)"
            : $"Pendiente ($ {Saldo:N0})";
}

internal sealed class InvoiceListResumenDto
{
    public int TotalFacturas { get; set; }
    public decimal TotalFacturado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int FacturasPagadas { get; set; }
}

internal sealed class InvoicePrintDetailDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Retencion { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public string Notas { get; set; } = string.Empty;
    public List<InvoicePrintItemDto> Items { get; set; } = [];
}

internal sealed class InvoicePrintItemDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal TotalLinea { get; set; }
}

// Payment DTOs
internal sealed class Payment
{
    public long Id { get; set; }
    public long FacturaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public long MetodoPagoId { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}

internal sealed class PaymentLookupDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string DisplayName => $"{Numero} - {Cliente} (Saldo: $ {Saldo:N0})";
}

internal sealed class PaymentGridRowDto
{
    public long Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}

internal sealed class PaymentCreateRequest
{
    public long FacturaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Valor { get; set; }
    public long? MetodoPagoId { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}

internal sealed class PaymentSummaryDto
{
    public long Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
}

internal sealed class PaymentResumenDto
{
    public int TotalPagos { get; set; }
    public decimal MontoTotal { get; set; }
    public int ClientesPagaron { get; set; }
    public int FacturasPagadas { get; set; }
}

// Lookup/Common DTOs
internal sealed class PaymentMethodLookupDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

internal sealed class PaymentMethodDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int UsoFacturas { get; set; }
    public int UsoPagos { get; set; }
    public bool PuedeEliminar => UsoFacturas == 0 && UsoPagos == 0;
    public string UsoResumen => $"Facturas: {UsoFacturas:N0} · Pagos: {UsoPagos:N0}";
}

internal sealed class ProductLookupDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal StockActual { get; set; }
    public string DisplayName => $"{Codigo} - {Nombre}";
}

internal sealed class InventorySummaryDto
{
    public decimal TotalValue { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

// Invoice Creation DTOs
internal sealed class InvoiceCreateRequest
{
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public long ClienteId { get; set; }
    public long MetodoPagoId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Retencion { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public string Notas { get; set; } = string.Empty;
    public List<InvoiceItemInput> Items { get; set; } = [];
}

internal sealed class InvoiceItemInput
{
    public long ProductId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal TotalLinea { get; set; }
}

// Cartera DTOs
internal sealed class CarteraResumenDto
{
    public int ClientesConSaldo { get; set; }
    public int FacturasPendientes { get; set; }
    public decimal TotalPorCobrar { get; set; }
    public decimal SaldoVencido { get; set; }
    public int ClientesConSaldoAFavor { get; set; }
    public decimal TotalSaldoAFavor { get; set; }
}

internal sealed class FacturaPendienteDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public int DiasTranscurridos { get; set; }
    public string EstadoVencimiento { get; set; } = string.Empty;
}

internal sealed class ClienteSaldoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int FacturasPendientes { get; set; }
    public decimal SaldoTotal { get; set; }
    public decimal SaldoVencido { get; set; }
}

internal sealed class EdadSaldoDto
{
    public string RangoEdad { get; set; } = string.Empty;
    public int CantidadFacturas { get; set; }
    public decimal TotalSaldo { get; set; }
    public decimal PorcentajeSaldo { get; set; }
}

internal sealed class ClienteSaldoFavorDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int FacturasConCredito { get; set; }
    public decimal SaldoAFavor { get; set; }
}

// Balance DTOs
internal enum BalancePeriodo
{
    Diario,
    Quincenal,
    Mensual,
    Anual,
    Total
}

internal sealed class BalanceResumenDto
{
    public decimal TotalFacturado { get; set; }
    public decimal TotalRecaudado { get; set; }
    public decimal CuentasPorCobrar { get; set; }
    public decimal BalanceNeto { get; set; }
    public int FacturasEmitidas { get; set; }
}

internal sealed class BalanceMensualDto
{
    public string Periodo { get; set; } = string.Empty;
    public string MesNombre { get; set; } = string.Empty;
    public decimal TotalFacturado { get; set; }
    public decimal TotalRecaudado { get; set; }
    public int NumeroFacturas { get; set; }
}

internal sealed class TopClienteDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int NumeroFacturas { get; set; }
    public decimal TotalFacturado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal PorcentajeTotal { get; set; }
}

internal sealed class BalanceFacturaDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public string Estado { get; set; } = string.Empty;
}

internal sealed class BalancePagoDto
{
    public long Id { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}

internal sealed class EstadisticaMensualDto
{
    public string Periodo { get; set; } = string.Empty;
    public int TotalFacturas { get; set; }
    public int ClientesUnicos { get; set; }
    public decimal PromedioFactura { get; set; }
    public decimal FacturaMayor { get; set; }
    public decimal FacturaMenor { get; set; }
}

internal sealed class BalanceProductoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
    public decimal ValorVentasTotales { get; set; }
    public decimal PrecioPromedioPago { get; set; }
}

// Inventario DTOs
internal sealed class InventarioResumenDto
{
    public int TotalProductos { get; set; }
    public decimal ValorInventario { get; set; }
    public int ProductosStockBajo { get; set; }
    public int ProductosAgotados { get; set; }
}

internal sealed class ProductoInventarioDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal ValorStock { get; set; }
    public string EstadoStock { get; set; } = string.Empty;
}

internal sealed class MovimientoInventarioDto
{
    public long Id { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string Referencia { get; set; } = string.Empty;
}

internal sealed class ProductoStockBajoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal Faltante { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal ValorFaltante { get; set; }
}

internal sealed class ProductoCreateRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
}
