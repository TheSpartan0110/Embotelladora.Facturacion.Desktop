namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class InvoiceCustomerLookupDto
{
    public long Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Nit { get; init; } = string.Empty;
    public string DisplayName => $"{Nombre} ({Nit})";
}

internal sealed class PaymentMethodLookupDto
{
    public long Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}

internal sealed class ProductLookupDto
{
    public long Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public decimal PrecioBase { get; init; }
    public decimal StockActual { get; init; }
    public string DisplayName => $"{Codigo} - {Nombre}";
}

internal sealed class InvoiceSummaryDto
{
    public long Id { get; init; }
    public string Numero { get; init; } = string.Empty;
    public string Fecha { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public decimal Saldo { get; init; }
    public string Estado { get; init; } = string.Empty;
}
