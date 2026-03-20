using ClosedXML.Excel;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

internal sealed class ClosedXmlExcelExportService : IExcelExportService
{
    /// <summary>
    /// Sanitiza un valor string para prevenir inyección de fórmulas en Excel.
    /// Agrega prefijo "'" si el valor inicia con =, +, -, o @.
    /// </summary>
    private static string SanitizeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value[0] is '=' or '+' or '-' or '@'
            ? $"'{value}"
            : value;
    }

    public void ExportInvoice(InvoicePrintDetailDto invoice, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Factura");

        // ── Encabezado ──────────────────────────────────────────────────────────
        ws.Cell(1, 1).Value = "Número";
        ws.Cell(1, 2).Value = SanitizeValue(invoice.Numero);

        ws.Cell(2, 1).Value = "Fecha";
        ws.Cell(2, 2).Value = invoice.Fecha.ToString("yyyy-MM-dd");

        ws.Cell(3, 1).Value = "Cliente";
        ws.Cell(3, 2).Value = SanitizeValue(invoice.Cliente);

        ws.Cell(4, 1).Value = "NIT";
        ws.Cell(4, 2).Value = SanitizeValue(invoice.Nit);

        ws.Cell(5, 1).Value = "Método de Pago";
        ws.Cell(5, 2).Value = SanitizeValue(invoice.MetodoPago);

        ws.Cell(6, 1).Value = "Estado";
        ws.Cell(6, 2).Value = SanitizeValue(invoice.Estado);

        // Estilo encabezado — etiquetas en negrita
        var headerRange = ws.Range(1, 1, 6, 1);
        headerRange.Style.Font.Bold = true;

        // ── Tabla de ítems ───────────────────────────────────────────────────────
        const int itemsStartRow = 8;

        ws.Cell(itemsStartRow, 1).Value = "Código";
        ws.Cell(itemsStartRow, 2).Value = "Descripción";
        ws.Cell(itemsStartRow, 3).Value = "Cantidad";
        ws.Cell(itemsStartRow, 4).Value = "Precio Unitario";
        ws.Cell(itemsStartRow, 5).Value = "Subtotal";

        var tableHeaderRange = ws.Range(itemsStartRow, 1, itemsStartRow, 5);
        tableHeaderRange.Style.Font.Bold = true;
        tableHeaderRange.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 210);

        var currentRow = itemsStartRow + 1;
        foreach (var item in invoice.Items)
        {
            ws.Cell(currentRow, 1).Value = SanitizeValue(item.Codigo);
            ws.Cell(currentRow, 2).Value = SanitizeValue(item.Descripcion);
            ws.Cell(currentRow, 3).Value = item.Cantidad;
            ws.Cell(currentRow, 4).Value = item.PrecioUnitario;
            ws.Cell(currentRow, 5).Value = item.TotalLinea;

            // Formato moneda
            ws.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";

            currentRow++;
        }

        // ── Totales ──────────────────────────────────────────────────────────────
        var totalsRow = currentRow + 1;

        ws.Cell(totalsRow, 4).Value = "Subtotal";
        ws.Cell(totalsRow, 5).Value = invoice.Subtotal;
        ws.Cell(totalsRow, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(totalsRow, 4).Style.Font.Bold = true;

        ws.Cell(totalsRow + 1, 4).Value = "Retención";
        ws.Cell(totalsRow + 1, 5).Value = invoice.Retencion;
        ws.Cell(totalsRow + 1, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(totalsRow + 1, 4).Style.Font.Bold = true;

        ws.Cell(totalsRow + 2, 4).Value = "Total";
        ws.Cell(totalsRow + 2, 5).Value = invoice.Total;
        ws.Cell(totalsRow + 2, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(totalsRow + 2, 4).Style.Font.Bold = true;
        ws.Cell(totalsRow + 2, 5).Style.Font.Bold = true;

        // ── Ajuste de columnas ───────────────────────────────────────────────────
        ws.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }
}
