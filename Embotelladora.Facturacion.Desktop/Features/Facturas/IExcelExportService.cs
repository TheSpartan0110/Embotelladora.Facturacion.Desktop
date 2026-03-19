namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

/// <summary>
/// Servicio para exportar facturas a hojas de cálculo Excel (.xlsx).
/// </summary>
internal interface IExcelExportService
{
    /// <summary>
    /// Exporta el detalle de una factura al archivo indicado por <paramref name="filePath"/>.
    /// </summary>
    /// <param name="invoice">DTO con el detalle completo de la factura.</param>
    /// <param name="filePath">Ruta absoluta del archivo .xlsx de destino.</param>
    void ExportInvoice(InvoicePrintDetailDto invoice, string filePath);
}
