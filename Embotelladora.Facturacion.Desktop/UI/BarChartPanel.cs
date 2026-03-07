using System.Drawing.Drawing2D;

namespace Embotelladora.Facturacion.Desktop.UI;

internal sealed class BarChartPanel : Panel
{
    private List<BarChartSeries> _series = [];
    private List<string> _labels = [];

    public BarChartPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
    }

    public void SetData(List<string> labels, params BarChartSeries[] series)
    {
        _labels = labels;
        _series = [.. series];
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = ClientRectangle;
        if (_series.Count == 0 || _labels.Count == 0)
        {
            DrawEmptyState(g, rect);
            return;
        }

        var chartLeft = rect.Left + 72;
        var chartTop = rect.Top + 30;
        var chartRight = rect.Right - 16;
        var chartBottom = rect.Bottom - 36;
        var chartWidth = chartRight - chartLeft;
        var chartHeight = chartBottom - chartTop;

        if (chartWidth <= 0 || chartHeight <= 0) return;

        var maxValue = _series.SelectMany(s => s.Values).DefaultIfEmpty(0).Max();
        if (maxValue <= 0) maxValue = 1;
        var gridMax = RoundUpNice(maxValue);

        using var penGrid = new Pen(Color.FromArgb(242, 242, 242), 1);
        using var fontAxis = new Font("Segoe UI", 7.5f);
        using var brushAxis = new SolidBrush(Color.FromArgb(150, 150, 150));

        const int gridLines = 5;
        for (var i = 0; i <= gridLines; i++)
        {
            var y = chartBottom - (int)((float)i / gridLines * chartHeight);
            g.DrawLine(penGrid, chartLeft, y, chartRight, y);
            var val = gridMax * i / gridLines;
            var label = FormatShort(val);
            var sz = g.MeasureString(label, fontAxis);
            g.DrawString(label, fontAxis, brushAxis, chartLeft - sz.Width - 6, y - sz.Height / 2);
        }

        var groupCount = _labels.Count;
        var seriesCount = _series.Count;
        var groupWidth = (float)chartWidth / groupCount;
        var totalBarWidth = Math.Min(groupWidth * 0.7f, 56f * seriesCount);
        var barWidth = totalBarWidth / seriesCount;
        var gap = (groupWidth - totalBarWidth) / 2;

        using var fontLabel = new Font("Segoe UI", 7.5f);
        using var fontValue = new Font("Segoe UI", 7, FontStyle.Bold);

        for (var gi = 0; gi < groupCount; gi++)
        {
            var groupX = chartLeft + gi * groupWidth;

            for (var si = 0; si < seriesCount; si++)
            {
                if (gi >= _series[si].Values.Count) continue;

                var value = _series[si].Values[gi];
                var barHeight = (float)((double)value / (double)gridMax * chartHeight);
                var barX = groupX + gap + si * barWidth;
                var barY = chartBottom - barHeight;

                using var brush = new SolidBrush(_series[si].Color);
                var barRect = new RectangleF(barX + 1, barY, barWidth - 2, barHeight);

                if (barHeight > 6)
                {
                    using var path = RoundedTopRect(barRect, 4);
                    g.FillPath(brush, path);
                }
                else if (barHeight > 0)
                {
                    g.FillRectangle(brush, barRect);
                }

                if (value > 0)
                {
                    var vt = FormatShort(value);
                    var vs = g.MeasureString(vt, fontValue);
                    using var vb = new SolidBrush(Color.FromArgb(180, _series[si].Color));
                    g.DrawString(vt, fontValue, vb, barX + (barWidth - 2) / 2 - vs.Width / 2, barY - vs.Height - 1);
                }
            }

            var lt = _labels[gi];
            var ls = g.MeasureString(lt, fontLabel);
            g.DrawString(lt, fontLabel, brushAxis, groupX + groupWidth / 2 - ls.Width / 2, chartBottom + 6);
        }

        if (seriesCount > 1)
        {
            var lx = chartRight;
            using var fontLeg = new Font("Segoe UI", 8);

            for (var si = seriesCount - 1; si >= 0; si--)
            {
                var t = _series[si].Name;
                var ts = g.MeasureString(t, fontLeg);
                lx -= (int)ts.Width + 22;
                using var lb = new SolidBrush(_series[si].Color);
                g.FillRectangle(lb, lx, chartTop - 20, 10, 10);
                g.DrawString(t, fontLeg, brushAxis, lx + 14, chartTop - 22);
            }
        }
    }

    private static void DrawEmptyState(Graphics g, Rectangle rect)
    {
        using var font = new Font("Segoe UI", 10, FontStyle.Italic);
        using var brush = new SolidBrush(Color.FromArgb(180, 180, 180));
        var text = "Sin datos disponibles";
        var sz = g.MeasureString(text, font);
        g.DrawString(text, font, brush, rect.Left + (rect.Width - sz.Width) / 2, rect.Top + (rect.Height - sz.Height) / 2);
    }

    private static GraphicsPath RoundedTopRect(RectangleF r, float radius)
    {
        var p = new GraphicsPath();
        var d = radius * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddLine(r.Right, r.Y + radius, r.Right, r.Bottom);
        p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
        p.CloseFigure();
        return p;
    }

    private static decimal RoundUpNice(decimal value)
    {
        if (value <= 0) return 100;
        var exp = (int)Math.Floor(Math.Log10((double)value));
        var mag = (decimal)Math.Pow(10, exp);
        var norm = value / mag;
        decimal nice = norm switch { <= 1.2m => 1.5m, <= 2m => 2m, <= 3.5m => 4m, <= 5m => 5m, <= 7.5m => 8m, _ => 10m };
        return nice * mag;
    }

    private static string FormatShort(decimal value) => value switch
    {
        >= 1_000_000 => $"${value / 1_000_000:N1}M",
        >= 10_000 => $"${value / 1_000:N0}K",
        >= 1_000 => $"${value / 1_000:N1}K",
        _ => $"${value:N0}"
    };
}

internal sealed class BarChartSeries
{
    public string Name { get; set; } = string.Empty;
    public Color Color { get; set; } = Color.FromArgb(45, 111, 26);
    public List<decimal> Values { get; set; } = [];
}
