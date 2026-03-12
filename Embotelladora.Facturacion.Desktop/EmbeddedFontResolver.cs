using System;
using System.Collections.Generic;
using PdfSharpCore.Fonts;
using System.IO;

// Minimal embedded font resolver for PdfSharpCore.
// It tries to load fonts from a "fonts" folder in application directory.
internal class EmbeddedFontResolver : IFontResolver
{
    private readonly string _basePath;
    public EmbeddedFontResolver()
    {
        _basePath = AppDomain.CurrentDomain.BaseDirectory;
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var key = familyName.ToLowerInvariant();
        if (key.Contains("consolas"))
        {
            return new FontResolverInfo("Consolas#" + (isBold ? "B" : "R"));
        }

        // Default to Times New Roman
        return new FontResolverInfo("TimesNewRoman#" + (isBold ? "B" : "R"));
    }

    public byte[]? GetFont(string faceName)
    {
        try
        {
            var parts = faceName.Split('#');
            var name = parts[0];
            var style = parts.Length > 1 ? parts[1] : "R";

            // Look for fonts folder
            var fontsDir = Path.Combine(_basePath, "fonts");
            if (!Directory.Exists(fontsDir)) return null;

            string? fileName = name switch
            {
                "Consolas" when style == "R" => "consola.ttf",
                "Consolas" when style == "B" => "consolab.ttf",
                "TimesNewRoman" when style == "R" => "times.ttf",
                "TimesNewRoman" when style == "B" => "timesbd.ttf",
                _ => null
            };

            if (fileName is null) return null;

            var path = Path.Combine(fontsDir, fileName);
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);
            if (bytes == null || bytes.Length == 0) return null;
            return bytes;
        }
        catch
        {
            return null;
        }
    }

    public string DefaultFontName => "TimesNewRoman";
}
