using System.Drawing.Drawing2D;

namespace Embotelladora.Facturacion.Desktop.UI;

internal sealed class DonutChartPanel : Panel
{
    private List<DonutSlice> _slices = [];
    private string _centerText = string.Empty;
    private string _centerLabel = string.Empty;

    public DonutChartPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
    }

    public void SetData(List<DonutSlice> slices, string centerText = "", string centerLabel = "")
    {
        _slices = slices;
        _centerText = centerText;
        _centerLabel = centerLabel;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = ClientRectangle;
        if (_slices.Count == 0)
        {
            DrawEmpty(g, rect);
            return;
        }

        var total = _slices.Sum(s => s.Value);
        if (total <= 0) { DrawEmpty(g, rect); return; }

        // Horizontal layout: fixed 40/60 split for consistent alignment
        using var fontLeg = new Font("Segoe UI", 8.5f);
        using var brushText = new SolidBrush(Color.FromArgb(70, 70, 70));

        var legendItemH = 20;
        var legendTotalH = _slices.Count * legendItemH;
        var legendAreaW = (int)(rect.Width * 0.40f);
        var donutAreaLeft = rect.Left + legendAreaW + 4;
        var donutAreaWidth = rect.Right - donutAreaLeft - 4;
        var chartDiam = Math.Min(donutAreaWidth, rect.Height - 8);
        chartDiam = Math.Max(chartDiam, 50);

        var chartRect = new Rectangle(
            donutAreaLeft + (donutAreaWidth - chartDiam) / 2,
            rect.Top + (rect.Height - chartDiam) / 2,
            chartDiam, chartDiam);

        var holeRatio = 0.58f;
        var holeDiam = (int)(chartDiam * holeRatio);
        var holeRect = new Rectangle(
            chartRect.X + (chartDiam - holeDiam) / 2,
            chartRect.Y + (chartDiam - holeDiam) / 2,
            holeDiam, holeDiam);

        var startAngle = -90f;
        foreach (var slice in _slices)
        {
            var sweep = (float)((double)slice.Value / (double)total * 360);
            using var brush = new SolidBrush(slice.Color);
            g.FillPie(brush, chartRect, startAngle, Math.Max(sweep, 0.5f));
            startAngle += sweep;
        }

        using var holeBrush = new SolidBrush(BackColor);
        g.FillEllipse(holeBrush, holeRect);

        if (!string.IsNullOrEmpty(_centerText))
        {
            var maxSize = Math.Max(holeDiam / 6f, 8f);
            var availWidth = holeDiam * 0.82f;
            using var measureFont = new Font("Segoe UI", maxSize, FontStyle.Bold);
            var measured = g.MeasureString(_centerText, measureFont);
            var fontSize = measured.Width > availWidth
                ? Math.Max(7f, maxSize * availWidth / measured.Width)
                : maxSize;
            using var fc = new Font("Segoe UI", fontSize, FontStyle.Bold);
            using var fl = new Font("Segoe UI", Math.Max(fontSize * 0.55f, 7f));
            using var bc = new SolidBrush(Color.FromArgb(33, 33, 33));
            using var bl = new SolidBrush(Color.FromArgb(140, 140, 140));

            var szC = g.MeasureString(_centerText, fc);
            var cy = holeRect.Y + holeRect.Height / 2f;

            g.DrawString(_centerText, fc, bc,
                holeRect.X + holeRect.Width / 2f - szC.Width / 2,
                cy - szC.Height / 2 - 2);

            if (!string.IsNullOrEmpty(_centerLabel))
            {
                var szL = g.MeasureString(_centerLabel, fl);
                g.DrawString(_centerLabel, fl, bl,
                    holeRect.X + holeRect.Width / 2f - szL.Width / 2,
                    cy + szC.Height / 2 - 6);
            }
        }

        // Draw legend on the left, vertically centered
        var ly = rect.Top + (rect.Height - legendTotalH) / 2;
        foreach (var slice in _slices)
        {
            if (ly + legendItemH > rect.Bottom) break;
            using var sb = new SolidBrush(slice.Color);
            g.FillRectangle(sb, rect.Left + 6, ly + 4, 10, 10);
            var pct = (double)slice.Value / (double)total * 100;
            g.DrawString($"{slice.Label}: {slice.Value:N0} ({pct:N0}%)", fontLeg, brushText, rect.Left + 22, ly);
            ly += legendItemH;
        }
    }

    private static void DrawEmpty(Graphics g, Rectangle rect)
    {
        using var f = new Font("Segoe UI", 10, FontStyle.Italic);
        using var b = new SolidBrush(Color.FromArgb(180, 180, 180));
        var t = "Sin datos";
        var s = g.MeasureString(t, f);
        g.DrawString(t, f, b, rect.Left + (rect.Width - s.Width) / 2, rect.Top + (rect.Height - s.Height) / 2);
    }
}

internal sealed class DonutSlice
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public Color Color { get; set; }
}
