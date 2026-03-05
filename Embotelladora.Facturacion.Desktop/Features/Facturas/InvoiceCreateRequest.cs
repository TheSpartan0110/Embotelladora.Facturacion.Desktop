namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class InvoiceCreateRequest
{
    public string Numero { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public long ClienteId { get; init; }
    public long? MetodoPagoId { get; init; }
    public string Estado { get; init; } = "Enviada";
    public decimal Subtotal { get; init; }
    public decimal IvaPorcentaje { get; init; }
    public decimal IvaValor { get; init; }
    public decimal Retencion { get; init; }
    public decimal Total { get; init; }
    public decimal Saldo { get; init; }
    public string Notas { get; init; } = string.Empty;
    public IReadOnlyList<InvoiceItemInput> Items { get; init; } = [];
}

internal sealed class InvoiceItemInput
{
    public long ProductId { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public decimal Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public bool AplicaIva { get; init; }
    public decimal TotalLinea { get; init; }
}
