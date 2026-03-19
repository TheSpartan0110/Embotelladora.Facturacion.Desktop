using ClosedXML.Excel;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

/// <summary>
/// Implementación de <see cref="IExcelExportService"/> usando ClosedXML.
/// </summary>
internal sealed class ClosedXmlExcelExportService : IExcelExportService
{
    // Prefijos que Excel interpreta como fórmulas (inyección CSV/Excel)
    private static readonly char[] FormulaStartChars = ['=', '+', '-', '@'];

    /// <inheritdoc/>
    public void ExportInvoice(InvoicePrintDetailDto invoice, string filePath)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Factura");

        var row = 1;

        // ── Encabezado ──────────────────────────────────────────────────────
        ws.Cell(row, 1).Value = "FACTURA";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        ws.Range(row, 1, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = "N° Factura";
        ws.Cell(row, 2).Value = Sanitize(invoice.Numero);
        ws.Cell(row, 3).Value = "Fecha";
        ws.Cell(row, 4).Value = invoice.Fecha.ToString("yyyy-MM-dd");
        row++;

        ws.Cell(row, 1).Value = "Cliente";
        ws.Cell(row, 2).Value = Sanitize(invoice.Cliente);
        ws.Cell(row, 3).Value = "NIT";
        ws.Cell(row, 4).Value = Sanitize(invoice.Nit);
        row++;

        ws.Cell(row, 1).Value = "Dirección";
        ws.Cell(row, 2).Value = Sanitize(invoice.Direccion);
        ws.Cell(row, 3).Value = "Método de pago";
        ws.Cell(row, 4).Value = Sanitize(invoice.MetodoPago);
        row++;

        ws.Cell(row, 1).Value = "Estado";
        ws.Cell(row, 2).Value = Sanitize(invoice.Estado);
        row++;

        if (!string.IsNullOrWhiteSpace(invoice.Notas))
        {
            ws.Cell(row, 1).Value = "Notas";
            ws.Cell(row, 2).Value = Sanitize(invoice.Notas);
            ws.Range(row, 2, row, 5).Merge();
            row++;
        }

        row++; // fila en blanco

        // ── Cabecera de tabla de ítems ───────────────────────────────────────
        var headerRow = row;
        ws.Cell(row, 1).Value = "Código";
        ws.Cell(row, 2).Value = "Descripción";
        ws.Cell(row, 3).Value = "Cantidad";
        ws.Cell(row, 4).Value = "Precio Unitario";
        ws.Cell(row, 5).Value = "Total Línea";

        var headerRange = ws.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(45, 111, 26);
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;

        // ── Filas de ítems ────────────────────────────────────────────────────
        foreach (var item in invoice.Items ?? [])
        {
            ws.Cell(row, 1).Value = Sanitize(item.Codigo);
            ws.Cell(row, 2).Value = Sanitize(item.Descripcion);
            ws.Cell(row, 3).Value = item.Cantidad;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.##";
            ws.Cell(row, 4).Value = item.PrecioUnitario;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = item.TotalLinea;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 247, 245);
            }

            row++;
        }

        // Borde de tabla
        ws.Range(headerRow, 1, row - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(headerRow, 1, row - 1, 5).Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        row++; // fila en blanco

        // ── Totales ────────────────────────────────────────────────────────────
        ws.Cell(row, 4).Value = "Subtotal";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = invoice.Subtotal;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        row++;

        if (invoice.Retencion != 0)
        {
            ws.Cell(row, 4).Value = "Retención";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = invoice.Retencion;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 4).Value = "TOTAL";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Font.FontSize = 12;
        ws.Cell(row, 5).Value = invoice.Total;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.Font.FontSize = 12;
        row++;

        ws.Cell(row, 4).Value = "Saldo Pendiente";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = invoice.Saldo;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

        // ── Ajustar anchos ────────────────────────────────────────────────────
        ws.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }

    /// <summary>
    /// Sanitiza valores de texto para evitar inyección de fórmulas en Excel.
    /// Valores que comienzan con =, +, -, @ se prefijan con una comilla simple.
    /// </summary>
    internal static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return Array.IndexOf(FormulaStartChars, value[0]) >= 0
            ? "'" + value
            : value;
    }
}
