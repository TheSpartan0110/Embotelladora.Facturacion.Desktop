using System.Drawing.Drawing2D;
using Embotelladora.Facturacion.Desktop.Features.Facturas;

namespace Embotelladora.Facturacion.Desktop;

public partial class Form1
{
    private void LoadFacturasPendientes(string? search)
    {
        var facturasPendientes = _carteraRepository.GetFacturasPendientes(search);
        _gridFacturasPendientes.DataSource = facturasPendientes;

        if (_gridFacturasPendientes.Columns.Count > 0)
        {
            _gridFacturasPendientes.Columns["Id"]!.Visible = false;
            _gridFacturasPendientes.Columns["Numero"]!.HeaderText = "N° Factura";
            _gridFacturasPendientes.Columns["Numero"]!.Width = 110;
            _gridFacturasPendientes.Columns["Fecha"]!.HeaderText = "Fecha";
            _gridFacturasPendientes.Columns["Fecha"]!.Width = 100;
            _gridFacturasPendientes.Columns["Cliente"]!.HeaderText = "Cliente";
            _gridFacturasPendientes.Columns["Cliente"]!.MinimumWidth = 180;
            _gridFacturasPendientes.Columns["Total"]!.HeaderText = "Total";
            _gridFacturasPendientes.Columns["Total"]!.DefaultCellStyle.Format = "C0";
            _gridFacturasPendientes.Columns["Total"]!.Width = 120;
            _gridFacturasPendientes.Columns["Saldo"]!.HeaderText = "Saldo";
            _gridFacturasPendientes.Columns["Saldo"]!.DefaultCellStyle.Format = "C0";
            _gridFacturasPendientes.Columns["Saldo"]!.Width = 120;
            _gridFacturasPendientes.Columns["DiasTranscurridos"]!.HeaderText = "Días";
            _gridFacturasPendientes.Columns["DiasTranscurridos"]!.Width = 70;
            _gridFacturasPendientes.Columns["EstadoVencimiento"]!.HeaderText = "Estado";
            _gridFacturasPendientes.Columns["EstadoVencimiento"]!.Width = 130;

            EnsureCarteraActionColumn();

            foreach (DataGridViewRow row in _gridFacturasPendientes.Rows)
            {
                if (row.DataBoundItem is FacturaPendienteDto factura)
                {
                    row.DefaultCellStyle.BackColor = factura.EstadoVencimiento switch
                    {
                        "Al día" => Color.FromArgb(232, 245, 232),
                        "Vencida 30 días" => Color.FromArgb(255, 255, 230),
                        "Vencida 60 días" => Color.FromArgb(255, 240, 230),
                        _ => Color.FromArgb(255, 220, 220)
                    };

                    if (factura.EstadoVencimiento is "Vencida 60 días" or "Vencida +90 días")
                    {
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
        }
    }

    private void EnsureCarteraActionColumn()
    {
        if (_gridFacturasPendientes.Columns.Contains("AccionPagar"))
        {
            return;
        }

        _gridFacturasPendientes.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "AccionPagar",
            HeaderText = "Acción",
            Text = "💰 Pagar",
            UseColumnTextForButtonValue = true,
            Width = 100,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void OnCarteraFacturaCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var column = _gridFacturasPendientes.Columns[e.ColumnIndex].Name;
        if (column == "AccionPagar")
        {
            OnCarteraRegistrarPago();
        }
    }

    private void OnCarteraRegistrarPago()
    {
        if (_gridFacturasPendientes.CurrentRow?.DataBoundItem is not FacturaPendienteDto factura)
        {
            return;
        }

        _pendingPaymentInvoiceId = factura.Id;
        ShowModule("NuevoPago");
    }

    private void PrintCartera()
    {
        var printDoc = new System.Drawing.Printing.PrintDocument
        {
            DocumentName = "Cartera - AceitesPro Facturación"
        };
        printDoc.DefaultPageSettings.Landscape = true;

        printDoc.PrintPage += (_, e) =>
        {
            var g = e.Graphics!;
            var b = e.MarginBounds;
            var y = (float)b.Top;

            using var fontTitle = new Font("Segoe UI", 14, FontStyle.Bold);
            using var fontSub = new Font("Segoe UI", 9);
            using var fontSection = new Font("Segoe UI", 10, FontStyle.Bold);
            using var fontLabel = new Font("Segoe UI", 8, FontStyle.Bold);
            using var fontValue = new Font("Segoe UI", 11, FontStyle.Bold);
            using var fontHeader = new Font("Segoe UI", 7, FontStyle.Bold);
            using var fontCell = new Font("Segoe UI", 7);
            using var penLine = new Pen(Color.FromArgb(210, 210, 210));
            using var brushGreen = new SolidBrush(Color.FromArgb(11, 52, 22));

            g.DrawString("AceitesPro \u2014 Cartera de Cuentas por Cobrar", fontTitle, Brushes.Black, b.Left, y);
            y += fontTitle.GetHeight(g) + 2;
            g.DrawString($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSub, Brushes.Gray, b.Left, y);
            y += fontSub.GetHeight(g) + 8;
            g.DrawLine(Pens.DarkGray, b.Left, y, b.Right, y);
            y += 12;

            var cw = b.Width / 5f;
            DrawCarteraPrintCard(g, b.Left, y, "Clientes con Saldo", _lblCarteraClientes.Text, fontLabel, fontValue, brushGreen);
            DrawCarteraPrintCard(g, b.Left + cw, y, "Facturas Pendientes", _lblCarteraFacturas.Text, fontLabel, fontValue, brushGreen);
            DrawCarteraPrintCard(g, b.Left + cw * 2, y, "Total por Cobrar", _lblCarteraTotalCobrar.Text, fontLabel, fontValue, brushGreen);
            DrawCarteraPrintCard(g, b.Left + cw * 3, y, "Saldo Vencido", _lblCarteraVencido.Text, fontLabel, fontValue, Brushes.DarkRed);
            DrawCarteraPrintCard(g, b.Left + cw * 4, y, "Saldo a Favor", _lblCarteraSaldoFavor.Text, fontLabel, fontValue, Brushes.DarkGreen);
            y += fontValue.GetHeight(g) + fontLabel.GetHeight(g) + 16;

            g.DrawLine(penLine, b.Left, y, b.Right, y);
            y += 10;

            y = DrawCarteraPrintGrid(g, _gridFacturasPendientes, "Facturas Pendientes de Pago", b, y, fontSection, fontHeader, fontCell, penLine);
            y += 10;

            if (y < b.Bottom - 80)
            {
                y = DrawCarteraPrintGrid(g, _gridClientesSaldo, "Clientes con Saldo Pendiente", b, y, fontSection, fontHeader, fontCell, penLine);
                y += 10;
            }

            if (y < b.Bottom - 80 && _gridClientesSaldoFavor.Rows.Count > 0)
            {
                y = DrawCarteraPrintGrid(g, _gridClientesSaldoFavor, "Clientes con Saldo a Favor", b, y, fontSection, fontHeader, fontCell, penLine);
                y += 10;
            }

            if (y < b.Bottom - 80)
            {
                y = DrawCarteraPrintGrid(g, _gridEdadSaldos, "Análisis de Edad de Saldos", b, y, fontSection, fontHeader, fontCell, penLine);
            }

            e.HasMorePages = false;
        };

        using var preview = new PrintPreviewDialog
        {
            Document = printDoc,
            WindowState = FormWindowState.Maximized
        };
        preview.ShowDialog(this);
    }

    private static void DrawCarteraPrintCard(Graphics g, float x, float y, string label, string value, Font fontLabel, Font fontValue, Brush valueBrush)
    {
        g.DrawString(label, fontLabel, Brushes.Gray, x + 4, y);
        g.DrawString(value, fontValue, valueBrush, x + 4, y + fontLabel.GetHeight(g) + 2);
    }

    private static float DrawCarteraPrintGrid(Graphics g, DataGridView grid, string title, Rectangle bounds, float y, Font fontSection, Font fontHeader, Font fontCell, Pen penLine)
    {
        if (grid.Rows.Count == 0)
        {
            return y;
        }

        g.DrawString(title, fontSection, Brushes.Black, bounds.Left, y);
        y += fontSection.GetHeight(g) + 6;

        var columns = new List<(string Header, int Index, float Weight)>();
        var totalWeight = 0f;
        foreach (DataGridViewColumn col in grid.Columns)
        {
            if (col.Visible && col is not DataGridViewButtonColumn)
            {
                columns.Add((col.HeaderText, col.Index, col.Width));
                totalWeight += col.Width;
            }
        }

        if (columns.Count == 0)
        {
            return y;
        }

        var availableWidth = (float)bounds.Width;
        var colWidths = columns.Select(c => c.Weight / totalWeight * availableWidth).ToArray();

        var headerHeight = fontHeader.GetHeight(g) + 8;
        using var headerBrush = new SolidBrush(Color.FromArgb(235, 238, 235));
        g.FillRectangle(headerBrush, bounds.Left, y, availableWidth, headerHeight);

        var x = (float)bounds.Left;
        for (var i = 0; i < columns.Count; i++)
        {
            g.DrawString(columns[i].Header, fontHeader, Brushes.Black, x + 3, y + 4);
            x += colWidths[i];
        }
        y += headerHeight;
        g.DrawLine(penLine, bounds.Left, y, bounds.Right, y);

        var rowHeight = fontCell.GetHeight(g) + 6;
        var alternate = false;
        using var altBrush = new SolidBrush(Color.FromArgb(248, 250, 248));

        foreach (DataGridViewRow row in grid.Rows)
        {
            if (y + rowHeight > bounds.Bottom - 40)
            {
                break;
            }

            if (alternate)
            {
                g.FillRectangle(altBrush, bounds.Left, y, availableWidth, rowHeight);
            }

            x = bounds.Left;
            for (var i = 0; i < columns.Count; i++)
            {
                var cellValue = row.Cells[columns[i].Index].FormattedValue?.ToString() ?? "";
                g.DrawString(cellValue, fontCell, Brushes.Black, x + 3, y + 3);
                x += colWidths[i];
            }

            y += rowHeight;
            g.DrawLine(penLine, bounds.Left, y, bounds.Right, y);
            alternate = !alternate;
        }

        return y;
    }
}
