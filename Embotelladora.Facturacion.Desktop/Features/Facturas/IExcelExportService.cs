namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

/// <summary>
/// Servicio para exportar facturas a archivos Excel (.xlsx).
/// </summary>
internal interface IExcelExportService
{
    /// <summary>
    /// Exporta los detalles de una factura a un archivo Excel.
    /// </summary>
    /// <param name="invoice">Datos de la factura a exportar.</param>
    /// <param name="filePath">Ruta completa del archivo de destino (.xlsx).</param>
    void ExportInvoice(InvoicePrintDetailDto invoice, string filePath);
}
