using System.Globalization;
using System.Windows.Forms;

namespace Embotelladora.Facturacion.Desktop;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var culture = new CultureInfo("es-CO");
        culture.NumberFormat.CurrencySymbol = "$";
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        ApplicationConfiguration.Initialize();
        var form = new Form1();
        Application.Run(form);
    }
}