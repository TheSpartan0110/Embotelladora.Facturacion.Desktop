using System.Drawing.Drawing2D;

namespace Embotelladora.Facturacion.Desktop.UI;

internal sealed class HorizontalBarChartPanel : Panel
{
    private List<HBarItem> _items = [];

    public HorizontalBarChartPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
    }

    public void SetData(List<HBarItem> items)
    {
        _items = items;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = ClientRectangle;
        if (_items.Count == 0)
        {
            using var f = new Font("Segoe UI", 10, FontStyle.Italic);
            using var b = new SolidBrush(Color.FromArgb(180, 180, 180));
            var t = "Sin datos";
            var s = g.MeasureString(t, f);
            g.DrawString(t, f, b, rect.Left + (rect.Width - s.Width) / 2, rect.Top + (rect.Height - s.Height) / 2);
            return;
        }

        var maxVal = _items.Max(i => i.Value);
        if (maxVal <= 0) maxVal = 1;

        var labelW = 140;
        var valueW = 90;
        var barLeft = rect.Left + labelW;
        var barRight = rect.Right - valueW;
        var barSpan = barRight - barLeft;

        if (barSpan < 20) return;

        var rowH = Math.Min(44, (rect.Height - 8) / Math.Max(_items.Count, 1));
        var barH = Math.Max(rowH * 0.5f, 8);

        using var fontLabel = new Font("Segoe UI", 8.5f);
        using var fontValue = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        using var brushText = new SolidBrush(Color.FromArgb(60, 60, 60));
        using var brushBg = new SolidBrush(Color.FromArgb(244, 246, 242));

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var y = rect.Top + 4 + i * rowH;
            var barY = y + (rowH - barH) / 2;

            var lt = item.Label.Length > 20 ? item.Label[..20] + "…" : item.Label;
            var lh = fontLabel.GetHeight(g);
            g.DrawString(lt, fontLabel, brushText, rect.Left + 6, y + (rowH - lh) / 2);

            g.FillRectangle(brushBg, barLeft, barY, barSpan, barH);

            var bw = (float)((double)item.Value / (double)maxVal * barSpan);
            if (bw > 0)
            {
                using var bb = new SolidBrush(item.Color);
                var br = new RectangleF(barLeft, barY, bw, barH);
                if (bw > 8)
                {
                    using var p = RoundedRight(br, 4);
                    g.FillPath(bb, p);
                }
                else
                {
                    g.FillRectangle(bb, br);
                }
            }

            var vt = item.Value switch
            {
                >= 1_000_000 => $"$ {item.Value / 1_000_000:N1}M",
                >= 1_000 => $"$ {item.Value / 1_000:N0}K",
                _ => $"$ {item.Value:N0}"
            };
            g.DrawString(vt, fontValue, brushText, barRight + 8, y + (rowH - fontValue.GetHeight(g)) / 2);
        }
    }

    private static GraphicsPath RoundedRight(RectangleF r, float rad)
    {
        var p = new GraphicsPath();
        p.AddLine(r.X, r.Bottom, r.X, r.Y);
        p.AddLine(r.X, r.Y, r.Right - rad, r.Y);
        p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
        p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
        p.CloseFigure();
        return p;
    }
}

internal sealed class HBarItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public Color Color { get; set; } = Color.FromArgb(45, 111, 26);
}
