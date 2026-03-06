using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Embotelladora.Facturacion.Desktop.UI;

/// <summary>
/// Panel with rounded corners
/// </summary>
internal sealed class RoundedPanel : Panel
{
    private int _radius = 10;

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            RecreateHandle();
        }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.Gray;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var path = CreateRoundedRectanglePath(ClientRectangle, _radius);
        e.Graphics.FillPath(new SolidBrush(BackColor), path);

        if (BorderStyle != BorderStyle.None)
        {
            using var pen = new Pen(BorderColor, 2);
            e.Graphics.DrawPath(pen, path);
        }

        base.OnPaint(e);
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
