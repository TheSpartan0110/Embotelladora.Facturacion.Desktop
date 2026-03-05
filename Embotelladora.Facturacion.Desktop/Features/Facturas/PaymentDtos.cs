namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class PaymentLookupDto
{
    public long Id { get; init; }
    public string Numero { get; init; } = string.Empty;
    public decimal Saldo { get; init; }
    public string Cliente { get; init; } = string.Empty;
    public string DisplayName => $"{Numero} - {Cliente} (Saldo: $ {Saldo:N0})";
}

internal sealed class PaymentGridRowDto
{
    public long Id { get; init; }
    public string NumeroFactura { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Fecha { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public string Referencia { get; init; } = string.Empty;
    public string Notas { get; init; } = string.Empty;
}

internal sealed class PaymentCreateRequest
{
    public long FacturaId { get; init; }
    public DateTime Fecha { get; init; }
    public decimal Valor { get; init; }
    public long? MetodoPagoId { get; init; }
    public string Referencia { get; init; } = string.Empty;
    public string Notas { get; init; } = string.Empty;
}

internal sealed class PaymentSummaryDto
{
    public long Id { get; init; }
    public string NumeroFactura { get; init; } = string.Empty;
    public string Fecha { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
}
