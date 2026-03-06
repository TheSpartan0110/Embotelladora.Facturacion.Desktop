using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using Embotelladora.Facturacion.Desktop.Data;
using Embotelladora.Facturacion.Desktop.Features.Clientes;
using Embotelladora.Facturacion.Desktop.Features.Dashboard;
using Embotelladora.Facturacion.Desktop.Features.Facturas;
using Embotelladora.Facturacion.Desktop.UI;

namespace Embotelladora.Facturacion.Desktop;

public partial class Form1 : Form
{
    private readonly DashboardService _dashboardService = new();
    private readonly CustomerRepository _customerRepository = new();
    private readonly InvoiceRepository _invoiceRepository = new();
    private readonly PaymentRepository _paymentRepository = new();
    private readonly InventoryRepository _inventoryRepository = new();
    
    private readonly Dictionary<string, Button> _menuButtons = [];
    private readonly BindingList<InvoiceItemDraft> _invoiceItems = [];
    private FlowLayoutPanel _sidebarMenu = null!;

    private Label _headerTitle = null!;
    private Panel _contentHost = null!;
    private Button _btnHeaderAction = null!;
    private Action? _headerAction;

    private Label _lblTotalFacturado = null!;
    private Label _lblSaldoPendiente = null!;
    private Label _lblTotalPagos = null!;
    private Label _lblClientesActivos = null!;
    private DataGridView _gridRecentInvoices = null!;
    private DataGridView _gridLowStock = null!;

    private DataGridView _gridCustomers = null!;
    private TextBox _txtSearchCustomer = null!;

    private DataGridView _gridInvoices = null!;
    private DataGridView _gridInvoiceItems = null!;
    private DateTimePicker _dtpInvoiceDate = null!;
    private ComboBox _cmbInvoiceCustomer = null!;
    private ComboBox _cmbInvoicePaymentMethod = null!;
    private TextBox _txtInvoiceNotes = null!;
    private ComboBox _cmbItemProduct = null!;
    private TextBox _txtItemDescription = null!;
    private NumericUpDown _numItemQuantity = null!;
    private NumericUpDown _numItemPrice = null!;
    private CheckBox _chkItemIva = null!;
    private Label _lblInvoiceSubtotal = null!;
    private Label _lblInvoiceIva = null!;
    private Label _lblInvoiceTotal = null!;
    private Label _lblItemStock = null!;

    private DataGridView _gridPayments = null!;
    private ComboBox _cmbPaymentInvoice = null!;
    private DateTimePicker _dtpPaymentDate = null!;
    private NumericUpDown _numPaymentAmount = null!;
    private ComboBox _cmbPaymentMethod = null!;
    private TextBox _txtPaymentReference = null!;
    private TextBox _txtPaymentNotes = null!;
    private Label _lblPaymentTotal = null!;

    private decimal _currentIvaRate = 0.19m;
    private string _currentModule = "Dashboard";

    public Form1()
    {
        InitializeComponent();
        DatabaseInitializer.Initialize();
        BuildLayout();
        LoadInitialData();
        ShowModule("Dashboard");
    }

    private void BuildLayout()
    {
        Text = "AceitesPro Facturación";
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(243, 245, 242);
        MinimumSize = new Size(1200, 720);

        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var sidebarPanel = new Panel { Dock = DockStyle.Fill };
        var contentPanel = new Panel { Dock = DockStyle.Fill };

        rootLayout.Controls.Add(sidebarPanel, 0, 0);
        rootLayout.Controls.Add(contentPanel, 1, 0);

        Controls.Add(rootLayout);
        BuildSidebar(sidebarPanel);
        BuildMainContent(contentPanel);
    }

    private void BuildSidebar(Control parent)
    {
        parent.BackColor = Color.FromArgb(18, 57, 26);

        var sidebarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 98));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        parent.Controls.Add(sidebarLayout);

        var logoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            BackColor = Color.FromArgb(13, 48, 22)
        };

        var logo = new Label
        {
            Text = "AceitesPro\nFACTURACIÓN",
            ForeColor = Color.FromArgb(255, 219, 126),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        logoPanel.Controls.Add(logo);

        _sidebarMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10),
            AutoScroll = true
        };

        sidebarLayout.Controls.Add(logoPanel, 0, 0);
        sidebarLayout.Controls.Add(_sidebarMenu, 0, 1);

        AddMenuSection("PRINCIPAL");
        AddMenuButton(_sidebarMenu, "Dashboard", () => ShowModule("Dashboard"));
        AddMenuButton(_sidebarMenu, "Clientes", () => ShowModule("Clientes"));
        AddMenuButton(_sidebarMenu, "Facturas", () => ShowModule("Facturas"));
        AddMenuButton(_sidebarMenu, "Pagos", () => ShowModule("Pagos"));
        AddMenuButton(_sidebarMenu, "Inventario", () => ShowModule("Inventario"));

        parent.SizeChanged += (_, _) => ResizeSidebarItems(parent.Width);
        ResizeSidebarItems(parent.Width);
    }

    private void AddMenuSection(string title, int topMargin = 8)
    {
        var section = new Label
        {
            Text = title,
            ForeColor = Color.FromArgb(123, 160, 129),
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Height = 22,
            Width = 210,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(5, topMargin, 5, 2)
        };

        _sidebarMenu.Controls.Add(section);
    }

    private void ResizeSidebarItems(int containerWidth)
    {
        var width = Math.Max(170, containerWidth - 28);
        foreach (Control control in _sidebarMenu.Controls)
        {
            control.Width = width;
        }
    }

    private void AddMenuButton(Control parent, string text, Action onClick)
    {
        var button = new Button
        {
            Text = text,
            Width = 200,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(18, 57, 26),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            Margin = new Padding(5)
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => onClick();
        _menuButtons[text] = button;
        parent.Controls.Add(button);
    }

    private void BuildMainContent(Control parent)
    {
        var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        parent.Controls.Add(root);

        var headerPanel = new Panel { Dock = DockStyle.Top, Height = 68 };

        _btnHeaderAction = new Button
        {
            Text = "+ Nueva Factura",
            Width = 150,
            Height = 38,
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Visible = false
        };
        _btnHeaderAction.FlatAppearance.BorderSize = 0;
        _btnHeaderAction.Click += (_, _) => _headerAction?.Invoke();

        _headerTitle = new Label
        {
            Text = "Dashboard",
            AutoSize = false,
            Height = 36,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 15, FontStyle.Bold)
        };

        headerPanel.Controls.Add(_headerTitle);
        headerPanel.Controls.Add(_btnHeaderAction);
        root.Controls.Add(headerPanel);

        _contentHost = new Panel { Dock = DockStyle.Fill };
        root.Controls.Add(_contentHost);
    }

    private void ShowModule(string module)
    {
        _currentModule = module;
        _contentHost.Controls.Clear();

        var view = module switch
        {
            "Dashboard" => BuildDashboardView(),
            "Clientes" => BuildCustomersView(),
            "Facturas" => BuildInvoicesListView(),
            "Pagos" => BuildPaymentsListView(),
            "Inventario" => BuildInventoryView(),
            _ => new Panel { Dock = DockStyle.Fill }
        };

        _contentHost.Controls.Add(view);
        _btnHeaderAction.Visible = module == "Facturas";
        _headerTitle.Text = module;
    }

    private Panel BuildDashboardView()
    {
        var view = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 140,
            ColumnCount = 4,
            Padding = new Padding(0, 6, 0, 0)
        };
        cards.ColumnStyles.AddRange(new[] {
            new ColumnStyle(SizeType.Percent, 25),
            new ColumnStyle(SizeType.Percent, 25),
            new ColumnStyle(SizeType.Percent, 25),
            new ColumnStyle(SizeType.Percent, 25)
        });

        _lblTotalFacturado = CreateCard(cards, 0, "Total Facturado");
        _lblSaldoPendiente = CreateCard(cards, 1, "Saldo Pendiente");
        _lblTotalPagos = CreateCard(cards, 2, "Total Pagos");
        _lblClientesActivos = CreateCard(cards, 3, "Clientes Activos");
        view.Controls.Add(cards);

        var analytics = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 330,
            ColumnCount = 2,
            Padding = new Padding(4, 8, 4, 8)
        };
        analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

        var recentCard = CreateDataPanel("Facturas Recientes", out _gridRecentInvoices);
        analytics.Controls.Add(recentCard, 0, 0);

        var stockCard = CreateDataPanel("Alertas de Stock", out _gridLowStock);
        analytics.Controls.Add(stockCard, 1, 0);
        view.Controls.Add(analytics);

        LoadDashboardData();
        return view;
    }

    private Panel BuildCustomersView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 8, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var searchPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };

        _txtSearchCustomer = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar cliente...",
            Font = new Font("Segoe UI", 10)
        };
        _txtSearchCustomer.TextChanged += (_, _) => LoadCustomers(_txtSearchCustomer.Text);
        searchPanel.Controls.Add(_txtSearchCustomer);

        var tableCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(12)
        };

        _gridCustomers = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false
        };
        ConfigureGridStyle(_gridCustomers);
        tableCard.Controls.Add(_gridCustomers);

        layout.Controls.Add(searchPanel, 0, 0);
        layout.Controls.Add(tableCard, 0, 1);
        view.Controls.Add(layout);

        LoadCustomers();
        return view;
    }

    private Panel BuildInvoicesListView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 8, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filterPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };

        var btnNewInvoice = new Button
        {
            Text = "+ Nueva Factura",
            Width = 120,
            Height = 38,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Right
        };
        btnNewInvoice.Click += (_, _) => ShowModule("Nueva Factura");
        filterPanel.Controls.Add(btnNewInvoice);

        var tableCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(12)
        };

        _gridInvoices = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false
        };
        ConfigureGridStyle(_gridInvoices);
        tableCard.Controls.Add(_gridInvoices);

        layout.Controls.Add(filterPanel, 0, 0);
        layout.Controls.Add(tableCard, 0, 1);
        view.Controls.Add(layout);

        LoadInvoicesList();
        return view;
    }

    private Panel BuildPaymentsListView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 8, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        var titleLabel = new Label
        {
            Text = "Historial de Pagos",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        titlePanel.Controls.Add(titleLabel);

        var tableCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(12)
        };

        _gridPayments = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false
        };
        ConfigureGridStyle(_gridPayments);
        tableCard.Controls.Add(_gridPayments);

        layout.Controls.Add(titlePanel, 0, 0);
        layout.Controls.Add(tableCard, 0, 1);
        view.Controls.Add(layout);

        LoadPayments();
        return view;
    }

    private Panel BuildInventoryView()
    {
        var view = new Panel { Dock = DockStyle.Fill };
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false
        };
        ConfigureGridStyle(grid);
        view.Controls.Add(grid);
        LoadInventory();
        return view;
    }

    private void LoadInitialData()
    {
        LoadCustomers();
        LoadPaymentMethods();
        LoadProducts();
    }

    private void LoadDashboardData()
    {
        try
        {
            var data = _invoiceRepository.GetGridRows();
            if (_gridRecentInvoices?.Parent?.Parent is not null)
            {
                _gridRecentInvoices.DataSource = data.Take(10).ToList();
                FormatInvoicesGrid(_gridRecentInvoices);
            }

            _lblTotalFacturado.Text = $"$ {data.Sum(x => x.Total):N0}";
            _lblSaldoPendiente.Text = $"$ {data.Sum(x => x.Total):N0}";
            _lblClientesActivos.Text = _customerRepository.GetGridRows().Count.ToString();
            _lblTotalPagos.Text = "$ 0";
        }
        catch { }
    }

    private void LoadCustomers(string? search = null)
    {
        var data = _customerRepository.GetGridRows(search);
        _gridCustomers.DataSource = data;
        
        var idCol = _gridCustomers.Columns["Id"];
        if (idCol != null) idCol.Visible = false;

        if (_gridCustomers.Columns["Cliente"] is not null)
            _gridCustomers.Columns["Cliente"].HeaderText = "Cliente";
        if (_gridCustomers.Columns["NitCedula"] is not null)
            _gridCustomers.Columns["NitCedula"].HeaderText = "NIT / Cédula";
    }

    private void LoadInvoicesList()
    {
        var data = _invoiceRepository.GetGridRows();
        _gridInvoices.DataSource = data;
        FormatInvoicesGrid(_gridInvoices);
    }

    private void LoadPayments()
    {
        var data = _paymentRepository.GetPaymentHistory();
        _gridPayments.DataSource = data;
    }

    private void LoadInventory()
    {
        var data = _inventoryRepository.GetProducts();
        if (_contentHost.Controls[0].Controls[0] is DataGridView grid)
        {
            grid.DataSource = data;
        }
    }

    private void LoadProducts()
    {
        var products = _invoiceRepository.GetProducts();
        _cmbItemProduct?.Items.Clear();
        foreach (var product in products)
        {
            _cmbItemProduct?.Items.Add(new { product.Id, product.Nombre });
        }
    }

    private void LoadPaymentMethods()
    {
        var methods = _invoiceRepository.GetPaymentMethods();
        _cmbInvoicePaymentMethod?.Items.Clear();
        _cmbPaymentMethod?.Items.Clear();
        
        foreach (var method in methods)
        {
            _cmbInvoicePaymentMethod?.Items.Add(new { method.Id, method.Nombre });
            _cmbPaymentMethod?.Items.Add(new { method.Id, method.Nombre });
        }
    }

    private void FormatInvoicesGrid(DataGridView grid)
    {
        if (grid.Columns["Id"] is not null) grid.Columns["Id"].Visible = false;
        if (grid.Columns["Numero"] is not null) grid.Columns["Numero"].HeaderText = "Factura #";
        if (grid.Columns["Cliente"] is not null) grid.Columns["Cliente"].HeaderText = "Cliente";
        if (grid.Columns["Total"] is not null) 
        {
            grid.Columns["Total"].HeaderText = "Total";
            grid.Columns["Total"].DefaultCellStyle.Format = "C2";
        }
    }

    private void ConfigureGridStyle(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles = false;
        var headerStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(239, 239, 242),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            Padding = new Padding(8, 4, 8, 4)
        };
        grid.ColumnHeadersDefaultCellStyle = headerStyle;

        var rowStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Color.FromArgb(33, 33, 33),
            Padding = new Padding(8, 4, 8, 4)
        };
        grid.DefaultCellStyle = rowStyle;
    }

    private Panel CreateDataPanel(string title, out DataGridView grid)
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 52, 22),
            Height = 28
        };
        panel.Controls.Add(titleLabel);

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false
        };
        ConfigureGridStyle(grid);
        panel.Controls.Add(grid);

        return panel;
    }

    private Label CreateCard(TableLayoutPanel parent, int column, string title)
    {
        var card = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(12),
            Margin = new Padding(4),
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BorderColor = Color.FromArgb(222, 226, 219)
        };
        parent.Controls.Add(card, column, 0);

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(123, 160, 129),
            Dock = DockStyle.Top,
            Height = 24
        };
        card.Controls.Add(titleLabel);

        var valueLabel = new Label
        {
            Text = "$ 0",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 52, 22),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        card.Controls.Add(valueLabel);

        return valueLabel;
    }

    private string FormatNit(string nit)
    {
        if (string.IsNullOrWhiteSpace(nit))
            return nit;

        var soloNumeros = new string(nit.Where(char.IsDigit).ToArray());

        if (soloNumeros.Length > 3)
            return $"{soloNumeros.Insert(soloNumeros.Length - 3, "-").Insert(soloNumeros.Length - 7, "-")}";

        return soloNumeros;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        LoadInitialData();
    }
}
