using ClosedXML.Excel;

namespace Embotelladora.Facturacion.Desktop.Features.Facturas;

/// <summary>
/// Implementación de <see cref="IExcelExportService"/> usando la biblioteca ClosedXML.
/// </summary>
internal sealed class ClosedXmlExcelExportService : IExcelExportService
{
    // Caracteres que inician fórmulas en Excel — se prefijarán con comilla simple para
    // evitar inyección de fórmulas (CSV/Excel injection).
    private static readonly char[] FormulaStartChars = ['=', '+', '-', '@'];

    // Color de cabecera de la tabla de ítems (verde corporativo).
    private static readonly XLColor HeaderBackgroundColor = XLColor.FromArgb(45, 111, 26);

    /// <inheritdoc/>
    public void ExportInvoice(InvoicePrintDetailDto invoice, string filePath)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("La ruta de destino no puede estar vacía.", nameof(filePath));
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Factura");

        // ── Encabezado de la empresa ────────────────────────────────────────
        var row = 1;
        ws.Cell(row, 1).Value = "FACTURA";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = $"N° {Sanitize(invoice.Numero)}";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 12;
        ws.Range(row, 1, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = $"Fecha: {invoice.Fecha:dd/MM/yyyy}";
        ws.Range(row, 1, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = $"Estado: {Sanitize(invoice.Estado)}";
        ws.Range(row, 1, row, 5).Merge();
        row++;

        row++; // línea en blanco

        // ── Datos del cliente ───────────────────────────────────────────────
        ws.Cell(row, 1).Value = "CLIENTE";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Nombre:";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = Sanitize(invoice.Cliente);
        ws.Range(row, 2, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = "NIT:";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = Sanitize(invoice.Nit);
        ws.Range(row, 2, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = "Dirección:";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = Sanitize(invoice.Direccion);
        ws.Range(row, 2, row, 5).Merge();
        row++;

        ws.Cell(row, 1).Value = "Método de pago:";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = Sanitize(invoice.MetodoPago);
        ws.Range(row, 2, row, 5).Merge();
        row++;

        row++; // línea en blanco

        // ── Tabla de ítems ──────────────────────────────────────────────────
        var headerRow = row;
        ws.Cell(row, 1).Value = "Código";
        ws.Cell(row, 2).Value = "Descripción";
        ws.Cell(row, 3).Value = "Cantidad";
        ws.Cell(row, 4).Value = "Precio Unitario";
        ws.Cell(row, 5).Value = "Total Línea";

        var headerRange = ws.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = HeaderBackgroundColor;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;

        // Ítems
        var currencyFormat = "#,##0.00";
        foreach (var item in invoice.Items)
        {
            ws.Cell(row, 1).Value = Sanitize(item.Codigo);
            ws.Cell(row, 2).Value = Sanitize(item.Descripcion);
            ws.Cell(row, 3).Value = item.Cantidad;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 4).Value = item.PrecioUnitario;
            ws.Cell(row, 4).Style.NumberFormat.Format = currencyFormat;
            ws.Cell(row, 5).Value = item.TotalLinea;
            ws.Cell(row, 5).Style.NumberFormat.Format = currencyFormat;

            // Alternar color de filas
            if (row % 2 == 0)
            {
                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 250, 245);
            }

            row++;
        }

        // Borde de la tabla
        ws.Range(headerRow, 1, row - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(headerRow, 1, row - 1, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        row++; // línea en blanco

        // ── Totales ─────────────────────────────────────────────────────────
        ws.Cell(row, 4).Value = "Subtotal:";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = invoice.Subtotal;
        ws.Cell(row, 5).Style.NumberFormat.Format = currencyFormat;
        row++;

        if (invoice.Retencion != 0)
        {
            ws.Cell(row, 4).Value = "Retención:";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = invoice.Retencion;
            ws.Cell(row, 5).Style.NumberFormat.Format = currencyFormat;
            row++;
        }

        ws.Cell(row, 4).Value = "TOTAL:";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Font.FontSize = 11;
        ws.Cell(row, 5).Value = invoice.Total;
        ws.Cell(row, 5).Style.NumberFormat.Format = currencyFormat;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.Font.FontSize = 11;
        row++;

        ws.Cell(row, 4).Value = "Saldo pendiente:";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = invoice.Saldo;
        ws.Cell(row, 5).Style.NumberFormat.Format = currencyFormat;
        row++;

        // ── Notas ───────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(invoice.Notas))
        {
            row++;
            ws.Cell(row, 1).Value = "Notas:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).Value = Sanitize(invoice.Notas);
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Alignment.WrapText = true;
        }

        // ── Ajustar anchos de columna ────────────────────────────────────────
        ws.Columns().AdjustToContents();

        // Garantizar un ancho mínimo razonable para la columna de descripción
        if (ws.Column(2).Width < 30)
        {
            ws.Column(2).Width = 30;
        }

        workbook.SaveAs(filePath);
    }

    /// <summary>
    /// Sanitiza un valor de texto para evitar inyección de fórmulas en Excel.
    /// Los valores que empiezan con <c>=</c>, <c>+</c>, <c>-</c> o <c>@</c>
    /// se prefijan con una comilla simple (<c>'</c>).
    /// </summary>
    internal static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (Array.IndexOf(FormulaStartChars, value[0]) >= 0)
        {
            return "'" + value;
        }

        return value;
    }
}
