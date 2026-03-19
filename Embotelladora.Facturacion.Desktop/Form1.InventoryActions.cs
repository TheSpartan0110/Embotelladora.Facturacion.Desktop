using Embotelladora.Facturacion.Desktop.Features.Facturas;

namespace Embotelladora.Facturacion.Desktop;

public partial class Form1
{
    private void OnProductosInventarioCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (_gridProductosInventario.Columns[e.ColumnIndex].Name != "AccionEliminarProducto")
        {
            return;
        }

        if (_gridProductosInventario.Rows[e.RowIndex].DataBoundItem is not ProductoInventarioDto producto)
        {
            return;
        }

        var confirmacion = MessageBox.Show(
            $"¿Está seguro de que desea eliminar el producto \"{producto.Nombre}\" (Código: {producto.Codigo})?\n\n" +
            "Si el producto tiene facturas asociadas, será desactivado en lugar de eliminado.",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmacion != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var eliminado = _inventarioRepository.DeleteOrDeactivate(producto.Id);
            var mensaje = eliminado
                ? $"El producto \"{producto.Nombre}\" fue eliminado correctamente."
                : $"El producto \"{producto.Nombre}\" fue desactivado porque tiene facturas asociadas.";

            MessageBox.Show(mensaje, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadInventario();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al eliminar el producto:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OpenInventoryMovementDialog()
    {
        var products = _inventarioRepository.GetProductos();
        if (products.Count == 0)
        {
            MessageBox.Show("No hay productos disponibles para ajustar inventario.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var options = new List<InventoryProductOption>();
        foreach (var product in products)
        {
            options.Add(new InventoryProductOption
            {
                Id = product.Id,
                DisplayName = $"{product.Codigo} - {product.Nombre}",
                StockActual = product.StockActual
            });
        }

        using var dialog = new Form
        {
            Text = "Movimiento de Inventario",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(460, 320),
            BackColor = Color.White
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(18)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var cmbProducto = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbProducto.DataSource = options;
        cmbProducto.DisplayMember = nameof(InventoryProductOption.DisplayName);
        cmbProducto.ValueMember = nameof(InventoryProductOption.Id);

        var cmbTipo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbTipo.Items.AddRange(["ENTRADA", "SALIDA"]);
        cmbTipo.SelectedIndex = 0;

        var numCantidad = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 0,
            Minimum = 1,
            Maximum = 999999,
            ThousandsSeparator = true,
            Value = 1
        };

        var txtNota = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Nota del movimiento (opcional)" };

        layout.Controls.Add(new Label { Text = "Producto", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 0);
        layout.Controls.Add(cmbProducto, 0, 1);
        layout.Controls.Add(new Label { Text = "Tipo", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 2);
        layout.Controls.Add(cmbTipo, 0, 3);
        layout.Controls.Add(new Label { Text = "Cantidad", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 4);
        layout.Controls.Add(numCantidad, 0, 5);
        layout.Controls.Add(new Label { Text = "Nota", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 6);
        layout.Controls.Add(txtNota, 0, 7);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 0, 18, 12)
        };

        var btnGuardar = new Button
        {
            Text = "Guardar",
            Width = 110,
            Height = 34,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGuardar.FlatAppearance.BorderSize = 0;

        var btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 110,
            Height = 34,
            BackColor = Color.FromArgb(245, 247, 245),
            FlatStyle = FlatStyle.Flat
        };
        btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
        btnCancelar.Click += (_, _) => dialog.Close();

        btnGuardar.Click += (_, _) =>
        {
            if (cmbProducto.SelectedItem is not InventoryProductOption selectedProduct)
            {
                MessageBox.Show("Selecciona un producto.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tipo = cmbTipo.SelectedItem?.ToString() ?? string.Empty;
            var cantidad = numCantidad.Value;

            if (tipo == "SALIDA" && cantidad > selectedProduct.StockActual)
            {
                MessageBox.Show("La cantidad a retirar supera el stock disponible.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _inventarioRepository.AjustarInventario(selectedProduct.Id, cantidad, tipo, txtNota.Text);
                MessageBox.Show("Movimiento registrado correctamente.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        buttonPanel.Controls.Add(btnGuardar);
        buttonPanel.Controls.Add(btnCancelar);

        dialog.Controls.Add(layout);
        dialog.Controls.Add(buttonPanel);

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadInventario();
        }
    }

    private void OpenNewProductDialog()
    {
        using var dialog = new Form
        {
            Text = "Nuevo Producto",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(520, 420),
            BackColor = Color.White
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));

        for (var i = 0; i < 5; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var txtCodigo = new TextBox { Dock = DockStyle.Fill };
        var txtNombre = new TextBox { Dock = DockStyle.Fill };
        var cmbUnidad = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbUnidad.Items.AddRange(["Und", "Botella", "Litro", "Galón", "Caja"]);
        cmbUnidad.SelectedIndex = 0;

        var numPrecio = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 999999999,
            ThousandsSeparator = true
        };

        var numStockInicial = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 999999,
            ThousandsSeparator = true
        };

        var numStockMinimo = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            DecimalPlaces = 0,
            Minimum = 0,
            Maximum = 999999,
            ThousandsSeparator = true
        };

        layout.Controls.Add(new Label { Text = "Código *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 0);
        layout.Controls.Add(txtCodigo, 1, 0);
        layout.Controls.Add(new Label { Text = "Nombre *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 1);
        layout.Controls.Add(txtNombre, 1, 1);
        layout.Controls.Add(new Label { Text = "Unidad *", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 2);
        layout.Controls.Add(cmbUnidad, 1, 2);
        layout.Controls.Add(new Label { Text = "Precio Base", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 3);
        layout.Controls.Add(numPrecio, 1, 3);

        var stockRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        stockRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        stockRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        stockRow.Controls.Add(numStockInicial, 0, 0);
        stockRow.Controls.Add(numStockMinimo, 1, 0);

        var stockLabelRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        stockLabelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        stockLabelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        stockLabelRow.Controls.Add(new Label { Text = "Stock Inicial", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold) }, 0, 0);
        stockLabelRow.Controls.Add(new Label { Text = "Stock Mínimo", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold) }, 1, 0);

        var stockContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        stockContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        stockContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        stockContainer.Controls.Add(stockLabelRow, 0, 0);
        stockContainer.Controls.Add(stockRow, 0, 1);

        layout.Controls.Add(new Label { Text = "Inventario", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold) }, 0, 4);
        layout.Controls.Add(stockContainer, 1, 4);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 0, 18, 12)
        };

        var btnGuardar = new Button
        {
            Text = "Guardar",
            Width = 110,
            Height = 34,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGuardar.FlatAppearance.BorderSize = 0;

        var btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 110,
            Height = 34,
            BackColor = Color.FromArgb(245, 247, 245),
            FlatStyle = FlatStyle.Flat
        };
        btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
        btnCancelar.Click += (_, _) => dialog.Close();

        btnGuardar.Click += (_, _) =>
        {
            var request = new ProductoCreateRequest
            {
                Codigo = txtCodigo.Text,
                Nombre = txtNombre.Text,
                Unidad = cmbUnidad.SelectedItem?.ToString() ?? string.Empty,
                PrecioBase = numPrecio.Value,
                StockActual = numStockInicial.Value,
                StockMinimo = numStockMinimo.Value
            };

            try
            {
                _inventarioRepository.CrearProducto(request);
                MessageBox.Show("Producto creado correctamente.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        buttonPanel.Controls.Add(btnGuardar);
        buttonPanel.Controls.Add(btnCancelar);

        dialog.Controls.Add(layout);
        dialog.Controls.Add(buttonPanel);

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadInventario();
        }
    }

    private sealed class InventoryProductOption
    {
        public long Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
    }
}
