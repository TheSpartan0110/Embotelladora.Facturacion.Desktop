namespace Embotelladora.Facturacion.Desktop.Features.Clientes;

internal sealed class CustomerGridRowDto
{
    public long Id { get; init; }
    public string Cliente { get; init; } = string.Empty;
    public string NitCedula { get; init; } = string.Empty;
    public string Contacto { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public int NumeroFacturas { get; init; }
}
