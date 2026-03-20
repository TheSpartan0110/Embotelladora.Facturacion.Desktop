namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal interface IExcelExportService
{
    void ExportInvoice(InvoicePrintDetailDto invoice, string filePath);
}
