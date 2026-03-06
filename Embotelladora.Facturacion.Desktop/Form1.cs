using System.ComponentModel;
using System.Data;
using System.Drawing.Drawing2D;
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
    private readonly CarteraRepository _carteraRepository = new();
    private readonly BalanceRepository _balanceRepository = new();
    private readonly InventarioRepository _inventarioRepository = new();
    private readonly Dictionary<string, Button> _menuButtons = [];
    private readonly BindingList<InvoiceItemDraft> _invoiceItems = [];
    private FlowLayoutPanel _sidebarMenu = null!;

    private Label _headerTitle = null!;
    private Label _headerSubtitle = null!;
    private Panel _contentHost = null!;
    private Panel _dashboardView = null!;
    private Panel _customersView = null!;
    private Panel _invoicesView = null!;
    private Panel _newInvoiceView = null!;
    private Panel _paymentsView = null!;
    private Panel _newPaymentView = null!;
    private Panel _carteraView = null!;
    private Panel _balanceView = null!;
    private Panel _inventarioView = null!;
    private Button _btnHeaderAction = null!;
    private Action? _headerAction;
    private System.Windows.Forms.Timer? _moduleAnimationTimer;

    private Label _lblTotalFacturado = null!;
    private Label _lblSaldoPendiente = null!;
    private Label _lblTotalPagos = null!;
    private Label _lblClientesActivos = null!;
    private Label _lblPendingCount = null!;
    private Label _lblPaymentsCount = null!;
    private DataGridView _gridRecentInvoices = null!;
    private DataGridView _gridLowStock = null!;

    private DataGridView _gridCustomers = null!;
    private TextBox _txtSearchCustomer = null!;
    private DataGridView _gridInvoiceItems = null!;
    private DataGridView _gridInvoices = null!;
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
    private decimal _currentIvaRate = 0.19m;

    // Payment module fields
    private DataGridView _gridPayments = null!;
    private ComboBox _cmbPaymentInvoice = null!;
    private DateTimePicker _dtpPaymentDate = null!;
    private NumericUpDown _numPaymentAmount = null!;
    private ComboBox _cmbPaymentMethod = null!;
    private TextBox _txtPaymentReference = null!;
    private TextBox _txtPaymentNotes = null!;
    private Label _lblPaymentBalance = null!;
    private Label _lblPaymentTotal = null!;

    // Cartera module fields
    private Label _lblCarteraClientes = null!;
    private Label _lblCarteraFacturas = null!;
    private Label _lblCarteraTotalCobrar = null!;
    private Label _lblCarteraVencido = null!;
    private DataGridView _gridFacturasPendientes = null!;
    private DataGridView _gridClientesSaldo = null!;
    private DataGridView _gridEdadSaldos = null!;

    // Balance module fields
    private Label _lblBalanceFacturado = null!;
    private Label _lblBalanceRecaudado = null!;
    private Label _lblBalanceCuentasPorCobrar = null!;
    private Label _lblBalanceNeto = null!;
    private DataGridView _gridBalanceMensual = null!;
    private DataGridView _gridTopClientes = null!;

    // Inventario module fields
    private Label _lblInventarioTotalProductos = null!;
    private Label _lblInventarioValorTotal = null!;
    private Label _lblInventarioStockBajo = null!;
    private Label _lblInventarioAgotados = null!;
    private DataGridView _gridProductosInventario = null!;
    private DataGridView _gridMovimientos = null!;
    private DataGridView _gridStockBajo = null!;
    private TextBox _txtSearchInventario = null!;

    public Form1()
    {
        InitializeComponent();
        DatabaseInitializer.Initialize();
        BuildLayout();
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
            Padding = new Padding(16, 16, 12, 12),
            BackColor = Color.FromArgb(13, 48, 22)
        };

        var logo = new Label
        {
            Text = "AceitesPro\nFACTURACIÓN",
            ForeColor = Color.FromArgb(255, 219, 126),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = false,
            AutoEllipsis = false,
            Dock = DockStyle.Fill
        };
        logoPanel.Controls.Add(logo);

        _sidebarMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10, 12, 10, 10),
            AutoScroll = true
        };

        sidebarLayout.Controls.Add(logoPanel, 0, 0);
        sidebarLayout.Controls.Add(_sidebarMenu, 0, 1);

        AddMenuSection("PRINCIPAL");
        AddMenuButton(_sidebarMenu, "Dashboard", () => ShowModule("Dashboard"));
        AddMenuButton(_sidebarMenu, "Clientes", () => ShowModule("Clientes"));
        AddMenuButton(_sidebarMenu, "Facturas", () => ShowModule("Facturas"));
        AddMenuButton(_sidebarMenu, "Pagos", () => ShowModule("Pagos"));
        AddMenuButton(_sidebarMenu, "Cartera", () => ShowModule("Cartera"));
        AddMenuButton(_sidebarMenu, "Balance", () => ShowModule("Balance"));
        AddMenuButton(_sidebarMenu, "Inventario", () => ShowModule("Inventario"));

        AddMenuSection("SISTEMA", 18);
        AddMenuButton(_sidebarMenu, "Configuración", OpenDatabaseFolder);

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
            if (control is Button button)
            {
                button.Width = width;
            }
            else if (control is Label label)
            {
                label.Width = width;
            }
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
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
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

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68
        };

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
        _btnHeaderAction.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 96, 23);
        _btnHeaderAction.FlatAppearance.MouseDownBackColor = Color.FromArgb(34, 84, 20);
        _btnHeaderAction.Padding = new Padding(6, 0, 6, 0);
        _btnHeaderAction.Click += (_, _) => _headerAction?.Invoke();

        _headerTitle = new Label
        {
            Text = "Dashboard",
            AutoSize = false,
            Height = 36,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 15, FontStyle.Bold)
        };

        _headerSubtitle = new Label
        {
            Text = "Resumen de facturación y cartera",
            AutoSize = false,
            Height = 28,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.DimGray
        };

        headerPanel.Controls.Add(_headerSubtitle);
        headerPanel.Controls.Add(_headerTitle);
        headerPanel.Controls.Add(_btnHeaderAction);
        root.Controls.Add(headerPanel);

        _contentHost = new Panel
        {
            Dock = DockStyle.Fill
        };
        root.Controls.Add(_contentHost);

        _dashboardView = BuildDashboardView();
        _customersView = BuildCustomersView();
        _newInvoiceView = BuildInvoicesView();
        _invoicesView = BuildInvoicesListView();
        _paymentsView = BuildPaymentsListView();
        _newPaymentView = BuildNewPaymentView();
        _carteraView = BuildCarteraView();
        _balanceView = BuildBalanceView();
        _inventarioView = BuildInventarioView();
    }

    private Panel BuildInvoicesListView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 72, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        view.Controls.Add(layout);

        var searchCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14)
        };

        var searchRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));

        var txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar por número o cliente...",
            Font = new Font("Segoe UI", 10)
        };
        txtSearch.TextChanged += (_, _) => LoadInvoicesList();

        var cmbEstado = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        cmbEstado.Items.AddRange(["Todos los estados", "Enviada", "Pagada", "Vencida"]);
        cmbEstado.SelectedIndex = 0;

        searchRow.Controls.Add(txtSearch, 0, 0);
        searchRow.Controls.Add(cmbEstado, 1, 0);
        searchCard.Controls.Add(searchRow);

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
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridInvoices);
        tableCard.Controls.Add(_gridInvoices);

        layout.Controls.Add(searchCard, 0, 0);
        layout.Controls.Add(tableCard, 0, 1);
        return view;
    }

    private Panel BuildDashboardView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 140,
            ColumnCount = 4,
            Padding = new Padding(0, 6, 0, 0)
        };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        _lblTotalFacturado = CreateCard(cards, 0, "Total Facturado", out _);
        _lblSaldoPendiente = CreateCard(cards, 1, "Saldo Pendiente", out _lblPendingCount);
        _lblTotalPagos = CreateCard(cards, 2, "Total Pagos", out _lblPaymentsCount);
        _lblClientesActivos = CreateCard(cards, 3, "Clientes Activos", out _);
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

        var monthlyPanel = CreateDataPanel("Facturación Mensual", out var gridMonthly);
        gridMonthly.Visible = false;
        monthlyPanel.Controls.Add(CreateDashboardBars());

        var statusPanel = CreateDataPanel("Estado de Facturas", out var gridStatus);
        gridStatus.Visible = false;
        statusPanel.Controls.Add(CreateStatusLegend());

        analytics.Controls.Add(monthlyPanel, 0, 0);
        analytics.Controls.Add(statusPanel, 1, 0);
        view.Controls.Add(analytics);

        var content = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BackColor = Color.Transparent,
            IsSplitterFixed = true
        };
        view.Controls.Add(content);

        void AdjustDashboardSplit()
        {
            const int desiredPanel1Min = 220;
            const int desiredPanel2Min = 220;
            const int desiredSplitterDistance = 280;

            var available = content.Height - content.SplitterWidth;
            if (available <= 0)
            {
                return;
            }

            if (available >= desiredPanel1Min + desiredPanel2Min)
            {
                content.Panel1MinSize = desiredPanel1Min;
                content.Panel2MinSize = desiredPanel2Min;
            }
            else
            {
                var panel1Fallback = available / 2;
                content.Panel1MinSize = panel1Fallback;
                content.Panel2MinSize = available - panel1Fallback;
            }

            var min = content.Panel1MinSize;
            var max = Math.Max(min, available - content.Panel2MinSize);
            content.SplitterDistance = Math.Clamp(desiredSplitterDistance, min, max);
        }

        content.SizeChanged += (_, _) => AdjustDashboardSplit();
        view.SizeChanged += (_, _) => AdjustDashboardSplit();

        var recentPanel = CreateDataPanel("Facturas Recientes", out _gridRecentInvoices);
        content.Panel1.Controls.Add(recentPanel);

        var stockPanel = CreateDataPanel("Alertas de Stock Mínimo", out _gridLowStock);
        content.Panel2.Controls.Add(stockPanel);

        AdjustDashboardSplit();

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
            Margin = Padding.Empty,
            Padding = new Padding(0, 72, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        view.Controls.Add(layout);

        var searchCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12, 10, 12, 8),
            Margin = new Padding(0, 0, 0, 8)
        };
        searchCard.Paint += (_, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(222, 226, 219), 1);
            pe.Graphics.DrawLine(pen, 0, searchCard.Height - 1, searchCard.Width, searchCard.Height - 1);
        };

        var searchRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));

        var lblSearchIcon = new Label
        {
            Text = "🔍",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12)
        };

        _txtSearchCustomer = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar por nombre, NIT / Cédula o email...",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = Padding.Empty
        };
        _txtSearchCustomer.TextChanged += (_, _) => LoadCustomers(_txtSearchCustomer.Text);

        var btnClearSearch = new Button
        {
            Text = "✕",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        btnClearSearch.FlatAppearance.BorderSize = 0;
        btnClearSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnClearSearch.Click += (_, _) => { _txtSearchCustomer.Clear(); _txtSearchCustomer.Focus(); };

        var lblSearchHint = new Label
        {
            Text = "  Puede buscar por nombre, NIT / Cédula o email del cliente",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 8, FontStyle.Regular),
            ForeColor = Color.FromArgb(160, 160, 160),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(30, 0, 0, 0)
        };

        searchRow.Controls.Add(lblSearchIcon, 0, 0);
        searchRow.Controls.Add(_txtSearchCustomer, 1, 0);
        searchRow.Controls.Add(btnClearSearch, 2, 0);

        searchCard.Controls.Add(lblSearchHint);
        searchCard.Controls.Add(searchRow);

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
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowTemplate = { Height = 52 }
        };
        _gridCustomers.CellDoubleClick += (_, _) => EditSelectedCustomer();
        _gridCustomers.CellContentClick += OnCustomerGridCellContentClick;
        ConfigureGridStyle(_gridCustomers);
        _gridCustomers.DefaultCellStyle.Padding = new Padding(6, 8, 6, 8);
        tableCard.Controls.Add(_gridCustomers);

        layout.Controls.Add(searchCard, 0, 0);
        layout.Controls.Add(tableCard, 0, 1);
        return view;
    }

    private Panel BuildInvoicesView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 72, 0, 0),
            AutoScroll = true
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        view.Controls.Add(mainLayout);

        // Columna izquierda: 3 filas
        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(12, 12, 6, 12),
            Padding = Padding.Empty
        };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        // 1. INFORMACIÓN GENERAL
        var infoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var infoTitle = new Label
        {
            Text = "📋 Información General",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        infoCard.Controls.Add(infoTitle);

        var infoGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        infoGrid.Height = 140;

        infoGrid.Controls.Add(new Label { Text = "Cliente *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 0);
        _cmbInvoiceCustomer = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        infoGrid.Controls.Add(_cmbInvoiceCustomer, 0, 1);

        infoGrid.Controls.Add(new Label { Text = "Método de Pago *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 1, 0);
        _cmbInvoicePaymentMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        infoGrid.Controls.Add(_cmbInvoicePaymentMethod, 1, 1);

        infoGrid.Controls.Add(new Label { Text = "Fecha *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 2);
        _dtpInvoiceDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        infoGrid.Controls.Add(_dtpInvoiceDate, 0, 3);

        infoGrid.Controls.Add(new Label { Text = "Tasa de IVA", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 1, 2);
        var lblIvaRate = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray };
        lblIvaRate.Text = "IVA 19% (predeterminado)";
        infoGrid.Controls.Add(lblIvaRate, 1, 3);

        infoCard.Controls.Add(infoGrid);
        leftLayout.Controls.Add(infoCard, 0, 0);

        // 2. PRODUCTOS DE LA FACTURA
        var productsCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var productsTitle = new Label
        {
            Text = "📦 Productos de la Factura",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        productsCard.Controls.Add(productsTitle);

        var productsTopBar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 40,
            Margin = new Padding(0, 8, 0, 8),
            Padding = Padding.Empty
        };
        productsTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        productsTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

        var cmbProductLabel = new Label { Text = "Seleccionar producto", Font = new Font("Segoe UI", 9), TextAlign = ContentAlignment.MiddleLeft };
        _cmbItemProduct = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbItemProduct.SelectedIndexChanged += (_, _) => OnProductChanged();
        productsTopBar.Controls.Add(_cmbItemProduct, 0, 0);

        var btnAddItem = new Button { Text = "+ Agregar", Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 111, 26), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnAddItem.Click += (_, _) => AddInvoiceItem();
        productsTopBar.Controls.Add(btnAddItem, 1, 0);
        productsCard.Controls.Add(productsTopBar);

        // Formulario de edición del producto
        var productEditForm = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 2,
            Height = 80,
            Margin = new Padding(0, 8, 0, 8),
            Padding = Padding.Empty
        };
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

        productEditForm.Controls.Add(new Label { Text = "Descripción", Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top }, 0, 0);
        _txtItemDescription = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 0) };
        productEditForm.Controls.Add(_txtItemDescription, 0, 1);

        productEditForm.Controls.Add(new Label { Text = "Cantidad", Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top }, 1, 0);
        _numItemQuantity = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Maximum = 999999, Minimum = 0, Value = 1, Margin = new Padding(0, 4, 8, 0) };
        productEditForm.Controls.Add(_numItemQuantity, 1, 1);

        productEditForm.Controls.Add(new Label { Text = "Precio", Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top }, 2, 0);
        _numItemPrice = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Maximum = 999999999, ThousandsSeparator = true, Margin = new Padding(0, 4, 8, 0) };
        productEditForm.Controls.Add(_numItemPrice, 2, 1);

        productEditForm.Controls.Add(new Label { Text = "Stock", Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top }, 3, 0);
        _lblItemStock = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.DimGray, Text = "Stock: -", Margin = new Padding(0, 4, 0, 0), Font = new Font("Segoe UI", 9) };
        productEditForm.Controls.Add(_lblItemStock, 3, 1);

        productsCard.Controls.Add(productEditForm);

        // Checkbox para IVA
        _chkItemIva = new CheckBox
        {
            Text = "✓ Aplicar IVA",
            Dock = DockStyle.Top,
            Height = 24,
            Checked = true,
            Font = new Font("Segoe UI", 9),
            Margin = new Padding(0, 0, 0, 8)
        };
        productsCard.Controls.Add(_chkItemIva);

        _gridInvoiceItems = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            ReadOnly = false
        };

        // Agregar columna de eliminación ANTES de vincular DataSource
        var deleteColumn = new DataGridViewButtonColumn
        {
            Name = "Eliminar",
            HeaderText = "Acción",
            Text = "❌ Quitar",
            UseColumnTextForButtonValue = true,
            Width = 100,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        _gridInvoiceItems.Columns.Add(deleteColumn);

        // Ahora vincular DataSource
        _gridInvoiceItems.DataSource = _invoiceItems;

        _gridInvoiceItems.CellEndEdit += (_, _) => RecalculateInvoiceTotals();
        _gridInvoiceItems.DataBindingComplete += (_, _) => ConfigureInvoiceItemsColumns();
        _gridInvoiceItems.DefaultCellStyle.Padding = new Padding(2, 4, 2, 4);
        ConfigureGridStyle(_gridInvoiceItems);
        _gridInvoiceItems.CellContentClick += (sender, e) =>
        {
            var eliminarColumn = _gridInvoiceItems.Columns["Eliminar"];
            if (eliminarColumn != null && e.ColumnIndex == eliminarColumn.Index && e.RowIndex >= 0)
            {
                if (_gridInvoiceItems.Rows[e.RowIndex].DataBoundItem is InvoiceItemDraft item)
                {
                    _invoiceItems.Remove(item);
                    RecalculateInvoiceTotals();
                }
            }
        };

        productsCard.Controls.Add(_gridInvoiceItems);

        leftLayout.Controls.Add(productsCard, 0, 1);

        // 3. NOTAS
        var notesCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };

        var notesTitle = new Label
        {
            Text = "📝 Notas",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        notesCard.Controls.Add(notesTitle);

        _txtInvoiceNotes = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            Margin = new Padding(0, 8, 0, 0)
        };
        notesCard.Controls.Add(_txtInvoiceNotes);

        leftLayout.Controls.Add(notesCard, 0, 2);
        mainLayout.Controls.Add(leftLayout, 0, 0);

        // Columna derecha: RESUMEN
        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 12, 12, 12),
            Padding = Padding.Empty,
            AutoScroll = true
        };

        var summaryCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(20)
        };

        var summaryTitle = new Label
        {
            Text = "Resumen",
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Margin = new Padding(0, 0, 0, 16)
        };
        summaryCard.Controls.Add(summaryTitle);

        // Panel con scroll para el contenido del resumen
        var summaryContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 20, 0, 0)
        };

        var summaryGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Height = 220
        };
        summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

        // Fila 1: Fecha
        summaryGrid.Controls.Add(new Label { Text = "📅 Fecha de Factura", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        var lblSummaryDate = new Label { Text = DateTime.Today.ToString("d 'de' MMMM 'de' yyyy"), Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(lblSummaryDate, 1, 0);

        // Fila 2: Subtotal
        summaryGrid.Controls.Add(new Label { Text = "Subtotal", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        _lblInvoiceSubtotal = new Label { Text = "$ 0", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblInvoiceSubtotal, 1, 1);

        // Fila 3: IVA
        summaryGrid.Controls.Add(new Label { Text = "IVA (19%)", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        _lblInvoiceIva = new Label { Text = "$ 0", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblInvoiceIva, 1, 2);

        // Fila 4: Total (separado con más altura)
        var totalLblRow = new Label { Text = "Total", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(11, 52, 22), Padding = new Padding(0, 12, 0, 0) };
        summaryGrid.Controls.Add(totalLblRow, 0, 3);
        _lblInvoiceTotal = new Label { Text = "$ 0", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(11, 52, 22), Padding = new Padding(0, 12, 0, 0) };
        summaryGrid.Controls.Add(_lblInvoiceTotal, 1, 3);

        summaryContentPanel.Controls.Add(summaryGrid);

        summaryCard.Controls.Add(summaryContentPanel);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 120,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };

        var btnSaveInvoice = new Button
        {
            Text = "✓ Crear y Enviar",
            Width = 260,
            Height = 48,
            BackColor = Color.FromArgb(33, 99, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12),
            Cursor = Cursors.Hand
        };
        btnSaveInvoice.FlatAppearance.BorderSize = 0;
        btnSaveInvoice.Click += (_, _) => SaveInvoice();
        buttonPanel.Controls.Add(btnSaveInvoice);

        var btnDraft = new Button
        {
            Text = "💾 Guardar como Borrador",
            Width = 260,
            Height = 48,
            BackColor = Color.FromArgb(245, 247, 245),
            ForeColor = Color.FromArgb(33, 33, 33),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            Margin = Padding.Empty,
            Cursor = Cursors.Hand
        };
        btnDraft.FlatAppearance.BorderSize = 1;
        btnDraft.FlatAppearance.BorderColor = Color.FromArgb(222, 226, 219);
        btnDraft.Click += (_, _) => ClearInvoiceForm();
        buttonPanel.Controls.Add(btnDraft);

        summaryCard.Controls.Add(buttonPanel);
        rightPanel.Controls.Add(summaryCard);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        return view;
    }

    private static Label CreateEditorLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private void ShowPendingModule(string moduleName)
    {
        SetMenuSelection(moduleName);
        MessageBox.Show($"El módulo {moduleName} se implementará en el siguiente bloque.", "En desarrollo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void OpenDatabaseFolder()
    {
        var dbPath = AppDatabase.DatabasePath;
        if (File.Exists(dbPath))
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dbPath}\"");
        }
        else
        {
            MessageBox.Show($"No se encontró la base de datos en:\n{dbPath}", "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ShowModule(string moduleName)
    {
        var menuSelection = moduleName switch
        {
            "NuevaFactura" => "Facturas",
            "NuevoPago" => "Pagos",
            _ => moduleName
        };
        SetMenuSelection(menuSelection);
        _btnHeaderAction.Visible = false;
        _headerAction = null;

        if (moduleName == "Clientes")
        {
            _headerTitle.Text = "Clientes";
            _headerSubtitle.Text = "Maestro de clientes con validación anti-duplicado";
            _btnHeaderAction.Text = "+ Nuevo Cliente";
            _btnHeaderAction.Visible = true;
            _headerAction = OpenNewCustomerDialog;
            ShowAnimatedView(_customersView, () => LoadCustomers(_txtSearchCustomer.Text));
            return;
        }

        if (moduleName == "Facturas")
        {
            _headerTitle.Text = "Facturas";
            _headerSubtitle.Text = "Gestiona tus facturas y remisiones";
            _btnHeaderAction.Text = "+ Nueva Factura";
            _btnHeaderAction.Visible = true;
            _headerAction = () => ShowModule("NuevaFactura");
            ShowAnimatedView(_invoicesView, LoadInvoicesList);
            return;
        }

        if (moduleName == "NuevaFactura")
        {
            _headerTitle.Text = "Nueva Factura";
            _headerSubtitle.Text = "Crea una nueva factura o remisión";
            _btnHeaderAction.Text = "← Volver";
            _btnHeaderAction.Visible = true;
            _headerAction = () => ShowModule("Facturas");
            ShowAnimatedView(_newInvoiceView, LoadInvoiceModule);
            return;
        }

        if (moduleName == "Pagos")
        {
            _headerTitle.Text = "Pagos";
            _headerSubtitle.Text = "Gestiona pagos de facturas";
            _btnHeaderAction.Text = "+ Nuevo Pago";
            _btnHeaderAction.Visible = true;
            _headerAction = () => ShowModule("NuevoPago");
            ShowAnimatedView(_paymentsView, LoadPaymentsList);
            return;
        }

        if (moduleName == "NuevoPago")
        {
            _headerTitle.Text = "Nuevo Pago";
            _headerSubtitle.Text = "Registra un nuevo pago para una factura";
            _btnHeaderAction.Text = "← Volver";
            _btnHeaderAction.Visible = true;
            _headerAction = () => ShowModule("Pagos");
            ShowAnimatedView(_newPaymentView, LoadPaymentModule);
            return;
        }

        if (moduleName == "Cartera")
        {
            _headerTitle.Text = "Cartera";
            _headerSubtitle.Text = "Análisis de cuentas por cobrar y vencimientos";
            ShowAnimatedView(_carteraView, LoadCartera);
            return;
        }

        if (moduleName == "Balance")
        {
            _headerTitle.Text = "Balance";
            _headerSubtitle.Text = "Análisis financiero y rentabilidad";
            ShowAnimatedView(_balanceView, LoadBalance);
            return;
        }

        if (moduleName == "Inventario")
        {
            _headerTitle.Text = "Inventario";
            _headerSubtitle.Text = "Gestión de productos, stock y movimientos";
            ShowAnimatedView(_inventarioView, LoadInventario);
            return;
        }

        _headerTitle.Text = "Dashboard";
        _headerSubtitle.Text = "Resumen de facturación y cartera";
        ShowAnimatedView(_dashboardView, () =>
        {
            LoadDashboard();
            LoadRecentInvoices();
            LoadLowStock();
        });
    }

    private void ShowAnimatedView(Panel targetView, Action loadAction)
    {
        _moduleAnimationTimer?.Stop();
        _moduleAnimationTimer?.Dispose();
        _moduleAnimationTimer = null;

        _contentHost.Controls.Clear();
        var hostSize = _contentHost.ClientSize;
        if (hostSize.Width <= 0 || hostSize.Height <= 0)
        {
            targetView.Dock = DockStyle.Fill;
            _contentHost.Controls.Add(targetView);
            loadAction();
            return;
        }

        targetView.Dock = DockStyle.None;
        targetView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        targetView.Bounds = new Rectangle(hostSize.Width, 0, hostSize.Width, hostSize.Height);
        _contentHost.Controls.Add(targetView);
        loadAction();

        _moduleAnimationTimer = new System.Windows.Forms.Timer { Interval = 12 };
        _moduleAnimationTimer.Tick += (_, _) =>
        {
            var step = Math.Max(24, targetView.Left / 5);
            var next = targetView.Left - step;
            if (next <= 0)
            {
                targetView.Left = 0;
                _moduleAnimationTimer?.Stop();
                _moduleAnimationTimer?.Dispose();
                _moduleAnimationTimer = null;

                targetView.Anchor = AnchorStyles.None;
                targetView.Dock = DockStyle.Fill;
                return;
            }

            targetView.Left = next;
        };
        _moduleAnimationTimer.Start();
    }

    private void SetMenuSelection(string selected)
    {
        foreach (var item in _menuButtons)
        {
            item.Value.BackColor = item.Key == selected
                ? Color.FromArgb(44, 98, 52)
                : Color.FromArgb(18, 57, 26);
        }
    }

    private static Label CreateCard(TableLayoutPanel cards, int column, String title, out Label secondary)
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8),
            BackColor = Color.White,
            Padding = new Padding(16),
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Height = 26
        };

        var valueLabel = new Label
        {
            Text = "$ 0",
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 52, 22),
            Height = 50
        };

        secondary = new Label
        {
            Text = string.Empty,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.DimGray,
            Height = 20
        };

        var accent = new Panel
        {
            Size = new Size(56, 56),
            BackColor = Color.FromArgb(240, 247, 240),
            Location = new Point(card.Width - 62, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        card.Controls.Add(accent);
        card.Controls.Add(secondary);
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        cards.Controls.Add(card, column, 0);
        return valueLabel;
    }

    private static Panel CreateDataPanel(string title, out DataGridView grid)
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12),
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14
        };

        var label = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };

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

        panel.Controls.Add(grid);
        panel.Controls.Add(label);
        return panel;
    }

    private static void ConfigureGridStyle(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Color.FromArgb(229, 229, 229);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 239, 224);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 245);
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 247, 245);
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(2, 6, 2, 6);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        grid.ColumnHeadersHeight = 50;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
    }

    private void LoadDashboard()
    {
        var snapshot = _dashboardService.GetSnapshot();
        _lblTotalFacturado.Text = snapshot.TotalFacturado.ToString("C0");
        _lblSaldoPendiente.Text = snapshot.SaldoPendiente.ToString("C0");
        _lblTotalPagos.Text = snapshot.TotalPagos.ToString("C0");
        _lblClientesActivos.Text = snapshot.ClientesActivos.ToString("N0");
        _lblPendingCount.Text = $"{snapshot.FacturasPendientes} facturas pendientes";
        _lblPaymentsCount.Text = $"{snapshot.PagosRegistrados} pagos registrados";
    }

    private void LoadCustomers(string? search)
    {
        var data = _customerRepository.GetGridRows(search);
        _gridCustomers.DataSource = data;

        var idColumn = _gridCustomers.Columns["Id"];
        if (idColumn != null)
        {
            idColumn.Visible = false;
        }

        _gridCustomers.Columns["Cliente"]!.HeaderText = "Cliente";
        _gridCustomers.Columns["NitCedula"]!.HeaderText = "NIT / Cédula";
        _gridCustomers.Columns["Contacto"]!.HeaderText = "Contacto";
        _gridCustomers.Columns["Balance"]!.HeaderText = "Balance";
        _gridCustomers.Columns["NumeroFacturas"]!.HeaderText = "Facturas";
        _gridCustomers.Columns["Balance"]!.DefaultCellStyle.Format = "C0";
        _gridCustomers.Columns["Cliente"]!.MinimumWidth = 220;
        _gridCustomers.Columns["NitCedula"]!.MinimumWidth = 140;
        _gridCustomers.Columns["Contacto"]!.MinimumWidth = 230;
        _gridCustomers.Columns["Balance"]!.MinimumWidth = 130;
        _gridCustomers.Columns["NumeroFacturas"]!.MinimumWidth = 90;

        EnsureCustomerActionColumns();
    }

    private void EnsureCustomerActionColumns()
    {
        if (_gridCustomers.Columns.Contains("AccionVer"))
        {
            return;
        }

        _gridCustomers.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "AccionVer",
            HeaderText = "Acciones",
            Text = "Ver",
            UseColumnTextForButtonValue = true,
            Width = 86,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        _gridCustomers.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "AccionEditar",
            HeaderText = "",
            Text = "Editar",
            UseColumnTextForButtonValue = true,
            Width = 86,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        _gridCustomers.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "AccionEliminar",
            HeaderText = "",
            Text = "Eliminar",
            UseColumnTextForButtonValue = true,
            Width = 92,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void OnCustomerGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var column = _gridCustomers.Columns[e.ColumnIndex].Name;
        switch (column)
        {
            case "AccionVer":
                ViewSelectedCustomer();
                break;
            case "AccionEditar":
                EditSelectedCustomer();
                break;
            case "AccionEliminar":
                DeleteSelectedCustomer();
                break;
        }
    }

    private void OpenNewCustomerDialog()
    {
        var draft = new CustomerDto
        {
            Codigo = _customerRepository.GenerateNextCode(),
            TipoIva = "GRAVADO",
            Activo = true
        };

        using var dialog = new CustomerEditorDialog(draft);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
        {
            return;
        }

        UpsertCustomer(dialog.Result);
    }

    private void EditSelectedCustomer()
    {
        if (_gridCustomers.CurrentRow?.DataBoundItem is not CustomerGridRowDto row)
        {
            return;
        }

        var selected = _customerRepository.GetById(row.Id);
        if (selected is null)
        {
            return;
        }

        using var dialog = new CustomerEditorDialog(selected);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
        {
            return;
        }

        UpsertCustomer(dialog.Result);
    }

    private void ViewSelectedCustomer()
    {
        if (_gridCustomers.CurrentRow?.DataBoundItem is not CustomerGridRowDto row)
        {
            return;
        }

        MessageBox.Show($"Cliente: {row.Cliente}\nNIT/Cédula: {row.NitCedula}\nContacto: {row.Contacto}\nBalance: {row.Balance:C0}\nFacturas: {row.NumeroFacturas}",
            "Detalle de cliente", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DeleteSelectedCustomer()
    {
        if (_gridCustomers.CurrentRow?.DataBoundItem is not CustomerGridRowDto row)
        {
            return;
        }

        var confirm = MessageBox.Show($"¿Deseas eliminar el cliente {row.Cliente}?", "Eliminar cliente", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var changed = _customerRepository.DeleteOrDeactivate(row.Id);
        if (!changed)
        {
            MessageBox.Show("No se pudo eliminar el cliente en la base de datos.", "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LoadCustomers(_txtSearchCustomer.Text);
        LoadDashboard();
        MessageBox.Show("Cliente eliminado correctamente.", "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpsertCustomer(CustomerDto customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Codigo))
        {
            customer = new CustomerDto
            {
                Id = customer.Id,
                Codigo = _customerRepository.GenerateNextCode(),
                Nombre = customer.Nombre,
                Nit = customer.Nit,
                Direccion = customer.Direccion,
                Telefono = customer.Telefono,
                Email = customer.Email,
                TipoIva = customer.TipoIva,
                Activo = customer.Activo
            };
        }

        long? excludeId = customer.Id > 0 ? customer.Id : null;
        if (_customerRepository.ExistsDuplicate(customer.Codigo, customer.Nit, excludeId))
        {
            MessageBox.Show("Ya existe un cliente con el mismo código o NIT.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (customer.Id > 0)
        {
            _customerRepository.Update(customer);
            MessageBox.Show("Cliente actualizado.", "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            _customerRepository.Insert(customer);
            MessageBox.Show("Cliente creado.", "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        LoadCustomers(_txtSearchCustomer.Text);
        LoadDashboard();
    }

    private void LoadInvoiceModule()
    {
        var customers = _invoiceRepository.GetActiveCustomers();
        _cmbInvoiceCustomer.DataSource = customers;
        _cmbInvoiceCustomer.DisplayMember = nameof(InvoiceCustomerLookupDto.DisplayName);
        _cmbInvoiceCustomer.ValueMember = nameof(InvoiceCustomerLookupDto.Id);

        var paymentMethods = _invoiceRepository.GetPaymentMethods();
        _cmbInvoicePaymentMethod.DataSource = paymentMethods;
        _cmbInvoicePaymentMethod.DisplayMember = nameof(PaymentMethodLookupDto.Nombre);
        _cmbInvoicePaymentMethod.ValueMember = nameof(PaymentMethodLookupDto.Id);

        var products = _invoiceRepository.GetProducts();

        if (products.Count == 0)
        {
            MessageBox.Show("No hay productos registrados en la base de datos. Por favor, verifica la inicialización.", "Sin Productos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        _cmbItemProduct.DataSource = products;
        _cmbItemProduct.DisplayMember = nameof(ProductLookupDto.DisplayName);
        _cmbItemProduct.ValueMember = nameof(ProductLookupDto.Id);

        _currentIvaRate = _invoiceRepository.GetIvaRate();
        LoadInvoicesList();
        ClearInvoiceForm();
    }

    private void LoadInvoicesList()
    {
        _gridInvoices.DataSource = _invoiceRepository.GetRecentInvoices();
        var idColumn = _gridInvoices.Columns["Id"];
        if (idColumn != null)
        {
            idColumn.Visible = false;
        }

        _gridInvoices.Columns["Numero"]!.HeaderText = "N° Factura";
        _gridInvoices.Columns["Fecha"]!.HeaderText = "Fecha";
        _gridInvoices.Columns["Cliente"]!.HeaderText = "Cliente";
        _gridInvoices.Columns["Total"]!.DefaultCellStyle.Format = "C0";
        _gridInvoices.Columns["Saldo"]!.DefaultCellStyle.Format = "C0";
    }

    private void OnProductChanged()
    {
        if (_cmbItemProduct.SelectedItem is not ProductLookupDto product)
        {
            _lblItemStock.Text = "Stock: -";
            return;
        }

        _txtItemDescription.Text = product.Nombre;
        _numItemPrice.Value = Math.Min(_numItemPrice.Maximum, product.PrecioBase);
        _lblItemStock.Text = $"Stock: {product.StockActual:N0}";
    }

    private void AddInvoiceItem()
    {
        if (_cmbItemProduct.SelectedItem is not ProductLookupDto product)
        {
            MessageBox.Show("Selecciona un producto.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var quantity = _numItemQuantity.Value;
        if (quantity <= 0)
        {
            MessageBox.Show("La cantidad debe ser mayor a cero.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (quantity > product.StockActual)
        {
            MessageBox.Show("La cantidad supera el stock disponible.", "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var price = _numItemPrice.Value;
        var total = Math.Round(quantity * price, 0);

        // Verificar si el producto ya existe en los ítems
        var existingItem = _invoiceItems.FirstOrDefault(x => x.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Cantidad += quantity;
            existingItem.TotalLinea = Math.Round(existingItem.Cantidad * existingItem.PrecioUnitario, 0);
        }
        else
        {
            _invoiceItems.Add(new InvoiceItemDraft
            {
                ProductId = product.Id,
                Codigo = product.Codigo,
                Descripcion = string.IsNullOrWhiteSpace(_txtItemDescription.Text) ? product.Nombre : _txtItemDescription.Text.Trim(),
                Cantidad = quantity,
                PrecioUnitario = price,
                AplicaIva = _chkItemIva.Checked,
                TotalLinea = total
            });
        }

        // Limpiar formulario para agregar más items
        _numItemQuantity.Value = 1;
        _txtItemDescription.Clear();
        if (_cmbItemProduct.Items.Count > 0)
        {
            _cmbItemProduct.SelectedIndex = 0;
        }

        RecalculateInvoiceTotals();
        _gridInvoiceItems.Refresh();
    }

    private void RemoveInvoiceItem()
    {
        if (_gridInvoiceItems.CurrentRow?.DataBoundItem is InvoiceItemDraft item)
        {
            _invoiceItems.Remove(item);
            RecalculateInvoiceTotals();
        }
    }

    private (decimal subtotal, decimal iva, decimal total) ComputeInvoiceTotals()
    {
        foreach (var item in _invoiceItems)
        {
            item.TotalLinea = Math.Round(item.Cantidad * item.PrecioUnitario, 0);
        }

        var subtotal = _invoiceItems.Sum(x => x.TotalLinea);
        var ivaBase = _invoiceItems.Where(x => x.AplicaIva).Sum(x => x.TotalLinea);
        var iva = Math.Round(ivaBase * _currentIvaRate, 0);
        var total = subtotal + iva;
        return (subtotal, iva, total);
    }

    private void RecalculateInvoiceTotals()
    {
        var totals = ComputeInvoiceTotals();
        _lblInvoiceSubtotal.Text = $"$ {totals.subtotal:N0}";
        _lblInvoiceIva.Text = $"$ {totals.iva:N0}";
        _lblInvoiceTotal.Text = $"$ {totals.total:N0}";
        _gridInvoiceItems.Refresh();
        ConfigureInvoiceItemsColumns();
    }

    private void ConfigureInvoiceItemsColumns()
    {
        if (_gridInvoiceItems.Columns.Count == 0)
        {
            return;
        }

        if (_gridInvoiceItems.Columns["ProductId"] != null)
        {
            _gridInvoiceItems.Columns["ProductId"]!.Visible = false;
        }

        _gridInvoiceItems.Columns["Codigo"]!.HeaderText = "Código";
        _gridInvoiceItems.Columns["Codigo"]!.ReadOnly = true;
        _gridInvoiceItems.Columns["Codigo"]!.Width = 100;

        _gridInvoiceItems.Columns["Descripcion"]!.HeaderText = "Descripción";
        _gridInvoiceItems.Columns["Descripcion"]!.ReadOnly = true;
        _gridInvoiceItems.Columns["Descripcion"]!.Width = 180;

        _gridInvoiceItems.Columns["Cantidad"]!.HeaderText = "Cantidad";
        _gridInvoiceItems.Columns["Cantidad"]!.ReadOnly = false;
        _gridInvoiceItems.Columns["Cantidad"]!.Width = 100;
        _gridInvoiceItems.Columns["Cantidad"]!.DefaultCellStyle.Format = "N0";

        _gridInvoiceItems.Columns["PrecioUnitario"]!.HeaderText = "Precio Unit.";
        _gridInvoiceItems.Columns["PrecioUnitario"]!.ReadOnly = false;
        _gridInvoiceItems.Columns["PrecioUnitario"]!.Width = 110;
        _gridInvoiceItems.Columns["PrecioUnitario"]!.DefaultCellStyle.Format = "C0";

        _gridInvoiceItems.Columns["AplicaIva"]!.HeaderText = "¿IVA?";
        _gridInvoiceItems.Columns["AplicaIva"]!.ReadOnly = false;
        _gridInvoiceItems.Columns["AplicaIva"]!.Width = 60;

        _gridInvoiceItems.Columns["TotalLinea"]!.HeaderText = "Total";
        _gridInvoiceItems.Columns["TotalLinea"]!.ReadOnly = true;
        _gridInvoiceItems.Columns["TotalLinea"]!.Width = 100;
        _gridInvoiceItems.Columns["TotalLinea"]!.DefaultCellStyle.Format = "C0";
    }

    private void SaveInvoice()
    {
        if (_cmbInvoiceCustomer.SelectedItem is not InvoiceCustomerLookupDto customer)
        {
            MessageBox.Show("Debes seleccionar un cliente activo.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_invoiceItems.Count == 0)
        {
            MessageBox.Show("Agrega al menos un ítem a la factura.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var invoiceNumber = _invoiceRepository.GenerateNextInvoiceNumber();

        var totals = ComputeInvoiceTotals();
        if (totals.total < 0)
        {
            MessageBox.Show("El total no puede ser negativo.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var methodId = _cmbInvoicePaymentMethod.SelectedValue is long methodValue
            ? methodValue
            : Convert.ToInt64(_cmbInvoicePaymentMethod.SelectedValue);

        var request = new InvoiceCreateRequest
        {
            Numero = invoiceNumber,
            Fecha = _dtpInvoiceDate.Value.Date,
            ClienteId = customer.Id,
            MetodoPagoId = methodId,
            Estado = "Enviada",
            Subtotal = totals.subtotal,
            IvaPorcentaje = _currentIvaRate,
            IvaValor = totals.iva,
            Retencion = 0,
            Total = totals.total,
            Saldo = totals.total,
            Notas = _txtInvoiceNotes.Text,
            Items = _invoiceItems.Select(x => new InvoiceItemInput
            {
                ProductId = x.ProductId,
                Descripcion = x.Descripcion,
                Cantidad = x.Cantidad,
                PrecioUnitario = x.PrecioUnitario,
                AplicaIva = x.AplicaIva,
                TotalLinea = x.TotalLinea
            }).ToList()
        };

        try
        {
            _invoiceRepository.CreateInvoice(request);
            MessageBox.Show("Factura emitida correctamente.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadInvoicesList();
            ClearInvoiceForm();
            LoadDashboard();
            LoadRecentInvoices();
            LoadLowStock();
            ShowModule("Facturas");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearInvoiceForm()
    {
        _dtpInvoiceDate.Value = DateTime.Today;
        _txtInvoiceNotes.Clear();
        _invoiceItems.Clear();

        if (_cmbInvoiceCustomer.Items.Count > 0)
        {
            _cmbInvoiceCustomer.SelectedIndex = 0;
        }

        if (_cmbInvoicePaymentMethod.Items.Count > 0)
        {
            _cmbInvoicePaymentMethod.SelectedIndex = 0;
        }

        if (_cmbItemProduct.Items.Count > 0)
        {
            _cmbItemProduct.SelectedIndex = 0;
        }

        _numItemQuantity.Value = 1;
        _chkItemIva.Checked = true;
        RecalculateInvoiceTotals();
    }

    private void LoadRecentInvoices()
    {
        const string sql = @"
SELECT f.Numero,
       f.Fecha,
       c.Nombre AS Cliente,
       f.Total,
       f.Saldo,
       f.Estado
FROM Factura f
INNER JOIN Cliente c ON c.Id = f.ClienteId
ORDER BY date(f.Fecha) DESC, f.Id DESC
LIMIT 12;";

        _gridRecentInvoices.DataSource = ExecuteQuery(sql);
        ConfigureGridStyle(_gridRecentInvoices);
    }

    private void LoadLowStock()
    {
        const string sql = @"
SELECT Codigo,
       Nombre,
       StockActual,
       StockMinimo,
       (StockMinimo - StockActual) AS Faltante
FROM ProductoExt
WHERE StockActual <= StockMinimo
ORDER BY Faltante DESC;";

        _gridLowStock.DataSource = ExecuteQuery(sql);
        ConfigureGridStyle(_gridLowStock);
    }

    private static DataTable ExecuteQuery(string sql)
    {
        using var connection = AppDatabase.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    private static Control CreateDashboardBars()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 36, 16, 24) };
        var barA = new Panel { Width = 58, Height = 120, BackColor = Color.FromArgb(231, 168, 57), Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(panel.Width - 170, panel.Height - 150) };
        var barB = new Panel { Width = 58, Height = 210, BackColor = Color.FromArgb(62, 146, 137), Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(panel.Width - 105, panel.Height - 240) };
        var axis = new Label { Text = "Oct    Nov    Dic    Ene    Feb    Mar", Dock = DockStyle.Bottom, Height = 24, ForeColor = Color.Gray };
        panel.Controls.Add(barA);
        panel.Controls.Add(barB);
        panel.Controls.Add(axis);
        return panel;
    }

    private static Control CreateStatusLegend()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var circle = new Label
        {
            Text = "◉",
            ForeColor = Color.FromArgb(39, 106, 22),
            Font = new Font("Segoe UI", 70, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(70, 60)
        };
        var legend = new Label
        {
            Text = "● Pagadas   ● Enviadas",
            ForeColor = Color.FromArgb(95, 95, 95),
            Dock = DockStyle.Bottom,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter
        };
        panel.Controls.Add(circle);
        panel.Controls.Add(legend);
        return panel;
    }

    private Panel BuildPaymentsListView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 72, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        view.Controls.Add(layout);

        var filterCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14)
        };

        var filterRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        var lblFilter = new Label
        {
            Text = "Historial de Pagos",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        filterRow.Controls.Add(lblFilter, 0, 0);
        filterCard.Controls.Add(filterRow);

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
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridPayments);
        tableCard.Controls.Add(_gridPayments);

        layout.Controls.Add(filterCard, 0, 0);
        layout.Controls.Add(tableCard, 0, 1);
        return view;
    }

    private Panel BuildNewPaymentView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 72, 0, 0),
            AutoScroll = true
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        view.Controls.Add(mainLayout);

        // Columna izquierda
        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Margin = new Padding(12, 12, 6, 12),
            Padding = Padding.Empty
        };

        var paymentCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var paymentTitle = new Label
        {
            Text = "📝 Información del Pago",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        paymentCard.Controls.Add(paymentTitle);

        var paymentGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 7,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0),
            Height = 360
        };
        paymentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Factura
        paymentGrid.Controls.Add(new Label { Text = "Factura *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 0);
        _cmbPaymentInvoice = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbPaymentInvoice.SelectedIndexChanged += (_, _) => OnInvoiceSelected();
        paymentGrid.Controls.Add(_cmbPaymentInvoice, 0, 1);

        // Fecha
        paymentGrid.Controls.Add(new Label { Text = "Fecha *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 2);
        _dtpPaymentDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        paymentGrid.Controls.Add(_dtpPaymentDate, 0, 3);

        // Monto
        paymentGrid.Controls.Add(new Label { Text = "Monto *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 4);
        _numPaymentAmount = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Maximum = 999999999, ThousandsSeparator = true };
        paymentGrid.Controls.Add(_numPaymentAmount, 0, 5);

        // Método de Pago
        paymentGrid.Controls.Add(new Label { Text = "Método de Pago", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 6);
        _cmbPaymentMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        paymentGrid.Controls.Add(_cmbPaymentMethod, 0, 7);

        paymentCard.Controls.Add(paymentGrid);

        // Referencia y Notas
        var refNotesGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(0, 12, 0, 0),
            Padding = Padding.Empty,
            Height = 160
        };

        refNotesGrid.Controls.Add(new Label { Text = "Referencia", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 0);
        _txtPaymentReference = new TextBox { Dock = DockStyle.Fill, Multiline = false };
        refNotesGrid.Controls.Add(_txtPaymentReference, 0, 1);

        refNotesGrid.Controls.Add(new Label { Text = "Notas", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 2);
        _txtPaymentNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true };
        refNotesGrid.Controls.Add(_txtPaymentNotes, 0, 3);

        paymentCard.Controls.Add(refNotesGrid);
        leftLayout.Controls.Add(paymentCard, 0, 0);
        mainLayout.Controls.Add(leftLayout, 0, 0);

        // Columna derecha: Resumen
        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 12, 12, 12),
            Padding = Padding.Empty,
            AutoScroll = true
        };

        var summaryCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(20)
        };

        var summaryTitle = new Label
        {
            Text = "Resumen",
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Margin = new Padding(0, 0, 0, 16)
        };
        summaryCard.Controls.Add(summaryTitle);

        // Panel con scroll para el contenido del resumen
        var summaryContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 20, 0, 0)
        };

        var summaryGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Height = 150
        };
        summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        summaryGrid.Controls.Add(new Label { Text = "Factura:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        var lblSummaryInvoice = new Label { Text = "-", Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(lblSummaryInvoice, 1, 0);

        summaryGrid.Controls.Add(new Label { Text = "Fecha de Pago:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        var lblSummaryDate = new Label { Text = DateTime.Today.ToString("d 'de' MMMM 'de' yyyy"), Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(lblSummaryDate, 1, 1);

        summaryGrid.Controls.Add(new Label { Text = "Monto a Pagar:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        _lblPaymentTotal = new Label { Text = "$ 0", Font = new Font("Segoe UI", 14, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(11, 52, 22) };
        summaryGrid.Controls.Add(_lblPaymentTotal, 1, 2);

        summaryContentPanel.Controls.Add(summaryGrid);

        summaryCard.Controls.Add(summaryContentPanel);

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 100,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };

        var btnSave = new Button
        {
            Text = "✓ Registrar Pago",
            Width = 240,
            Height = 48,
            BackColor = Color.FromArgb(33, 99, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SavePayment();
        btnPanel.Controls.Add(btnSave);

        var btnClear = new Button
        {
            Text = "🔄 Limpiar",
            Width = 240,
            Height = 48,
            BackColor = Color.FromArgb(245, 247, 245),
            ForeColor = Color.FromArgb(33, 33, 33),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand
        };
        btnClear.FlatAppearance.BorderSize = 1;
        btnClear.FlatAppearance.BorderColor = Color.FromArgb(222, 226, 219);
        btnClear.Click += (_, _) => ClearPaymentForm();
        btnPanel.Controls.Add(btnClear);

        summaryCard.Controls.Add(btnPanel);
        rightPanel.Controls.Add(summaryCard);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        return view;
    }

    private void LoadPaymentsList()
    {
        var data = _paymentRepository.GetPaymentHistory();
        _gridPayments.DataSource = data;

        if (_gridPayments.Columns.Count > 0)
        {
            _gridPayments.Columns["Id"]!.Visible = false;
            _gridPayments.Columns["NumeroFactura"]!.HeaderText = "N° Factura";
            _gridPayments.Columns["Cliente"]!.HeaderText = "Cliente";
            _gridPayments.Columns["Fecha"]!.HeaderText = "Fecha";
            _gridPayments.Columns["Valor"]!.HeaderText = "Valor";
            _gridPayments.Columns["Valor"]!.DefaultCellStyle.Format = "C0";
            _gridPayments.Columns["MetodoPago"]!.HeaderText = "Método";
            _gridPayments.Columns["Referencia"]!.HeaderText = "Referencia";
            _gridPayments.Columns["Notas"]!.HeaderText = "Notas";
        }
    }

    private void LoadPaymentModule()
    {
        var pendingInvoices = _paymentRepository.GetPendingInvoices();
        _cmbPaymentInvoice.DataSource = pendingInvoices;
        _cmbPaymentInvoice.DisplayMember = nameof(PaymentLookupDto.DisplayName);
        _cmbPaymentInvoice.ValueMember = nameof(PaymentLookupDto.Id);

        var paymentMethods = _invoiceRepository.GetPaymentMethods();
        _cmbPaymentMethod.DataSource = paymentMethods;
        _cmbPaymentMethod.DisplayMember = nameof(PaymentMethodLookupDto.Nombre);
        _cmbPaymentMethod.ValueMember = nameof(PaymentMethodLookupDto.Id);

        _dtpPaymentDate.Value = DateTime.Today;
        ClearPaymentForm();
    }

    private void OnInvoiceSelected()
    {
        if (_cmbPaymentInvoice.SelectedItem is not PaymentLookupDto invoice)
        {
            _lblPaymentBalance.Text = "$ 0";
            _numPaymentAmount.Value = 0;
            return;
        }

        _lblPaymentBalance.Text = $"$ {invoice.Saldo:N0}";
        _numPaymentAmount.Value = Math.Min(_numPaymentAmount.Maximum, invoice.Saldo);
        _lblPaymentTotal.Text = $"$ {_numPaymentAmount.Value:N0}";
    }

    private void SavePayment()
    {
        if (_cmbPaymentInvoice.SelectedItem is not PaymentLookupDto invoice)
        {
            MessageBox.Show("Selecciona una factura.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var amount = _numPaymentAmount.Value;
        if (amount <= 0)
        {
            MessageBox.Show("El monto debe ser mayor a cero.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (amount > invoice.Saldo)
        {
            MessageBox.Show("El monto supera el saldo pendiente.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var methodId = _cmbPaymentMethod.SelectedValue is long methodValue ? methodValue : Convert.ToInt64(_cmbPaymentMethod.SelectedValue);

        var request = new PaymentCreateRequest
        {
            FacturaId = invoice.Id,
            Fecha = _dtpPaymentDate.Value.Date,
            Valor = amount,
            MetodoPagoId = methodId,
            Referencia = _txtPaymentReference.Text,
            Notas = _txtPaymentNotes.Text
        };

        try
        {
            _paymentRepository.CreatePayment(request);
            MessageBox.Show("Pago registrado correctamente.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadPaymentsList();
            ClearPaymentForm();
            LoadDashboard();
            ShowModule("Pagos");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearPaymentForm()
    {
        _numPaymentAmount.Value = 0;
        _txtPaymentReference.Clear();
        _txtPaymentNotes.Clear();
        _dtpPaymentDate.Value = DateTime.Today;

        if (_cmbPaymentInvoice.Items.Count > 0)
        {
            _cmbPaymentInvoice.SelectedIndex = 0;
        }

        if (_cmbPaymentMethod.Items.Count > 0)
        {
            _cmbPaymentMethod.SelectedIndex = 0;
        }

        _lblPaymentBalance.Text = "$ 0";
        _lblPaymentTotal.Text = "$ 0";
    }

    private Panel BuildCarteraView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 72, 0, 0),
            AutoScroll = true
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(12),
            Padding = Padding.Empty
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        view.Controls.Add(mainLayout);

        // 1. TARJETAS DE RESUMEN
        var cardsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 8)
        };
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        _lblCarteraClientes = CreateCard(cardsPanel, 0, "Clientes con Saldo", out _);
        _lblCarteraFacturas = CreateCard(cardsPanel, 1, "Facturas Pendientes", out _);
        _lblCarteraTotalCobrar = CreateCard(cardsPanel, 2, "Total por Cobrar", out _);
        _lblCarteraVencido = CreateCard(cardsPanel, 3, "Saldo Vencido", out _);

        mainLayout.Controls.Add(cardsPanel, 0, 0);

        // 2. FACTURAS PENDIENTES
        var facturasPendientesCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var facturasPendientesTitle = new Label
        {
            Text = "📄 Facturas Pendientes de Pago",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        facturasPendientesCard.Controls.Add(facturasPendientesTitle);

        _gridFacturasPendientes = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridFacturasPendientes);
        facturasPendientesCard.Controls.Add(_gridFacturasPendientes);

        mainLayout.Controls.Add(facturasPendientesCard, 0, 1);

        // 3. CLIENTES CON SALDO
        var clientesSaldoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var clientesSaldoTitle = new Label
        {
            Text = "👥 Clientes con Saldo Pendiente",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        clientesSaldoCard.Controls.Add(clientesSaldoTitle);

        _gridClientesSaldo = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowTemplate = { Height = 52 }
        };
        _gridClientesSaldo.CellDoubleClick += (_, _) => EditSelectedCustomer();
        _gridClientesSaldo.CellContentClick += OnCustomerGridCellContentClick;
        ConfigureGridStyle(_gridClientesSaldo);
        _gridClientesSaldo.DefaultCellStyle.Padding = new Padding(6, 8, 6, 8);
        clientesSaldoCard.Controls.Add(_gridClientesSaldo);

        mainLayout.Controls.Add(clientesSaldoCard, 0, 2);

        // 4. EDAD DE SALDOS
        var edadSaldosCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };

        var edadSaldosTitle = new Label
        {
            Text = "📊 Análisis de Edad de Saldos",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        edadSaldosCard.Controls.Add(edadSaldosTitle);

        _gridEdadSaldos = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridEdadSaldos);
        edadSaldosCard.Controls.Add(_gridEdadSaldos);

        mainLayout.Controls.Add(edadSaldosCard, 0, 3);

        return view;
    }

    private void LoadCartera()
    {
        var resumen = _carteraRepository.GetResumen();
        _lblCarteraClientes.Text = resumen.ClientesConSaldo.ToString("N0");
        _lblCarteraFacturas.Text = resumen.FacturasPendientes.ToString("N0");
        _lblCarteraTotalCobrar.Text = resumen.TotalPorCobrar.ToString("C0");
        _lblCarteraVencido.Text = resumen.SaldoVencido.ToString("C0");

        // Cargar facturas pendientes
        var facturasPendientes = _carteraRepository.GetFacturasPendientes();
        _gridFacturasPendientes.DataSource = facturasPendientes;

        if (_gridFacturasPendientes.Columns.Count > 0)
        {
            _gridFacturasPendientes.Columns["Id"]!.Visible = false;
            _gridFacturasPendientes.Columns["Numero"]!.HeaderText = "N° Factura";
            _gridFacturasPendientes.Columns["Numero"]!.Width = 120;
            _gridFacturasPendientes.Columns["Fecha"]!.HeaderText = "Fecha";
            _gridFacturasPendientes.Columns["Fecha"]!.Width = 100;
            _gridFacturasPendientes.Columns["Cliente"]!.HeaderText = "Cliente";
            _gridFacturasPendientes.Columns["Total"]!.HeaderText = "Total";
            _gridFacturasPendientes.Columns["Total"]!.DefaultCellStyle.Format = "C0";
            _gridFacturasPendientes.Columns["Saldo"]!.HeaderText = "Saldo";
            _gridFacturasPendientes.Columns["Saldo"]!.DefaultCellStyle.Format = "C0";
            _gridFacturasPendientes.Columns["Saldo"]!.Width = 110;
            _gridFacturasPendientes.Columns["DiasTranscurridos"]!.HeaderText = "Días";
            _gridFacturasPendientes.Columns["DiasTranscurridos"]!.Width = 60;
            _gridFacturasPendientes.Columns["EstadoVencimiento"]!.HeaderText = "Estado";
            _gridFacturasPendientes.Columns["EstadoVencimiento"]!.Width = 130;

            // Colorear filas según vencimiento
            foreach (DataGridViewRow row in _gridFacturasPendientes.Rows)
            {
                if (row.DataBoundItem is FacturaPendienteDto factura)
                {
                    if (factura.DiasTranscurridos > 90)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                    }
                    else if (factura.DiasTranscurridos > 60)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230);
                    }
                    else if (factura.DiasTranscurridos > 30)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 230);
                    }
                }
            }
        }

        // Cargar clientes con saldo
        var clientesSaldo = _carteraRepository.GetClientesConSaldo();
        _gridClientesSaldo.DataSource = clientesSaldo;

        if (_gridClientesSaldo.Columns.Count > 0)
        {
            _gridClientesSaldo.Columns["Id"]!.Visible = false;
            _gridClientesSaldo.Columns["Codigo"]!.HeaderText = "Código";
            _gridClientesSaldo.Columns["Codigo"]!.Width = 100;
            _gridClientesSaldo.Columns["Nombre"]!.HeaderText = "Cliente";
            _gridClientesSaldo.Columns["Telefono"]!.HeaderText = "Teléfono";
            _gridClientesSaldo.Columns["Telefono"]!.Width = 120;
            _gridClientesSaldo.Columns["FacturasPendientes"]!.HeaderText = "Facturas";
            _gridClientesSaldo.Columns["FacturasPendientes"]!.Width = 90;
            _gridClientesSaldo.Columns["SaldoTotal"]!.HeaderText = "Saldo Total";
            _gridClientesSaldo.Columns["SaldoTotal"]!.DefaultCellStyle.Format = "C0";
            _gridClientesSaldo.Columns["SaldoVencido"]!.HeaderText = "Saldo Vencido";
            _gridClientesSaldo.Columns["SaldoVencido"]!.DefaultCellStyle.Format = "C0";
            _gridClientesSaldo.Columns["SaldoVencido"]!.Width = 130;
        }

        // Cargar edad de saldos
        var edadSaldos = _carteraRepository.GetEdadSaldos();
        _gridEdadSaldos.DataSource = edadSaldos;

        if (_gridEdadSaldos.Columns.Count > 0)
        {
            _gridEdadSaldos.Columns["RangoEdad"]!.HeaderText = "Rango de Edad";
            _gridEdadSaldos.Columns["RangoEdad"]!.Width = 150;
            _gridEdadSaldos.Columns["CantidadFacturas"]!.HeaderText = "Cantidad Facturas";
            _gridEdadSaldos.Columns["CantidadFacturas"]!.Width = 150;
            _gridEdadSaldos.Columns["TotalSaldo"]!.HeaderText = "Total Saldo";
            _gridEdadSaldos.Columns["TotalSaldo"]!.DefaultCellStyle.Format = "C0";
            
            // Colorear según edad
            foreach (DataGridViewRow row in _gridEdadSaldos.Rows)
            {
                if (row.DataBoundItem is EdadSaldoDto edad)
                {
                    if (edad.RangoEdad.Contains("90"))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (edad.RangoEdad.Contains("61-90"))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230);
                    }
                    else if (edad.RangoEdad.Contains("31-60"))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 230);
                    }
                }
            }
        }
    }

    private Panel BuildBalanceView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 72, 0, 0),
            AutoScroll = true
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(12),
            Padding = Padding.Empty
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        view.Controls.Add(mainLayout);

        // 1. TARJETAS DE RESUMEN
        var cardsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 8)
        };
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        _lblBalanceFacturado = CreateCard(cardsPanel, 0, "Total Facturado", out var lblFacturasEmitidas);
        _lblBalanceRecaudado = CreateCard(cardsPanel, 1, "Total Recaudado", out _);
        _lblBalanceCuentasPorCobrar = CreateCard(cardsPanel, 2, "Cuentas por Cobrar", out _);
        _lblBalanceNeto = CreateCard(cardsPanel, 3, "Balance Neto", out _);

        mainLayout.Controls.Add(cardsPanel, 0, 0);

        // 2. BALANCE MENSUAL Y TOP CLIENTES
        var analyticsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var balanceMensualCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 6, 12)
        };

        var balanceMensualTitle = new Label
        {
            Text = "📈 Balance Mensual (Últimos 6 Meses)",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        balanceMensualCard.Controls.Add(balanceMensualTitle);

        _gridBalanceMensual = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridBalanceMensual);
        balanceMensualCard.Controls.Add(_gridBalanceMensual);

        analyticsLayout.Controls.Add(balanceMensualCard, 0, 0);

        var topClientesCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(6, 0, 0, 12)
        };

        var topClientesTitle = new Label
        {
            Text = "🏆 Top 10 Clientes",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        topClientesCard.Controls.Add(topClientesTitle);

        _gridTopClientes = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridTopClientes);
        topClientesCard.Controls.Add(_gridTopClientes);

        analyticsLayout.Controls.Add(topClientesCard, 1, 0);

        mainLayout.Controls.Add(analyticsLayout, 0, 1);

        // 3. INFORMACIÓN ADICIONAL
        var infoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };

        var infoTitle = new Label
        {
            Text = "📊 Indicadores Clave del Mes Actual",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        infoCard.Controls.Add(infoTitle);

        var infoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 12, 0, 0)
        };

        var lblInfo = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(90, 90, 90),
            Text = "Cargando indicadores..."
        };
        infoPanel.Controls.Add(lblInfo);
        infoCard.Controls.Add(infoPanel);

        mainLayout.Controls.Add(infoCard, 0, 2);

        // Cargar datos del balance
        LoadBalance();
        return view;
    }

    private void LoadBalance()
    {
        var resumen = _balanceRepository.GetResumen();
        _lblBalanceFacturado.Text = resumen.TotalFacturado.ToString("C0");
        _lblBalanceRecaudado.Text = resumen.TotalRecaudado.ToString("C0");
        _lblBalanceCuentasPorCobrar.Text = resumen.CuentasPorCobrar.ToString("C0");
        _lblBalanceNeto.Text = resumen.BalanceNeto.ToString("C0");

        // Cargar balance mensual
        var balanceMensual = _balanceRepository.GetBalanceMensual();
        _gridBalanceMensual.DataSource = balanceMensual;

        if (_gridBalanceMensual.Columns.Count > 0)
        {
            _gridBalanceMensual.Columns["Periodo"]!.Visible = false;
            _gridBalanceMensual.Columns["MesNombre"]!.HeaderText = "Mes";
            _gridBalanceMensual.Columns["MesNombre"]!.Width = 100;
            _gridBalanceMensual.Columns["TotalFacturado"]!.HeaderText = "Facturado";
            _gridBalanceMensual.Columns["TotalFacturado"]!.DefaultCellStyle.Format = "C0";
            _gridBalanceMensual.Columns["TotalRecaudado"]!.HeaderText = "Recaudado";
            _gridBalanceMensual.Columns["TotalRecaudado"]!.DefaultCellStyle.Format = "C0";
            _gridBalanceMensual.Columns["NumeroFacturas"]!.HeaderText = "N° Facturas";
            _gridBalanceMensual.Columns["NumeroFacturas"]!.Width = 100;

            // Colorear mes actual
            var mesActual = DateTime.Now.ToString("yyyy-MM");
            foreach (DataGridViewRow row in _gridBalanceMensual.Rows)
            {
                if (row.DataBoundItem is BalanceMensualDto balance && balance.Periodo == mesActual)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(240, 250, 240);
                    row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
        }

        // Cargar top clientes
        var topClientes = _balanceRepository.GetTopClientes();
        _gridTopClientes.DataSource = topClientes;

        if (_gridTopClientes.Columns.Count > 0)
        {
            _gridTopClientes.Columns["Id"]!.Visible = false;
            _gridTopClientes.Columns["Codigo"]!.HeaderText = "Código";
            _gridTopClientes.Columns["Codigo"]!.Width = 80;
            _gridTopClientes.Columns["Nombre"]!.HeaderText = "Cliente";
            _gridTopClientes.Columns["NumeroFacturas"]!.HeaderText = "Facturas";
            _gridTopClientes.Columns["NumeroFacturas"]!.Width = 80;
            _gridTopClientes.Columns["TotalFacturado"]!.HeaderText = "Total";
            _gridTopClientes.Columns["TotalFacturado"]!.DefaultCellStyle.Format = "C0";
            _gridTopClientes.Columns["TotalFacturado"]!.Width = 110;
            _gridTopClientes.Columns["SaldoPendiente"]!.HeaderText = "Saldo";
            _gridTopClientes.Columns["SaldoPendiente"]!.DefaultCellStyle.Format = "C0";
            _gridTopClientes.Columns["SaldoPendiente"]!.Width = 110;
            _gridTopClientes.Columns["PorcentajeTotal"]!.HeaderText = "%";
            _gridTopClientes.Columns["PorcentajeTotal"]!.DefaultCellStyle.Format = "N2";
            _gridTopClientes.Columns["PorcentajeTotal"]!.Width = 60;

            // Colorear top 3
            for (int i = 0; i < Math.Min(3, _gridTopClientes.Rows.Count); i++)
            {
                var row = _gridTopClientes.Rows[i];
                row.DefaultCellStyle.BackColor = i switch
                {
                    0 => Color.FromArgb(255, 248, 220), // Oro
                    1 => Color.FromArgb(240, 240, 240), // Plata
                    2 => Color.FromArgb(255, 239, 213), // Bronce
                    _ => Color.White
                };
                row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
        }
    }

    private Panel BuildInventarioView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 72, 0, 0),
            AutoScroll = true
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(12),
            Padding = Padding.Empty
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        view.Controls.Add(mainLayout);

        // 1. TARJETAS DE RESUMEN
        var cardsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 8)
        };
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        _lblInventarioTotalProductos = CreateCard(cardsPanel, 0, "Total Productos", out _);
        _lblInventarioValorTotal = CreateCard(cardsPanel, 1, "Valor Inventario", out _);
        _lblInventarioStockBajo = CreateCard(cardsPanel, 2, "Stock Bajo", out _);
        _lblInventarioAgotados = CreateCard(cardsPanel, 3, "Productos Agotados", out _);

        mainLayout.Controls.Add(cardsPanel, 0, 0);

        // 2. BARRA DE BÚSQUEDA
        var searchCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 0, 12)
        };

        var searchRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));

        _txtSearchInventario = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar producto por código o nombre...",
            Font = new Font("Segoe UI", 10)
        };
        _txtSearchInventario.TextChanged += (_, _) => LoadProductosInventario(_txtSearchInventario.Text);

        var btnClearSearch = new Button
        {
            Text = "✕",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        btnClearSearch.FlatAppearance.BorderSize = 0;
        btnClearSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        btnClearSearch.Click += (_, _) => { _txtSearchInventario.Clear(); _txtSearchInventario.Focus(); };

        searchRow.Controls.Add(_txtSearchInventario, 0, 0);
        searchRow.Controls.Add(btnClearSearch, 1, 0);
        searchCard.Controls.Add(searchRow);

        mainLayout.Controls.Add(searchCard, 0, 1);

        // 3. PRODUCTOS Y MOVIMIENTOS
        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var productosCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 6, 12)
        };

        var productosTitle = new Label
        {
            Text = "📦 Productos en Inventario",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        productosCard.Controls.Add(productosTitle);

        _gridProductosInventario = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridProductosInventario);
        productosCard.Controls.Add(_gridProductosInventario);

        contentLayout.Controls.Add(productosCard, 0, 0);

        var movimientosCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(6, 0, 0, 12)
        };

        var movimientosTitle = new Label
        {
            Text = "📋 Movimientos Recientes",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        movimientosCard.Controls.Add(movimientosTitle);

        _gridMovimientos = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridMovimientos);
        movimientosCard.Controls.Add(_gridMovimientos);

        contentLayout.Controls.Add(movimientosCard, 1, 0);

        mainLayout.Controls.Add(contentLayout, 0, 2);

        // 4. PRODUCTOS CON STOCK BAJO
        var stockBajoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };

        var stockBajoTitle = new Label
        {
            Text = "⚠️ Alertas de Stock Bajo",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        stockBajoCard.Controls.Add(stockBajoTitle);

        _gridStockBajo = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        ConfigureGridStyle(_gridStockBajo);
        stockBajoCard.Controls.Add(_gridStockBajo);

        mainLayout.Controls.Add(stockBajoCard, 0, 3);

        return view;
    }

    private void LoadInventario()
    {
        var resumen = _inventarioRepository.GetResumen();
        _lblInventarioTotalProductos.Text = resumen.TotalProductos.ToString("N0");
        _lblInventarioValorTotal.Text = resumen.ValorInventario.ToString("C0");
        _lblInventarioStockBajo.Text = resumen.ProductosStockBajo.ToString("N0");
        _lblInventarioAgotados.Text = resumen.ProductosAgotados.ToString("N0");

        LoadProductosInventario(_txtSearchInventario.Text);
        LoadMovimientos();
        LoadStockBajo();
    }

    private void LoadProductosInventario(string? search)
    {
        var productos = _inventarioRepository.GetProductos(search);
        _gridProductosInventario.DataSource = productos;

        if (_gridProductosInventario.Columns.Count > 0)
        {
            _gridProductosInventario.Columns["Id"]!.Visible = false;
            _gridProductosInventario.Columns["Codigo"]!.HeaderText = "Código";
            _gridProductosInventario.Columns["Codigo"]!.Width = 100;
            _gridProductosInventario.Columns["Nombre"]!.HeaderText = "Producto";
            _gridProductosInventario.Columns["Unidad"]!.HeaderText = "Unidad";
            _gridProductosInventario.Columns["Unidad"]!.Width = 120;
            _gridProductosInventario.Columns["StockActual"]!.HeaderText = "Stock";
            _gridProductosInventario.Columns["StockActual"]!.DefaultCellStyle.Format = "N0";
            _gridProductosInventario.Columns["StockActual"]!.Width = 80;
            _gridProductosInventario.Columns["StockMinimo"]!.HeaderText = "Mínimo";
            _gridProductosInventario.Columns["StockMinimo"]!.DefaultCellStyle.Format = "N0";
            _gridProductosInventario.Columns["StockMinimo"]!.Width = 80;
            _gridProductosInventario.Columns["PrecioBase"]!.HeaderText = "Precio";
            _gridProductosInventario.Columns["PrecioBase"]!.DefaultCellStyle.Format = "C0";
            _gridProductosInventario.Columns["ValorStock"]!.HeaderText = "Valor Stock";
            _gridProductosInventario.Columns["ValorStock"]!.DefaultCellStyle.Format = "C0";
            _gridProductosInventario.Columns["ValorStock"]!.Width = 110;
            _gridProductosInventario.Columns["EstadoStock"]!.HeaderText = "Estado";
            _gridProductosInventario.Columns["EstadoStock"]!.Width = 90;

            // Colorear según estado
            foreach (DataGridViewRow row in _gridProductosInventario.Rows)
            {
                if (row.DataBoundItem is ProductoInventarioDto producto)
                {
                    switch (producto.EstadoStock)
                    {
                        case "Agotado":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                            row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                            break;
                        case "Bajo":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230);
                            break;
                        case "Medio":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 230);
                            break;
                    }
                }
            }
        }
    }

    private void LoadMovimientos()
    {
        var movimientos = _inventarioRepository.GetMovimientos(50);
        _gridMovimientos.DataSource = movimientos;

        if (_gridMovimientos.Columns.Count > 0)
        {
            _gridMovimientos.Columns["Id"]!.Visible = false;
            _gridMovimientos.Columns["Fecha"]!.HeaderText = "Fecha";
            _gridMovimientos.Columns["Fecha"]!.Width = 90;
            _gridMovimientos.Columns["Codigo"]!.HeaderText = "Código";
            _gridMovimientos.Columns["Codigo"]!.Width = 80;
            _gridMovimientos.Columns["Producto"]!.HeaderText = "Producto";
            _gridMovimientos.Columns["Tipo"]!.HeaderText = "Tipo";
            _gridMovimientos.Columns["Tipo"]!.Width = 80;
            _gridMovimientos.Columns["Cantidad"]!.HeaderText = "Cantidad";
            _gridMovimientos.Columns["Cantidad"]!.DefaultCellStyle.Format = "N0";
            _gridMovimientos.Columns["Cantidad"]!.Width = 80;
            _gridMovimientos.Columns["Referencia"]!.HeaderText = "Referencia";

            // Colorear según tipo
            foreach (DataGridViewRow row in _gridMovimientos.Rows)
            {
                if (row.DataBoundItem is MovimientoInventarioDto movimiento)
                {
                    if (movimiento.Tipo == "ENTRADA")
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 120, 0);
                    }
                    else if (movimiento.Tipo == "SALIDA")
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                    }
                }
            }
        }
    }

    private void LoadStockBajo()
    {
        var stockBajo = _inventarioRepository.GetProductosStockBajo();
        _gridStockBajo.DataSource = stockBajo;

        if (_gridStockBajo.Columns.Count > 0)
        {
            _gridStockBajo.Columns["Id"]!.Visible = false;
            _gridStockBajo.Columns["Codigo"]!.HeaderText = "Código";
            _gridStockBajo.Columns["Codigo"]!.Width = 100;
            _gridStockBajo.Columns["Nombre"]!.HeaderText = "Producto";
            _gridStockBajo.Columns["Unidad"]!.HeaderText = "Unidad";
            _gridStockBajo.Columns["Unidad"]!.Width = 120;
            _gridStockBajo.Columns["StockActual"]!.HeaderText = "Stock Actual";
            _gridStockBajo.Columns["StockActual"]!.DefaultCellStyle.Format = "N0";
            _gridStockBajo.Columns["StockActual"]!.Width = 100;
            _gridStockBajo.Columns["StockMinimo"]!.HeaderText = "Stock Mínimo";
            _gridStockBajo.Columns["StockMinimo"]!.DefaultCellStyle.Format = "N0";
            _gridStockBajo.Columns["StockMinimo"]!.Width = 110;
            _gridStockBajo.Columns["Faltante"]!.HeaderText = "Faltante";
            _gridStockBajo.Columns["Faltante"]!.DefaultCellStyle.Format = "N0";
            _gridStockBajo.Columns["Faltante"]!.Width = 90;
            _gridStockBajo.Columns["PrecioBase"]!.HeaderText = "Precio";
            _gridStockBajo.Columns["PrecioBase"]!.DefaultCellStyle.Format = "C0";
            _gridStockBajo.Columns["ValorFaltante"]!.HeaderText = "Valor Faltante";
            _gridStockBajo.Columns["ValorFaltante"]!.DefaultCellStyle.Format = "C0";
            _gridStockBajo.Columns["ValorFaltante"]!.Width = 120;

            // Colorear según criticidad
            foreach (DataGridViewRow row in _gridStockBajo.Rows)
            {
                if (row.DataBoundItem is ProductoStockBajoDto producto)
                {
                    if (producto.StockActual == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (producto.Faltante >= producto.StockMinimo)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 230);
                    }
                    else if (producto.Faltante > 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 230);
                    }
                }
            }
        }
    }

    private sealed class InvoiceItemDraft
    {
        public long ProductId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool AplicaIva { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
