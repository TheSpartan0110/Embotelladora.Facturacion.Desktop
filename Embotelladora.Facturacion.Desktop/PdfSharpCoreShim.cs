using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

// Minimal shim for PdfSharpCore types used by the project so it can compile when the real
// PdfSharpCore NuGet package is not available. These implementations are no-ops and only
// exist to satisfy the compiler. Replace with the real package for full PDF functionality.
namespace PdfSharpCore
{
    public enum PageSize { A4 }

    public static class PageSizeConverter
    {
        public static Size ToSize(PageSize ps) => new Size(595, 842); // A4 approx in points
    }

    public class PdfDocument : IDisposable
    {
        private readonly List<PdfPage> _pages = new List<PdfPage>();
        public IReadOnlyList<PdfPage> Pages => _pages;
        public PdfPage AddPage()
        {
            var p = new PdfPage();
            _pages.Add(p);
            return p;
        }

        public void Save(Stream stream)
        {
            // No-op shim
        }

        public void Dispose() { }
    }

    public class PdfPage
    {
        public Drawing.XUnit Width { get; set; } = Drawing.XUnit.FromPoint(595);
        public Drawing.XUnit Height { get; set; } = Drawing.XUnit.FromPoint(842);
    }
}

namespace PdfSharpCore.Drawing
{
    public class XUnit
    {
        public double Point { get; set; }
        public static XUnit FromPoint(double p) => new XUnit { Point = p };
        public override string ToString() => Point.ToString();
    }

    public class XRect
    {
        public double X, Y, Width, Height;
        public XRect(double x, double y, double w, double h) { X = x; Y = y; Width = w; Height = h; }
    }

    public class XGraphics : IDisposable
    {
        public static XGraphics FromPdfPage(PdfSharpCore.PdfPage page) => new XGraphics();
        public void DrawString(string s, XFont font, XBrush brush, XRect rect, XStringFormat fmt) { }
        public void DrawRectangle(XPen pen, double x, double y, double w, double h) { }
        public void DrawLine(XPen pen, double x1, double y1, double x2, double y2) { }
        public void Dispose() { }

        // Overload without format
        public void DrawString(string s, XFont font, XBrush brush, XRect rect)
        {
            // no-op
        }

        // Overload with coordinates
        public void DrawString(string s, XFont font, XBrush brush, double x, double y)
        {
            // no-op
        }
    }

    public class XFont
    {
        public XFont(string family, double size, XFontStyle style = XFontStyle.Regular) { }
    }

    public enum XFontStyle { Regular, Bold, Italic, BoldItalic }

    public class XPen { public XPen(object color, double width) { } }

    public struct XColor { }

    public static class XColors { public static XColor Black => new XColor(); }

    public class XBrush { }
    public static class XBrushes { public static XBrush Black => new XBrush(); public static XBrush DimGray => new XBrush(); }

    public class XStringFormat { public XStringAlignment Alignment; public XLineAlignment LineAlignment; }
    public enum XStringAlignment { Center, Near, Far }
    public enum XLineAlignment { Center }
}

namespace PdfSharpCore.Fonts
{
    public interface IFontResolver
    {
        FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic);
        byte[]? GetFont(string faceName);
        string DefaultFontName { get; }
    }

    public sealed class FontResolverInfo
    {
        public string Keyword { get; }
        public FontResolverInfo(string keyword) { Keyword = keyword; }
    }

    public static class GlobalFontSettings
    {
        public static IFontResolver? FontResolver { get; set; }
    }
}

namespace PdfSharpCore.Pdf
{
    // Provide PdfSharpCore.Pdf.PdfDocument and PdfPage types that delegate to the shim above.
    public class PdfDocument : global::PdfSharpCore.PdfDocument { }
    public class PdfPage : global::PdfSharpCore.PdfPage { }
}
