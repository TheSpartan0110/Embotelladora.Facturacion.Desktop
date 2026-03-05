namespace Embotelladora.Facturacion.Desktop.Features.Clientes;

internal sealed class CustomerEditorDialog : Form
{
    private readonly TextBox _txtCodigo = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtNombre = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtNit = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtDireccion = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtTelefono = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtEmail = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _cmbTipoIva = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _chkActivo = new() { Dock = DockStyle.Left, Text = "Cliente activo", AutoSize = true };

    public CustomerDto? Result { get; private set; }

    public CustomerEditorDialog(CustomerDto model)
    {
        Text = model.Id > 0 ? "Editar cliente" : "Nuevo cliente";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 520;
        Height = 440;

        var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14) };
        Controls.Add(root);

        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 8,
            Height = 290
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 8; i++)
        {
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        }

        _cmbTipoIva.Items.AddRange(["GRAVADO", "EXENTO"]);

        AddRow(form, 0, "Código", _txtCodigo);
        AddRow(form, 1, "Nombre", _txtNombre);
        AddRow(form, 2, "NIT", _txtNit);
        AddRow(form, 3, "Dirección", _txtDireccion);
        AddRow(form, 4, "Teléfono", _txtTelefono);
        AddRow(form, 5, "Email", _txtEmail);
        AddRow(form, 6, "Tipo IVA", _cmbTipoIva);
        AddRow(form, 7, "Activo", _chkActivo);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            FlowDirection = FlowDirection.RightToLeft
        };

        var btnGuardar = new Button
        {
            Text = "Guardar",
            Width = 110,
            Height = 32,
            BackColor = Color.FromArgb(33, 99, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += (_, _) => Save(model.Id);

        var btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 110,
            Height = 32
        };
        btnCancelar.Click += (_, _) => DialogResult = DialogResult.Cancel;

        buttons.Controls.Add(btnGuardar);
        buttons.Controls.Add(btnCancelar);

        root.Controls.Add(buttons);
        root.Controls.Add(form);

        Bind(model);
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        var lbl = new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        table.Controls.Add(lbl, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void Bind(CustomerDto customer)
    {
        _txtCodigo.Text = customer.Codigo;
        _txtNombre.Text = customer.Nombre;
        _txtNit.Text = customer.Nit;
        _txtDireccion.Text = customer.Direccion;
        _txtTelefono.Text = customer.Telefono;
        _txtEmail.Text = customer.Email;
        _cmbTipoIva.SelectedItem = customer.TipoIva;
        if (_cmbTipoIva.SelectedIndex < 0)
        {
            _cmbTipoIva.SelectedItem = "GRAVADO";
        }
        _chkActivo.Checked = customer.Activo;
    }

    private void Save(long id)
    {
        if (string.IsNullOrWhiteSpace(_txtNombre.Text) || string.IsNullOrWhiteSpace(_txtNit.Text))
        {
            MessageBox.Show("Nombre y NIT son obligatorios.", "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new CustomerDto
        {
            Id = id,
            Codigo = _txtCodigo.Text.Trim(),
            Nombre = _txtNombre.Text.Trim(),
            Nit = _txtNit.Text.Trim(),
            Direccion = _txtDireccion.Text.Trim(),
            Telefono = _txtTelefono.Text.Trim(),
            Email = _txtEmail.Text.Trim(),
            TipoIva = _cmbTipoIva.SelectedItem?.ToString() ?? "GRAVADO",
            Activo = _chkActivo.Checked
        };

        DialogResult = DialogResult.OK;
    }
}
