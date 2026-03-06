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
        ApplicationConfiguration.Initialize();
        var form = new Form1();
        Application.Run(form);
    }
}