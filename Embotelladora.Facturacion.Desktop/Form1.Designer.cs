using System.Windows.Forms;

#nullable enable

namespace Embotelladora.Facturacion.Desktop;

partial class Form1 : Form
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // Form1
        // 
        ClientSize = new Size(509, 390);
        Name = "Form1";
        ResumeLayout(false);
    }
}
