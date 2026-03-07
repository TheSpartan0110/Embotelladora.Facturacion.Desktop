namespace Embotelladora.Facturacion.Desktop.Features.Dashboard;

internal sealed class DashboardSnapshot
{
    public decimal TotalFacturado { get; init; }
    public decimal SaldoPendiente { get; init; }
    public decimal TotalPagos { get; init; }
    public int ClientesActivos { get; init; }
    public int FacturasPendientes { get; init; }
    public int PagosRegistrados { get; init; }
}

internal sealed class DashboardStatusDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

internal sealed class DashboardPaymentMethodDto
{
    public string MetodoPago { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}
