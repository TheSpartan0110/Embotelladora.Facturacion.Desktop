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
