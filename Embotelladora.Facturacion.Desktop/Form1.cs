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
    private Button _btnHeaderSecondaryAction = null!;
    private Action? _headerAction;
    private Action? _headerSecondaryAction;

    private Label _lblTotalFacturado = null!;
    private Label _lblSaldoPendiente = null!;
    private Label _lblTotalPagos = null!;
    private Label _lblClientesActivos = null!;
    private Label _lblPendingCount = null!;
    private Label _lblPaymentsCount = null!;
    private DataGridView _gridRecentInvoices = null!;
    private DataGridView _gridLowStock = null!;
    private BarChartPanel _chartFacturacion = null!;
    private DonutChartPanel _chartEstadoFacturas = null!;
    private HorizontalBarChartPanel _chartTopClientes = null!;
    private DonutChartPanel _chartMetodosPago = null!;
    private readonly Dictionary<string, Button> _dashboardPeriodButtons = [];
    private string _dashboardChartPeriodo = "6 Meses";

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
    private Label _lblInvoiceSubtotal = null!;
    private Label _lblInvoiceTotal = null!;
    private Label _lblInvoiceItemsCount = null!;
    private Label _lblInvoiceSummaryDate = null!;
    private Label _lblItemStock = null!;

    // Invoice list module fields
    private Label _lblInvoicesTotalFacturas = null!;
    private Label _lblInvoicesTotalFacturado = null!;
    private Label _lblInvoicesSaldoPendiente = null!;
    private Label _lblInvoicesFacturasPagadas = null!;
    private TextBox _txtSearchInvoices = null!;
    private ComboBox _cmbEstadoFilter = null!;

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
    private Label _lblPaymentSummaryInvoice = null!;
    private Label _lblPaymentSummaryDate = null!;
    private TextBox _txtSearchPayments = null!;
    private Label _lblPaymentsTotalPagos = null!;
    private long? _pendingPaymentInvoiceId;
    private Label _lblPaymentsMontoTotal = null!;
    private Label _lblPaymentsClientes = null!;
    private Label _lblPaymentsFacturas = null!;

    // Cartera module fields
    private Label _lblCarteraClientes = null!;
    private Label _lblCarteraFacturas = null!;
    private Label _lblCarteraTotalCobrar = null!;
    private Label _lblCarteraVencido = null!;
    private DataGridView _gridFacturasPendientes = null!;
    private DataGridView _gridClientesSaldo = null!;
    private DataGridView _gridEdadSaldos = null!;
    private TextBox _txtSearchCartera = null!;
    private DataGridView _gridClientesSaldoFavor = null!;
    private Label _lblCarteraSaldoFavor = null!;

    // Balance module fields
    private Label _lblBalanceFacturado = null!;
    private Label _lblBalanceRecaudado = null!;
    private Label _lblBalanceCuentasPorCobrar = null!;
    private Label _lblBalanceNeto = null!;
    private DataGridView _gridBalanceMensual = null!;
    private DataGridView _gridBalanceFacturas = null!;
    private DataGridView _gridBalancePagos = null!;
    private Label _lblBalanceDetalleTitulo = null!;
    private Label _lblBalanceFacturasTitulo = null!;
    private Label _lblBalancePagosTitulo = null!;
    private Label _lblBalanceInfo = null!;
    private DateTimePicker _dtpBalanceFecha = null!;
    private readonly Dictionary<BalancePeriodo, Button> _balancePeriodButtons = [];
    private BalancePeriodo _balancePeriodoSeleccionado = BalancePeriodo.Mensual;
    private DateTime? _balanceFechaEspecifica;

    // Inventario module fields
    private Label _lblInventarioTotalProductos = null!;
    private Label _lblInventarioValorTotal = null!;
    private Label _lblInventarioStockBajo = null!;
    private Label _lblInventarioAgotados = null!;
    private DataGridView _gridProductosInventario = null!;
    private DataGridView _gridMovimientos = null!;
    private DataGridView _gridStockBajo = null!;
    private TextBox _txtSearchInventario = null!;
    private Label _lblMovimientosTitulo = null!;

    // Configuración module fields
    private Panel _configuracionView = null!;
    private Panel _configContentPanel = null!;
    private FlowLayoutPanel _configMenu = null!;
    private readonly Dictionary<string, Button> _configMenuButtons = [];

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
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 98));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
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

        var versionLabel = new Label
        {
            Text = AppVersion.DisplayVersion,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(100, 140, 106),
            Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 4),
            BackColor = Color.FromArgb(13, 48, 22)
        };

        sidebarLayout.Controls.Add(logoPanel, 0, 0);
        sidebarLayout.Controls.Add(_sidebarMenu, 0, 1);
        sidebarLayout.Controls.Add(versionLabel, 0, 2);

        AddMenuSection("PRINCIPAL");
        AddMenuButton(_sidebarMenu, "Dashboard", () => ShowModule("Dashboard"));
        AddMenuButton(_sidebarMenu, "Clientes", () => ShowModule("Clientes"));
        AddMenuButton(_sidebarMenu, "Facturas", () => ShowModule("Facturas"));
        AddMenuButton(_sidebarMenu, "Pagos", () => ShowModule("Pagos"));
        AddMenuButton(_sidebarMenu, "Cartera", () => ShowModule("Cartera"));
        AddMenuButton(_sidebarMenu, "Balance", () => ShowModule("Balance"));
        AddMenuButton(_sidebarMenu, "Inventario", () => ShowModule("Inventario"));

        AddMenuSection("SISTEMA", 18);
        AddMenuButton(_sidebarMenu, "Configuración", () => ShowModule("Configuración"));

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

        _btnHeaderSecondaryAction = new Button
        {
            Text = "⟳ Movimiento",
            Width = 130,
            Height = 38,
            Dock = DockStyle.Right,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 45),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Visible = false,
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnHeaderSecondaryAction.FlatAppearance.BorderSize = 1;
        _btnHeaderSecondaryAction.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 210);
        _btnHeaderSecondaryAction.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 248, 244);
        _btnHeaderSecondaryAction.FlatAppearance.MouseDownBackColor = Color.FromArgb(238, 242, 235);
        _btnHeaderSecondaryAction.Padding = new Padding(6, 0, 6, 0);
        _btnHeaderSecondaryAction.Click += (_, _) => _headerSecondaryAction?.Invoke();

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
        headerPanel.Controls.Add(_btnHeaderSecondaryAction);
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
        _configuracionView = BuildConfiguracionView();
    }

    private Panel BuildInvoicesListView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = new Padding(0, 72, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        view.Controls.Add(layout);

        // Summary cards
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

        _lblInvoicesTotalFacturas = CreateCard(cardsPanel, 0, "Total Facturas", out _);
        _lblInvoicesTotalFacturado = CreateCard(cardsPanel, 1, "Total Facturado", out _);
        _lblInvoicesSaldoPendiente = CreateCard(cardsPanel, 2, "Saldo Pendiente", out _);
        _lblInvoicesFacturasPagadas = CreateCard(cardsPanel, 3, "Facturas Pagadas", out _);

        layout.Controls.Add(cardsPanel, 0, 0);

        // Search bar
        var searchCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 0, 8)
        };

        var searchRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));

        _txtSearchInvoices = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar por número de factura o cliente...",
            Font = new Font("Segoe UI", 10)
        };
        _txtSearchInvoices.TextChanged += (_, _) => LoadInvoicesList();

        _cmbEstadoFilter = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        _cmbEstadoFilter.Items.AddRange(["Todos los estados", "Enviada", "Pagada", "Parcial", "Vencida", "Anulada"]);
        _cmbEstadoFilter.SelectedIndex = 0;
        _cmbEstadoFilter.SelectedIndexChanged += (_, _) => LoadInvoicesList();

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
        btnClearSearch.Click += (_, _) => { _txtSearchInvoices.Clear(); _cmbEstadoFilter.SelectedIndex = 0; _txtSearchInvoices.Focus(); };

        searchRow.Controls.Add(_txtSearchInvoices, 0, 0);
        searchRow.Controls.Add(_cmbEstadoFilter, 1, 0);
        searchRow.Controls.Add(btnClearSearch, 2, 0);
        searchCard.Controls.Add(searchRow);

        layout.Controls.Add(searchCard, 0, 1);

        // Grid card
        var tableCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var gridTitle = new Label
        {
            Text = "📋 Listado de Facturas",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        tableCard.Controls.Add(gridTitle);

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
        _gridInvoices.CellMouseClick += OnInvoiceListCellMouseClick;
        _gridInvoices.CellPainting += OnInvoiceListCellPainting;
        AddGridWithTopMargin(tableCard, _gridInvoices, 20);

        layout.Controls.Add(tableCard, 0, 2);
        return view;
    }

    private Panel BuildDashboardView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 256));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 256));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        view.Controls.Add(mainLayout);

        // Row 0: Summary cards
        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
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
        mainLayout.Controls.Add(cards, 0, 0);

        // Row 1: Period navigator
        var periodCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(8, 4, 8, 4),
            Margin = new Padding(4, 2, 4, 2)
        };
        var periodFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        var periodLabel = new Label
        {
            Text = "📊 Período:",
            Width = 82,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 55, 45),
            Margin = new Padding(4, 0, 6, 0)
        };
        periodFlow.Controls.Add(periodLabel);
        AddDashboardPeriodButton(periodFlow, "7 Días");
        AddDashboardPeriodButton(periodFlow, "6 Meses");
        AddDashboardPeriodButton(periodFlow, "Anual");
        periodCard.Controls.Add(periodFlow);
        SetDashboardPeriodSelection("6 Meses");
        mainLayout.Controls.Add(periodCard, 0, 1);

        // Row 2: Bar chart (Facturado vs Recaudado) + Donut (Estado Facturas)
        var chartsRow1 = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(4, 2, 4, 2)
        };
        chartsRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        chartsRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var barChartCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(10),
            Margin = new Padding(0, 0, 4, 0)
        };
        var barChartTitle = new Label
        {
            Text = "📊 Facturado vs Recaudado",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        _chartFacturacion = new BarChartPanel { Dock = DockStyle.Fill };
        barChartCard.Controls.Add(_chartFacturacion);
        barChartCard.Controls.Add(barChartTitle);

        var donutStatusCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(10),
            Margin = new Padding(4, 0, 0, 0)
        };
        var donutStatusTitle = new Label
        {
            Text = "📈 Estado de Facturas",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        _chartEstadoFacturas = new DonutChartPanel { Dock = DockStyle.Fill };
        donutStatusCard.Controls.Add(_chartEstadoFacturas);
        donutStatusCard.Controls.Add(donutStatusTitle);

        chartsRow1.Controls.Add(barChartCard, 0, 0);
        chartsRow1.Controls.Add(donutStatusCard, 1, 0);
        mainLayout.Controls.Add(chartsRow1, 0, 2);

        // Row 3: Top Clientes (HBar) + Métodos de Pago (Donut)
        var chartsRow2 = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(4, 2, 4, 2)
        };
        chartsRow2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        chartsRow2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var topClientesCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(10),
            Margin = new Padding(0, 0, 4, 0)
        };
        var topClientesTitle = new Label
        {
            Text = "🏆 Top 5 Clientes",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        _chartTopClientes = new HorizontalBarChartPanel { Dock = DockStyle.Fill };
        topClientesCard.Controls.Add(_chartTopClientes);
        topClientesCard.Controls.Add(topClientesTitle);

        var metodosPagoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(10),
            Margin = new Padding(4, 0, 0, 0)
        };
        var metodosPagoTitle = new Label
        {
            Text = "💳 Recaudo por Método de Pago",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        _chartMetodosPago = new DonutChartPanel { Dock = DockStyle.Fill };
        metodosPagoCard.Controls.Add(_chartMetodosPago);
        metodosPagoCard.Controls.Add(metodosPagoTitle);

        chartsRow2.Controls.Add(topClientesCard, 0, 0);
        chartsRow2.Controls.Add(metodosPagoCard, 1, 0);
        mainLayout.Controls.Add(chartsRow2, 0, 3);

        // Row 4: Grids (Facturas Recientes + Stock Bajo)
        var gridsRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(4, 2, 4, 2),
            MinimumSize = new Size(0, 240)
        };
        gridsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        gridsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var recentPanel = CreateDataPanel("📄 Facturas Recientes", out _gridRecentInvoices);
        var stockPanel = CreateDataPanel("⚠️ Alertas de Stock Mínimo", out _gridLowStock);
        gridsRow.Controls.Add(recentPanel, 0, 0);
        gridsRow.Controls.Add(stockPanel, 1, 0);
        mainLayout.Controls.Add(gridsRow, 0, 4);

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
        AddGridWithTopMargin(tableCard, _gridCustomers, 20);

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
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        view.Controls.Add(mainLayout);

        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(12, 12, 6, 12),
            Padding = Padding.Empty
        };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 188));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 168));

        var infoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var infoLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        infoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        infoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var infoTitle = new Label
        {
            Text = "📋 Información General",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };

        var infoGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        infoGrid.Controls.Add(new Label { Text = "Cliente *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 0);
        _cmbInvoiceCustomer = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        infoGrid.Controls.Add(_cmbInvoiceCustomer, 0, 1);

        infoGrid.Controls.Add(new Label { Text = "Método de Pago *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 1, 0);
        _cmbInvoicePaymentMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        infoGrid.Controls.Add(_cmbInvoicePaymentMethod, 1, 1);

        infoGrid.Controls.Add(new Label { Text = "Fecha *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 2);
        _dtpInvoiceDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        _dtpInvoiceDate.ValueChanged += (_, _) => UpdateInvoiceSummaryDate();
        infoGrid.Controls.Add(_dtpInvoiceDate, 0, 3);

        var helperLabel = new Label
        {
            Text = "La fecha se reflejará en el resumen y en la impresión.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.DimGray
        };
        infoGrid.Controls.Add(helperLabel, 1, 3);

        infoLayout.Controls.Add(infoTitle, 0, 0);
        infoLayout.Controls.Add(infoGrid, 0, 1);
        infoCard.Controls.Add(infoLayout);
        leftLayout.Controls.Add(infoCard, 0, 0);

        var productsCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var productsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        productsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        productsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        productsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        productsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var productsTitle = new Label
        {
            Text = "📦 Productos de la Factura",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        productsLayout.Controls.Add(productsTitle, 0, 0);

        var productsTopBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 4),
            Padding = Padding.Empty
        };
        productsTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        productsTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

        _cmbItemProduct = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbItemProduct.SelectedIndexChanged += (_, _) => OnProductChanged();
        productsTopBar.Controls.Add(_cmbItemProduct, 0, 0);

        var btnAddItem = new Button
        {
            Text = "+ Agregar",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnAddItem.FlatAppearance.BorderSize = 0;
        btnAddItem.Click += (_, _) => AddInvoiceItem();
        productsTopBar.Controls.Add(btnAddItem, 1, 0);
        productsLayout.Controls.Add(productsTopBar, 0, 1);

        var productEditForm = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Margin = new Padding(0, 4, 0, 0),
            Padding = Padding.Empty
        };
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        productEditForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        productEditForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        productEditForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

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

        productsLayout.Controls.Add(productEditForm, 0, 2);

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

        productsLayout.Controls.Add(CreateGridHost(_gridInvoiceItems, 12), 0, 3);
        productsCard.Controls.Add(productsLayout);

        leftLayout.Controls.Add(productsCard, 0, 1);

        var notesCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };

        var notesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        notesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        notesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var notesTitle = new Label
        {
            Text = "📝 Notas",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        notesLayout.Controls.Add(notesTitle, 0, 0);

        _txtInvoiceNotes = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            Margin = new Padding(0, 8, 0, 0),
            ScrollBars = ScrollBars.Vertical
        };
        notesLayout.Controls.Add(_txtInvoiceNotes, 0, 1);
        notesCard.Controls.Add(notesLayout);

        leftLayout.Controls.Add(notesCard, 0, 2);
        mainLayout.Controls.Add(leftLayout, 0, 0);

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

        var summaryLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 168));

        var summaryTitle = new Label
        {
            Text = "Resumen",
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Margin = new Padding(0, 0, 0, 16)
        };
        summaryLayout.Controls.Add(summaryTitle, 0, 0);

        var summaryContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 12, 0, 0)
        };

        var summaryGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Height = 216
        };
        summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        summaryGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));

        summaryGrid.Controls.Add(new Label { Text = "📅 Fecha de Factura", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _lblInvoiceSummaryDate = new Label { Text = DateTime.Today.ToString("d 'de' MMMM 'de' yyyy"), Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblInvoiceSummaryDate, 1, 0);

        summaryGrid.Controls.Add(new Label { Text = "Items cargados", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        _lblInvoiceItemsCount = new Label { Text = "0 productos", Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.DimGray };
        summaryGrid.Controls.Add(_lblInvoiceItemsCount, 1, 1);

        summaryGrid.Controls.Add(new Label { Text = "Subtotal", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        _lblInvoiceSubtotal = new Label { Text = "$ 0", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblInvoiceSubtotal, 1, 2);

        var totalLblRow = new Label { Text = "Total", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(11, 52, 22), Padding = new Padding(0, 12, 0, 0) };
        summaryGrid.Controls.Add(totalLblRow, 0, 3);
        _lblInvoiceTotal = new Label { Text = "$ 0", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(11, 52, 22), Padding = new Padding(0, 12, 0, 0) };
        summaryGrid.Controls.Add(_lblInvoiceTotal, 1, 3);

        summaryContentPanel.Controls.Add(summaryGrid);
        summaryLayout.Controls.Add(summaryContentPanel, 0, 1);

        var buttonPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };
        buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        var btnSaveInvoice = new Button
        {
            Text = "✓ Crear y Enviar",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(33, 99, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6),
            Cursor = Cursors.Hand
        };
        btnSaveInvoice.FlatAppearance.BorderSize = 0;
        btnSaveInvoice.Click += (_, _) => SaveInvoice();
        buttonPanel.Controls.Add(btnSaveInvoice, 0, 0);

        var btnPrintInvoice = new Button
        {
            Text = "🖨 Imprimir",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 45),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6),
            Cursor = Cursors.Hand
        };
        btnPrintInvoice.FlatAppearance.BorderSize = 1;
        btnPrintInvoice.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 210);
        btnPrintInvoice.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 248, 244);
        btnPrintInvoice.Click += (_, _) => PrintCurrentInvoiceDraft();
        buttonPanel.Controls.Add(btnPrintInvoice, 0, 1);

        var btnDraft = new Button
        {
            Text = "🧹 Limpiar formulario",
            Dock = DockStyle.Fill,
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
        buttonPanel.Controls.Add(btnDraft, 0, 2);

        summaryLayout.Controls.Add(buttonPanel, 0, 2);
        summaryCard.Controls.Add(summaryLayout);
        rightPanel.Controls.Add(summaryCard);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        return view;
    }

    private void UpdateInvoiceSummaryDate()
    {
        if (_lblInvoiceSummaryDate is null || _dtpInvoiceDate is null)
        {
            return;
        }

        _lblInvoiceSummaryDate.Text = _dtpInvoiceDate.Value.ToString("d 'de' MMMM 'de' yyyy");
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

    private Panel BuildConfiguracionView()
    {
        var view = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 72, 0, 0)
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        view.Controls.Add(mainLayout);

        // Panel izquierdo: menú de opciones
        var menuCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14, 16, 14, 14),
            Margin = new Padding(0, 0, 10, 0)
        };

        var menuTitle = new Label
        {
            Text = "⚙ Opciones",
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Padding = new Padding(6, 6, 0, 0)
        };

        _configMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 10, 0, 0)
        };

        menuCard.Controls.Add(_configMenu);
        menuCard.Controls.Add(menuTitle);

        // Panel derecho: contenido de la opción seleccionada
        _configContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = Padding.Empty
        };

        var contentCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(24),
            Margin = new Padding(10, 0, 0, 0)
        };

        var contentPlaceholder = new Label
        {
            Text = "Selecciona una opción del menú",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(160, 160, 160),
            Font = new Font("Segoe UI", 12, FontStyle.Italic)
        };
        _configContentPanel.Controls.Add(contentPlaceholder);
        contentCard.Controls.Add(_configContentPanel);

        mainLayout.Controls.Add(menuCard, 0, 0);
        mainLayout.Controls.Add(contentCard, 1, 0);

        // Agregar opciones del menú de configuración
        AddConfigMenuOption("🗄 Gestor de Base de Datos", ShowConfigGestorBaseDeDatos);
        AddConfigMenuOption("📂 Ubicación BD", ShowConfigBaseDeDatos);

        return view;
    }

    private void AddConfigMenuOption(string text, Action onClick)
    {
        var button = new Button
        {
            Text = text,
            Width = 220,
            Height = 46,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 45),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0),
            Margin = new Padding(2, 3, 2, 3),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(234, 242, 230);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(214, 228, 210);

        button.Click += (_, _) =>
        {
            SetConfigMenuSelection(text);
            onClick();
        };

        _configMenuButtons[text] = button;
        _configMenu.Controls.Add(button);

        _configMenu.SizeChanged += (_, _) =>
        {
            var w = Math.Max(160, _configMenu.ClientSize.Width - 10);
            foreach (Control c in _configMenu.Controls)
            {
                if (c is Button b) b.Width = w;
            }
        };
    }

    private void SetConfigMenuSelection(string selected)
    {
        foreach (var item in _configMenuButtons)
        {
            item.Value.BackColor = item.Key == selected
                ? Color.FromArgb(224, 238, 220)
                : Color.White;
            item.Value.Font = item.Key == selected
                ? new Font("Segoe UI", 9.5f, FontStyle.Bold)
                : new Font("Segoe UI", 9.5f, FontStyle.Regular);
        }
    }

    private void ShowConfigContent(Control content)
    {
        _configContentPanel.Controls.Clear();
        content.Dock = DockStyle.Fill;
        _configContentPanel.Controls.Add(content);
    }

    private void LoadConfiguracion()
    {
        // Seleccionar la primera opción por defecto
        if (_configMenuButtons.Count > 0)
        {
            var firstKey = _configMenuButtons.Keys.First();
            SetConfigMenuSelection(firstKey);
            _configMenuButtons[firstKey].PerformClick();
        }
    }

    private void ShowConfigBaseDeDatos()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        var title = new Label
        {
            Text = "📂 Base de datos",
            Dock = DockStyle.Top,
            Height = 42,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Padding = new Padding(0, 2, 0, 0)
        };

        var description = new Label
        {
            Text = "Ubicación del archivo de base de datos SQLite del sistema.",
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.DimGray,
            Padding = new Padding(0, 4, 0, 10)
        };

        var pathLabel = new Label
        {
            Text = $"Ruta: {AppDatabase.DatabasePath}",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(80, 80, 80),
            Padding = new Padding(0, 6, 0, 6)
        };

        var btnOpen = new Button
        {
            Text = "📁 Abrir carpeta de base de datos",
            Width = 280,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 12, 0, 0)
        };
        btnOpen.FlatAppearance.BorderSize = 0;
        btnOpen.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 96, 23);
        btnOpen.Click += (_, _) => OpenDatabaseFolder();

        var buttonContainer = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 64,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 0)
        };
        buttonContainer.Controls.Add(btnOpen);

        panel.Controls.Add(buttonContainer);
        panel.Controls.Add(pathLabel);
        panel.Controls.Add(description);
        panel.Controls.Add(title);

        ShowConfigContent(panel);
    }

    private void ShowConfigGestorBaseDeDatos()
    {
        var pinPanel = new Panel { Dock = DockStyle.Fill };

        var centerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1
        };
        centerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        centerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var box = new Panel
        {
            Width = 400,
            Height = 340,
            Anchor = AnchorStyles.None
        };

        var lockIcon = new Label
        {
            Text = "🔒",
            Dock = DockStyle.Top,
            Height = 60,
            Font = new Font("Segoe UI", 32),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 0, 0, 4)
        };

        var pinTitle = new Label
        {
            Text = "Acceso protegido",
            Dock = DockStyle.Top,
            Height = 38,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var pinSubtitle = new Label
        {
            Text = "Ingresa el PIN para acceder al Gestor de Base de Datos",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 0, 0, 6)
        };

        var txtPin = new TextBox
        {
            Width = 220,
            Height = 40,
            Font = new Font("Segoe UI", 18),
            TextAlign = HorizontalAlignment.Center,
            PasswordChar = '●',
            MaxLength = 10,
            Anchor = AnchorStyles.Top
        };

        var pinInputRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 56,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 0),
            WrapContents = false
        };
        pinInputRow.Controls.Add(txtPin);
        pinInputRow.SizeChanged += (_, _) =>
        {
            var left = Math.Max(0, (pinInputRow.ClientSize.Width - txtPin.Width) / 2);
            txtPin.Margin = new Padding(left, 0, 0, 0);
        };

        var lblError = new Label
        {
            Text = "",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(200, 40, 40),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnAcceder = new Button
        {
            Text = "Acceder",
            Width = 220,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top
        };
        btnAcceder.FlatAppearance.BorderSize = 0;
        btnAcceder.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 96, 23);

        var btnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 6, 0, 0),
            WrapContents = false
        };
        btnRow.Controls.Add(btnAcceder);
        btnRow.SizeChanged += (_, _) =>
        {
            var left = Math.Max(0, (btnRow.ClientSize.Width - btnAcceder.Width) / 2);
            btnAcceder.Margin = new Padding(left, 0, 0, 0);
        };

        void TryAccess()
        {
            if (txtPin.Text == "0000")
            {
                ShowGestorBaseDeDatosPanel();
            }
            else
            {
                lblError.Text = "PIN incorrecto. Intenta de nuevo.";
                txtPin.Clear();
                txtPin.Focus();
            }
        }

        btnAcceder.Click += (_, _) => TryAccess();
        txtPin.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TryAccess();
            }
        };

        box.Controls.Add(btnRow);
        box.Controls.Add(lblError);
        box.Controls.Add(pinInputRow);
        box.Controls.Add(pinSubtitle);
        box.Controls.Add(pinTitle);
        box.Controls.Add(lockIcon);

        centerLayout.Controls.Add(box, 0, 0);
        pinPanel.Controls.Add(centerLayout);

        ShowConfigContent(pinPanel);
        txtPin.Focus();
    }

    private void ShowGestorBaseDeDatosPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var title = new Label
        {
            Text = "🗄 Gestor de Base de Datos",
            Dock = DockStyle.Top,
            Height = 42,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Padding = new Padding(0, 2, 0, 0)
        };

        var description = new Label
        {
            Text = "Herramientas de administración de la base de datos del sistema.",
            Dock = DockStyle.Top,
            Height = 44,
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.DimGray,
            Padding = new Padding(0, 4, 0, 14)
        };

        var actionsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };

        actionsFlow.Controls.Add(CreateDbActionCard(
            "📁 Ver Base de Datos",
            "Abrir la carpeta donde se encuentra el archivo de la base de datos.",
            Color.FromArgb(45, 111, 26),
            () => OpenDatabaseFolder()));

        actionsFlow.Controls.Add(CreateDbActionCard(
            "📤 Exportar Base de Datos",
            "Crear una copia de respaldo (.db) en la ubicación que elijas.",
            Color.FromArgb(30, 100, 160),
            ExportDatabase));

        actionsFlow.Controls.Add(CreateDbActionCard(
            "📥 Importar Base de Datos",
            "Restaurar la base de datos desde un archivo de respaldo (.db).",
            Color.FromArgb(140, 100, 20),
            ImportDatabase));

        actionsFlow.Controls.Add(CreateDbActionCard(
            "🧹 Limpiar Toda la Base de Datos",
            "Eliminar TODOS los registros de todas las tablas. Esta acción es irreversible.",
            Color.FromArgb(180, 40, 40),
            CleanEntireDatabase));

        actionsFlow.Controls.Add(CreateDbActionCard(
            "🗑 Limpiar Tablas Específicas",
            "Seleccionar qué tablas limpiar individualmente.",
            Color.FromArgb(160, 80, 20),
            ShowCleanSpecificTables));

        actionsFlow.SizeChanged += (_, _) =>
        {
            var w = Math.Max(300, actionsFlow.ClientSize.Width - 20);
            foreach (Control c in actionsFlow.Controls)
            {
                c.Width = w;
            }
        };

        panel.Controls.Add(actionsFlow);
        panel.Controls.Add(description);
        panel.Controls.Add(title);

        ShowConfigContent(panel);
    }

    private static Panel CreateDbActionCard(string title, string description, Color accentColor, Action onClick)
    {
        var card = new Panel
        {
            Width = 500,
            Height = 82,
            Margin = new Padding(0, 0, 0, 10),
            Cursor = Cursors.Hand
        };

        var accent = new Panel
        {
            Dock = DockStyle.Left,
            Width = 6,
            BackColor = accentColor
        };

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(250, 252, 248),
            Padding = new Padding(18, 14, 18, 14)
        };

        var lblTitle = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 26,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };

        var lblDesc = new Label
        {
            Text = description,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray,
            Padding = new Padding(0, 2, 0, 0)
        };

        content.Controls.Add(lblDesc);
        content.Controls.Add(lblTitle);

        card.Controls.Add(content);
        card.Controls.Add(accent);

        void ApplyHover(Control c)
        {
            c.MouseEnter += (_, _) => content.BackColor = Color.FromArgb(238, 245, 233);
            c.MouseLeave += (_, _) => content.BackColor = Color.FromArgb(250, 252, 248);
            c.Click += (_, _) => onClick();
        }

        ApplyHover(card);
        ApplyHover(content);
        ApplyHover(lblTitle);
        ApplyHover(lblDesc);

        return card;
    }

    private void ExportDatabase()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Exportar Base de Datos",
            Filter = "SQLite Database (*.db)|*.db|Todos los archivos (*.*)|*.*",
            FileName = $"aceitespro_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
            DefaultExt = "db"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            File.Copy(AppDatabase.DatabasePath, dialog.FileName, overwrite: true);
            MessageBox.Show(
                $"Base de datos exportada exitosamente.\n\nUbicación:\n{dialog.FileName}",
                "Exportar BD",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al exportar la base de datos:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ImportDatabase()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Importar Base de Datos",
            Filter = "SQLite Database (*.db)|*.db|Todos los archivos (*.*)|*.*"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var confirm = MessageBox.Show(
            "⚠ Esta acción reemplazará TODA la base de datos actual.\n\n" +
            "Se recomienda exportar un respaldo antes de continuar.\n\n" +
            "¿Deseas continuar con la importación?",
            "Confirmar Importación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            File.Copy(dialog.FileName, AppDatabase.DatabasePath, overwrite: true);
            MessageBox.Show(
                "Base de datos importada exitosamente.\n\nLa aplicación se reiniciará para aplicar los cambios.",
                "Importar BD",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Application.Restart();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al importar la base de datos:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CleanEntireDatabase()
    {
        var confirm = MessageBox.Show(
            "⚠ ADVERTENCIA: Esta acción eliminará TODOS los registros de la base de datos.\n\n" +
            "Esta operación es IRREVERSIBLE.\n\n" +
            "¿Estás completamente seguro?",
            "Limpiar Base de Datos",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        var confirmFinal = MessageBox.Show(
            "Esta es la ÚLTIMA confirmación.\n\n" +
            "Se eliminarán todos los datos de:\n" +
            "• Movimientos de inventario\n• Items de factura\n• Pagos\n• Facturas\n• Clientes\n• Productos\n• Métodos de pago\n\n" +
            "¿Continuar?",
            "Confirmación Final",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Stop);

        if (confirmFinal != DialogResult.Yes) return;

        try
        {
            using var connection = AppDatabase.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            string[] tablesInOrder =
            [
                "MovimientoInventarioExt",
                "ItemFactura",
                "Pago",
                "Factura",
                "Cliente",
                "ProductoExt",
                "MetodoPago"
            ];

            foreach (var table in tablesInOrder)
            {
                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = $"DELETE FROM {table};";
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();

            MessageBox.Show(
                "Base de datos limpiada exitosamente.\nTodas las tablas han sido vaciadas.",
                "Limpiar BD",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ShowGestorBaseDeDatosPanel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al limpiar la base de datos:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ShowCleanSpecificTables()
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var title = new Label
        {
            Text = "🗑 Limpiar Tablas Específicas",
            Dock = DockStyle.Top,
            Height = 42,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Padding = new Padding(0, 2, 0, 0)
        };

        var description = new Label
        {
            Text = "Selecciona las tablas que deseas limpiar. Se respetará el orden de dependencias.",
            Dock = DockStyle.Top,
            Height = 44,
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.DimGray,
            Padding = new Padding(0, 4, 0, 14)
        };

        var tablesInfo = new (string Table, string DisplayName, string Description, string[] DependsOn)[]
        {
            ("MovimientoInventarioExt", "Movimientos de Inventario", "Entradas, salidas y ajustes de stock", []),
            ("ItemFactura", "Items de Factura", "Líneas de detalle de facturas", []),
            ("Pago", "Pagos", "Todos los pagos registrados", []),
            ("Factura", "Facturas", "Facturas y remisiones (elimina items y pagos asociados)", ["ItemFactura", "Pago"]),
            ("Cliente", "Clientes", "Maestro de clientes (elimina facturas asociadas)", ["Factura", "ItemFactura", "Pago"]),
            ("ProductoExt", "Productos", "Catálogo de productos (elimina movimientos asociados)", ["MovimientoInventarioExt", "ItemFactura"]),
            ("MetodoPago", "Métodos de Pago", "Efectivo, transferencia, crédito, etc.", []),
        };

        var checkboxes = new Dictionary<string, CheckBox>();

        var tablesFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 8)
        };

        foreach (var info in tablesInfo)
        {
            var row = new Panel
            {
                Width = 500,
                Height = 60,
                Margin = new Padding(0, 0, 0, 6),
                BackColor = Color.FromArgb(250, 252, 248),
                Padding = new Padding(14, 10, 14, 10)
            };

            var chk = new CheckBox
            {
                Text = $"  {info.DisplayName}",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 45)
            };

            var lbl = new Label
            {
                Text = info.Description,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Padding = new Padding(22, 3, 0, 0)
            };

            row.Controls.Add(lbl);
            row.Controls.Add(chk);
            tablesFlow.Controls.Add(row);
            checkboxes[info.Table] = chk;
        }

        tablesFlow.SizeChanged += (_, _) =>
        {
            var w = Math.Max(300, tablesFlow.ClientSize.Width - 20);
            foreach (Control c in tablesFlow.Controls)
            {
                c.Width = w;
            }
        };

        var buttonsRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 0)
        };

        var btnBack = new Button
        {
            Text = "← Volver",
            Width = 130,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 45),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnBack.FlatAppearance.BorderSize = 1;
        btnBack.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 210);
        btnBack.Click += (_, _) => ShowGestorBaseDeDatosPanel();

        var btnClean = new Button
        {
            Text = "🧹 Limpiar Seleccionadas",
            Width = 240,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 40, 40),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnClean.FlatAppearance.BorderSize = 0;
        btnClean.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 30, 30);

        btnClean.Click += (_, _) =>
        {
            var selected = checkboxes
                .Where(kv => kv.Value.Checked)
                .Select(kv => kv.Key)
                .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Selecciona al menos una tabla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tablesToClean = new List<string>();
            foreach (var info in tablesInfo)
            {
                if (selected.Contains(info.Table))
                {
                    foreach (var dep in info.DependsOn)
                    {
                        if (!tablesToClean.Contains(dep))
                            tablesToClean.Add(dep);
                    }
                    if (!tablesToClean.Contains(info.Table))
                        tablesToClean.Add(info.Table);
                }
            }

            var displayNames = tablesToClean
                .Select(t => tablesInfo.First(i => i.Table == t).DisplayName);

            var confirm = MessageBox.Show(
                $"Se limpiarán las siguientes tablas:\n\n• {string.Join("\n• ", displayNames)}\n\n" +
                "Esta acción es IRREVERSIBLE. ¿Continuar?",
                "Confirmar Limpieza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var connection = AppDatabase.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                foreach (var table in tablesToClean)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = $"DELETE FROM {table};";
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();

                MessageBox.Show(
                    $"Tablas limpiadas exitosamente:\n• {string.Join("\n• ", displayNames)}",
                    "Limpieza Completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al limpiar tablas:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };

        buttonsRow.Controls.Add(btnBack);
        buttonsRow.Controls.Add(btnClean);

        panel.Controls.Add(buttonsRow);
        panel.Controls.Add(tablesFlow);
        panel.Controls.Add(description);
        panel.Controls.Add(title);

        ShowConfigContent(panel);
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
        _btnHeaderSecondaryAction.Visible = false;
        _headerAction = null;
        _headerSecondaryAction = null;

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
            _btnHeaderAction.Text = "🖨 Imprimir";
            _btnHeaderAction.Visible = true;
            _headerAction = PrintCartera;
            ShowAnimatedView(_carteraView, LoadCartera);
            return;
        }

        if (moduleName == "Balance")
        {
            _headerTitle.Text = "Balance";
            _headerSubtitle.Text = "Análisis financiero y rentabilidad";
            _btnHeaderAction.Text = "🖨 Imprimir";
            _btnHeaderAction.Visible = true;
            _headerAction = PrintBalance;
            ShowAnimatedView(_balanceView, LoadBalance);
            return;
        }

        if (moduleName == "Inventario")
        {
            _headerTitle.Text = "Inventario";
            _headerSubtitle.Text = "Gestión de productos, stock y movimientos";

            _btnHeaderSecondaryAction.Text = "⟳ Movimiento";
            _btnHeaderSecondaryAction.Visible = true;
            _headerSecondaryAction = OpenInventoryMovementDialog;

            _btnHeaderAction.Text = "+ Nuevo Producto";
            _btnHeaderAction.Visible = true;
            _headerAction = OpenNewProductDialog;

            ShowAnimatedView(_inventarioView, LoadInventario);
            return;
        }

        if (moduleName == "Configuración")
        {
            _headerTitle.Text = "Configuración";
            _headerSubtitle.Text = "Opciones y ajustes del sistema";
            ShowAnimatedView(_configuracionView, LoadConfiguracion);
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
        _contentHost.Controls.Clear();
        targetView.Dock = DockStyle.Fill;
        _contentHost.Controls.Add(targetView);
        loadAction();
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
            Margin = new Padding(6),
            BackColor = Color.White,
            Padding = new Padding(12, 50, 12, 4),
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Height = 20
        };

        var valueLabel = new Label
        {
            Text = "$ 0",
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(11, 52, 22),
            Height = 46
        };

        secondary = new Label
        {
            Text = string.Empty,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = Color.DimGray,
            Height = 18
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

    private static Panel CreateGridHost(Control grid, int topMargin = 20)
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, topMargin, 0, 0),
            Margin = Padding.Empty
        };

        grid.Dock = DockStyle.Fill;
        host.Controls.Add(grid);
        return host;
    }

    private static void AddGridWithTopMargin(Control container, DataGridView grid, int topMargin = 20)
    {
        container.Controls.Add(CreateGridHost(grid, topMargin));
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
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.White
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

        var gridHost = CreateGridHost(grid, 20);

        panel.Controls.Add(gridHost);
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

        LoadDashboardBarChart();
        LoadDashboardStatusChart();
        LoadDashboardTopClientes();
        LoadDashboardMetodosPago();
    }

    private void LoadDashboardBarChart()
    {
        var data = _dashboardChartPeriodo switch
        {
            "7 Días" => _balanceRepository.GetBalanceDetalle(BalancePeriodo.Diario),
            "Anual" => _balanceRepository.GetBalanceDetalle(BalancePeriodo.Anual),
            _ => _balanceRepository.GetBalanceMensual()
        };

        var labels = data.Select(d => ShortenPeriodLabel(d.MesNombre)).ToList();

        _chartFacturacion.SetData(
            labels,
            new BarChartSeries
            {
                Name = "Facturado",
                Color = Color.FromArgb(45, 111, 26),
                Values = data.Select(d => d.TotalFacturado).ToList()
            },
            new BarChartSeries
            {
                Name = "Recaudado",
                Color = Color.FromArgb(62, 146, 137),
                Values = data.Select(d => d.TotalRecaudado).ToList()
            });
    }

    private void LoadDashboardStatusChart()
    {
        var statuses = _dashboardService.GetInvoiceStatusDistribution();
        var slices = statuses.Select(s => new DonutSlice
        {
            Label = s.Estado,
            Value = s.Cantidad,
            Color = s.Estado switch
            {
                "Pagada" => Color.FromArgb(45, 111, 26),
                "Pendiente" => Color.FromArgb(108, 142, 191),
                "Enviada" => Color.FromArgb(59, 130, 185),
                "Parcial" => Color.FromArgb(231, 168, 57),
                "Vencida" => Color.FromArgb(205, 70, 49),
                "Anulada" => Color.FromArgb(160, 160, 160),
                _ => Color.FromArgb(120, 120, 120)
            }
        }).ToList();

        var total = statuses.Sum(s => s.Cantidad);
        _chartEstadoFacturas.SetData(slices, total.ToString("N0"), "Facturas");
    }

    private void LoadDashboardTopClientes()
    {
        var topClientes = _balanceRepository.GetTopClientes(5, BalancePeriodo.Total);
        var colors = new[]
        {
            Color.FromArgb(45, 111, 26),
            Color.FromArgb(62, 146, 137),
            Color.FromArgb(59, 130, 185),
            Color.FromArgb(231, 168, 57),
            Color.FromArgb(136, 84, 208)
        };

        _chartTopClientes.SetData(topClientes.Select((c, i) => new HBarItem
        {
            Label = c.Nombre,
            Value = c.TotalFacturado,
            Color = colors[i % colors.Length]
        }).ToList());
    }

    private void LoadDashboardMetodosPago()
    {
        var methods = _dashboardService.GetPaymentMethodDistribution();
        var colors = new[]
        {
            Color.FromArgb(45, 111, 26),
            Color.FromArgb(62, 146, 137),
            Color.FromArgb(231, 168, 57),
            Color.FromArgb(59, 130, 185),
            Color.FromArgb(205, 70, 49),
            Color.FromArgb(136, 84, 208)
        };

        var slices = methods.Select((m, i) => new DonutSlice
        {
            Label = m.MetodoPago,
            Value = m.Total,
            Color = colors[i % colors.Length]
        }).ToList();

        var total = methods.Sum(m => m.Total);
        _chartMetodosPago.SetData(slices, total > 0 ? total.ToString("C0") : "$ 0", "Recaudado");
    }

    private void AddDashboardPeriodButton(Control parent, string text)
    {
        var button = new Button
        {
            Text = text,
            Width = 90,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 45),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 6, 0),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 210);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 248, 244);

        button.Click += (_, _) =>
        {
            _dashboardChartPeriodo = text;
            SetDashboardPeriodSelection(text);
            LoadDashboardBarChart();
        };

        _dashboardPeriodButtons[text] = button;
        parent.Controls.Add(button);
    }

    private void SetDashboardPeriodSelection(string selected)
    {
        foreach (var (key, button) in _dashboardPeriodButtons)
        {
            var active = key == selected;
            button.BackColor = active ? Color.FromArgb(45, 111, 26) : Color.White;
            button.ForeColor = active ? Color.White : Color.FromArgb(45, 55, 45);
            button.FlatAppearance.BorderColor = active ? Color.FromArgb(45, 111, 26) : Color.FromArgb(214, 218, 210);
        }
    }

    private static string ShortenPeriodLabel(string label)
    {
        var parts = label.Split(' ');
        if (parts.Length == 2 && parts[0].Length > 3)
            return parts[0][..3] + " " + (parts[1].Length == 4 ? parts[1][2..] : parts[1]);
        return label;
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

        LoadInvoicesList();
        ClearInvoiceForm();
    }

    private void LoadInvoicesList()
    {
        var resumen = _invoiceRepository.GetListResumen();
        _lblInvoicesTotalFacturas.Text = resumen.TotalFacturas.ToString("N0");
        _lblInvoicesTotalFacturado.Text = resumen.TotalFacturado.ToString("C0");
        _lblInvoicesSaldoPendiente.Text = resumen.SaldoPendiente.ToString("C0");
        _lblInvoicesFacturasPagadas.Text = resumen.FacturasPagadas.ToString("N0");

        var search = _txtSearchInvoices.Text;
        var estado = _cmbEstadoFilter.SelectedItem?.ToString();
        var data = _invoiceRepository.GetRecentInvoices(100, search, estado);
        _gridInvoices.DataSource = data;

        if (_gridInvoices.Columns.Count > 0)
        {
            if (_gridInvoices.Columns["Id"] != null)
                _gridInvoices.Columns["Id"]!.Visible = false;

            if (_gridInvoices.Columns["Numero"] != null)
            {
                _gridInvoices.Columns["Numero"]!.HeaderText = "N° Factura";
                _gridInvoices.Columns["Numero"]!.Width = 110;
            }

            if (_gridInvoices.Columns["Fecha"] != null)
            {
                _gridInvoices.Columns["Fecha"]!.HeaderText = "Fecha";
                _gridInvoices.Columns["Fecha"]!.Width = 90;
            }

            if (_gridInvoices.Columns["Cliente"] != null)
            {
                _gridInvoices.Columns["Cliente"]!.HeaderText = "Cliente";
                _gridInvoices.Columns["Cliente"]!.MinimumWidth = 160;
            }

            if (_gridInvoices.Columns["Total"] != null)
            {
                _gridInvoices.Columns["Total"]!.HeaderText = "Total";
                _gridInvoices.Columns["Total"]!.DefaultCellStyle.Format = "C0";
                _gridInvoices.Columns["Total"]!.Width = 110;
            }

            if (_gridInvoices.Columns["Saldo"] != null)
                _gridInvoices.Columns["Saldo"]!.Visible = false;

            if (_gridInvoices.Columns["Estado"] != null)
            {
                _gridInvoices.Columns["Estado"]!.HeaderText = "Estado";
                _gridInvoices.Columns["Estado"]!.Width = 90;
            }

            if (_gridInvoices.Columns["EstadoSaldo"] != null)
            {
                _gridInvoices.Columns["EstadoSaldo"]!.HeaderText = "Estado Saldo";
                _gridInvoices.Columns["EstadoSaldo"]!.Width = 140;
            }

            EnsureInvoiceActionColumns();

            foreach (DataGridViewRow row in _gridInvoices.Rows)
            {
                if (_gridInvoices.Columns["Acciones"] != null)
                {
                    row.Cells["Acciones"].Value = string.Empty;
                }

                if (row.DataBoundItem is InvoiceSummaryDto factura)
                {
                    row.DefaultCellStyle.BackColor = factura.Estado switch
                    {
                        "Pagada" => Color.FromArgb(232, 245, 232),
                        "Vencida" => Color.FromArgb(255, 235, 235),
                        "Parcial" => Color.FromArgb(255, 250, 230),
                        "Anulada" => Color.FromArgb(240, 240, 240),
                        _ => Color.White
                    };

                    if (factura.Estado is "Vencida")
                    {
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
        }
    }

    private void EnsureInvoiceActionColumns()
    {
        if (_gridInvoices.Columns.Contains("Acciones"))
        {
            return;
        }

        _gridInvoices.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Acciones",
            HeaderText = "Acciones",
            Width = 180,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void OnInvoiceListCellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (_gridInvoices.Rows[e.RowIndex].DataBoundItem is not InvoiceSummaryDto factura)
        {
            return;
        }

        if (_gridInvoices.Columns[e.ColumnIndex].Name != "Acciones")
        {
            return;
        }

        var buttonBounds = GetInvoiceActionButtonBounds(new Size(_gridInvoices.Columns[e.ColumnIndex].Width, _gridInvoices.Rows[e.RowIndex].Height));
        var clickPoint = new Point(e.X, e.Y);

        var selectedAction = buttonBounds.FindIndex(rect => rect.Contains(clickPoint)) switch
        {
            0 => "Ver",
            1 => "Imprimir",
            2 => "Anular",
            3 => "Eliminar",
            _ => null
        };

        if (selectedAction is null)
        {
            return;
        }

        switch (selectedAction)
        {
            case "Ver":
                ViewInvoiceDetail(factura.Id);
                break;
            case "Imprimir":
                PrintInvoice(factura.Id);
                break;
            case "Anular":
                VoidInvoice(factura.Id);
                break;
            case "Eliminar":
                DeleteInvoice(factura.Id);
                break;
        }
    }

    private void OnInvoiceListCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _gridInvoices.Columns[e.ColumnIndex].Name != "Acciones")
        {
            return;
        }

        var graphics = e.Graphics;
        if (graphics is null)
        {
            return;
        }

        e.PaintBackground(e.CellBounds, true);

        var buttonBounds = GetInvoiceActionButtonBounds(e.CellBounds.Size);
        var icons = new[] { "👁", "🖨", "🚫", "🗑" };
        var backColors = new[]
        {
            Color.FromArgb(232, 245, 255),
            Color.FromArgb(240, 247, 240),
            Color.FromArgb(255, 245, 225),
            Color.FromArgb(255, 235, 235)
        };
        var foreColors = new[]
        {
            Color.FromArgb(42, 104, 176),
            Color.FromArgb(33, 99, 42),
            Color.FromArgb(176, 120, 12),
            Color.FromArgb(180, 40, 40)
        };

        using var font = new Font("Segoe UI Emoji", 10, FontStyle.Regular);

        for (var i = 0; i < buttonBounds.Count; i++)
        {
            var rect = buttonBounds[i];
            rect.Offset(e.CellBounds.Left, e.CellBounds.Top);

            using var path = new GraphicsPath();
            const int radius = 8;
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            using var brush = new SolidBrush(backColors[i]);
            using var pen = new Pen(Color.FromArgb(214, 218, 210));
            using var textBrush = new SolidBrush(foreColors[i]);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            graphics.FillPath(brush, path);
            graphics.DrawPath(pen, path);
            graphics.DrawString(icons[i], font, textBrush, rect, format);
        }

        e.Handled = true;
    }

    private static List<Rectangle> GetInvoiceActionButtonBounds(Size cellSize)
    {
        const int buttonWidth = 32;
        const int buttonHeight = 24;
        const int spacing = 8;
        var totalWidth = (buttonWidth * 4) + (spacing * 3);
        var startX = Math.Max(6, (cellSize.Width - totalWidth) / 2);
        var startY = Math.Max(4, (cellSize.Height - buttonHeight) / 2);

        return
        [
            new Rectangle(startX, startY, buttonWidth, buttonHeight),
            new Rectangle(startX + buttonWidth + spacing, startY, buttonWidth, buttonHeight),
            new Rectangle(startX + ((buttonWidth + spacing) * 2), startY, buttonWidth, buttonHeight),
            new Rectangle(startX + ((buttonWidth + spacing) * 3), startY, buttonWidth, buttonHeight)
        ];
    }

    private void ViewInvoiceDetail(long invoiceId)
    {
        var detail = _invoiceRepository.GetInvoiceDetailText(invoiceId);
        using var dialog = new Form
        {
            Text = "Detalle de Factura",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.Sizable,
            MinimizeBox = false,
            MaximizeBox = true,
            ShowInTaskbar = false,
            ClientSize = new Size(550, 600),
            BackColor = Color.White
        };

        var txtDetail = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 9.5f),
            ScrollBars = ScrollBars.Vertical,
            Text = detail,
            Margin = new Padding(16),
            BackColor = Color.FromArgb(250, 252, 248),
            BorderStyle = BorderStyle.FixedSingle
        };

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 10, 16, 10)
        };

        var btnClose = new Button
        {
            Text = "Cerrar",
            Width = 110,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(45, 111, 26),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            DialogResult = DialogResult.OK
        };
        btnClose.FlatAppearance.BorderSize = 0;

        btnPanel.Controls.Add(btnClose);
        dialog.Controls.Add(txtDetail);
        dialog.Controls.Add(btnPanel);
        dialog.ShowDialog(this);
    }

    private void EditInvoice(long invoiceId)
    {
        MessageBox.Show(
            $"Edición de factura #{invoiceId}\n\nEsta funcionalidad se implementará en una futura versión.",
            "Editar Factura",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void PrintInvoice(long invoiceId)
    {
        var invoice = _invoiceRepository.GetInvoicePrintDetail(invoiceId);
        if (invoice is null)
        {
            MessageBox.Show("No se encontró la factura para imprimir.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ShowInvoicePrintPreview(invoice, $"Factura-{invoice.Numero}", false);
    }

    private void PrintCurrentInvoiceDraft()
    {
        if (_cmbInvoiceCustomer.SelectedItem is not InvoiceCustomerLookupDto customer)
        {
            MessageBox.Show("Selecciona un cliente antes de imprimir la vista previa.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_invoiceItems.Count == 0)
        {
            MessageBox.Show("Agrega al menos un ítem antes de imprimir la factura.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var paymentMethod = _cmbInvoicePaymentMethod.SelectedItem as PaymentMethodLookupDto;
        var totals = ComputeInvoiceTotals();

        var invoice = new InvoicePrintDetailDto
        {
            Numero = "BORRADOR",
            Fecha = _dtpInvoiceDate.Value.Date,
            Cliente = customer.Nombre,
            Nit = customer.Nit,
            Direccion = string.Empty,
            MetodoPago = paymentMethod?.Nombre ?? "-",
            Estado = "Borrador",
            Subtotal = totals.subtotal,
            Retencion = 0,
            Total = totals.total,
            Saldo = totals.total,
            Notas = _txtInvoiceNotes.Text,
            Items = _invoiceItems.Select(item => new InvoicePrintItemDto
            {
                Codigo = item.Codigo,
                Descripcion = item.Descripcion,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                TotalLinea = item.TotalLinea
            }).ToList()
        };

        ShowInvoicePrintPreview(invoice, $"Factura-Borrador-{DateTime.Now:yyyyMMddHHmmss}", true);
    }

    private void ShowInvoicePrintPreview(InvoicePrintDetailDto invoice, string documentName, bool isDraft)
    {
        var printDoc = new System.Drawing.Printing.PrintDocument
        {
            DocumentName = documentName
        };

        printDoc.PrintPage += (_, e) =>
        {
            DrawInvoicePrintPage(e.Graphics!, e.MarginBounds, invoice, isDraft);
            e.HasMorePages = false;
        };

        using var preview = new PrintPreviewDialog
        {
            Document = printDoc,
            WindowState = FormWindowState.Maximized
        };
        preview.ShowDialog(this);
    }

    private static void DrawInvoicePrintPage(Graphics g, Rectangle bounds, InvoicePrintDetailDto invoice, bool isDraft)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var fontTitle = new Font("Times New Roman", 18, FontStyle.Bold | FontStyle.Italic);
        using var fontSubtitle = new Font("Times New Roman", 14, FontStyle.Bold);
        using var fontSection = new Font("Times New Roman", 11, FontStyle.Bold);
        using var fontText = new Font("Times New Roman", 10);
        using var fontSmall = new Font("Times New Roman", 9);
        using var fontMono = new Font("Consolas", 9);
        using var pen = new Pen(Color.Black, 1.1f);
        using var thinPen = new Pen(Color.Black, 0.8f);
        using var centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var rightFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        using var leftFormat = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        var pageLeft = bounds.Left;
        var pageTop = bounds.Top;
        var pageWidth = bounds.Width;
        var rightBoxWidth = 190f;
        var headerLeftWidth = pageWidth - rightBoxWidth - 16f;

        var headerTitleArea = new RectangleF(pageLeft, pageTop, headerLeftWidth, 58f);
        g.DrawString("ACEITESPRO FACTURACIÓN", fontTitle, Brushes.Black, headerTitleArea, centerFormat);
        g.DrawString("Documento comercial", fontSubtitle, Brushes.Black,
            new RectangleF(pageLeft, pageTop + 34f, headerLeftWidth, 24f), centerFormat);

        var documentBox = new RectangleF(pageLeft + headerLeftWidth + 16f, pageTop, rightBoxWidth, 78f);
        g.DrawRectangle(pen, documentBox.X, documentBox.Y, documentBox.Width, documentBox.Height);
        g.DrawLine(pen, documentBox.Left, documentBox.Top + 26f, documentBox.Right, documentBox.Top + 26f);
        g.DrawString(isDraft ? "FACTURA BORRADOR" : "FACTURA", fontSection,
            Brushes.Black, new RectangleF(documentBox.Left, documentBox.Top, documentBox.Width, 26f), centerFormat);
        g.DrawString("Nro:", fontText, Brushes.Black, new RectangleF(documentBox.Left + 10f, documentBox.Top + 30f, 36f, 16f), leftFormat);
        g.DrawString(invoice.Numero, fontSection, Brushes.Black, new RectangleF(documentBox.Left + 44f, documentBox.Top + 28f, documentBox.Width - 54f, 18f), leftFormat);
        g.DrawString("Fecha:", fontText, Brushes.Black, new RectangleF(documentBox.Left + 10f, documentBox.Top + 50f, 44f, 18f), leftFormat);
        g.DrawString(invoice.Fecha.ToString("dd/MM/yyyy"), fontText, Brushes.Black, new RectangleF(documentBox.Left + 54f, documentBox.Top + 50f, documentBox.Width - 64f, 18f), leftFormat);

        var infoBox = new RectangleF(pageLeft, pageTop + 92f, pageWidth, 130f);
        g.DrawRectangle(pen, infoBox.X, infoBox.Y, infoBox.Width, infoBox.Height);

        var lineY = infoBox.Top + 10f;
        g.DrawString($"Recibimos de:  {invoice.Cliente}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 10f, lineY, infoBox.Width * 0.7f, 18f), leftFormat);
        g.DrawString($"Nit:  {invoice.Nit}", fontText, Brushes.Black, new RectangleF(infoBox.Left + infoBox.Width * 0.72f, lineY, infoBox.Width * 0.25f, 18f), leftFormat);

        lineY += 22f;
        g.DrawString($"Dirección:  {(string.IsNullOrWhiteSpace(invoice.Direccion) ? "-" : invoice.Direccion)}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 10f, lineY, infoBox.Width * 0.7f, 18f), leftFormat);
        g.DrawString($"Estado:  {invoice.Estado}", fontText, Brushes.Black, new RectangleF(infoBox.Left + infoBox.Width * 0.72f, lineY, infoBox.Width * 0.25f, 18f), leftFormat);

        lineY += 22f;
        g.DrawString($"La suma de:  {ConvertAmountToSpanishWords(invoice.Total)}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 10f, lineY, infoBox.Width - 20f, 18f), leftFormat);

        lineY += 26f;
        g.DrawString("Concepto:", fontSection, Brushes.Black, new RectangleF(infoBox.Left + 10f, lineY, 90f, 18f), leftFormat);
        g.DrawString("Venta de productos", fontText, Brushes.Black, new RectangleF(infoBox.Left + 102f, lineY, 220f, 18f), leftFormat);
        g.DrawString($"Documento:  {(isDraft ? "Vista previa" : "Factura")}", fontText, Brushes.Black, new RectangleF(infoBox.Left + infoBox.Width * 0.58f, lineY, infoBox.Width * 0.18f, 18f), leftFormat);
        g.DrawString($"Pago:  {invoice.Total:N0}", fontText, Brushes.Black, new RectangleF(infoBox.Left + infoBox.Width * 0.78f, lineY, infoBox.Width * 0.18f, 18f), leftFormat);

        lineY += 24f;
        g.DrawString($"Subtotal  {invoice.Subtotal:N0}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 10f, lineY, 140f, 18f), leftFormat);
        g.DrawString($"Retención  {invoice.Retencion:N0}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 170f, lineY, 150f, 18f), leftFormat);
        g.DrawString($"Saldo  {invoice.Saldo:N0}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 350f, lineY, 120f, 18f), leftFormat);
        g.DrawString($"Forma de pago  {invoice.MetodoPago}", fontText, Brushes.Black, new RectangleF(infoBox.Left + 500f, lineY, infoBox.Width - 510f, 18f), leftFormat);

        var tableTop = infoBox.Bottom + 8f;
        var rowHeight = 24f;
        var tableHeight = 30f + (Math.Max(invoice.Items.Count, 5) * rowHeight);
        var tableRect = new RectangleF(pageLeft, tableTop, pageWidth, tableHeight);
        g.DrawRectangle(pen, tableRect.X, tableRect.Y, tableRect.Width, tableRect.Height);

        var columns = new[] { 90f, 310f, 70f, 120f, 120f };
        var columnTitles = new[] { "Código", "Descripción", "Cant.", "Vr. Unitario", "Pago" };
        var headerBottom = tableRect.Top + 28f;
        g.DrawLine(thinPen, tableRect.Left, headerBottom, tableRect.Right, headerBottom);

        var x = tableRect.Left;
        for (var i = 0; i < columns.Length; i++)
        {
            if (i > 0)
            {
                g.DrawLine(thinPen, x, tableRect.Top, x, tableRect.Bottom);
            }

            var width = i == columns.Length - 1 ? tableRect.Right - x : columns[i];
            g.DrawString(columnTitles[i], fontSection, Brushes.Black, new RectangleF(x + 4f, tableRect.Top + 4f, width - 8f, 20f), i >= 2 ? rightFormat : leftFormat);
            x += width;
        }

        for (var row = 0; row < Math.Max(invoice.Items.Count, 5); row++)
        {
            var y = headerBottom + (row * rowHeight);
            g.DrawLine(thinPen, tableRect.Left, y + rowHeight, tableRect.Right, y + rowHeight);

            if (row >= invoice.Items.Count)
            {
                continue;
            }

            var item = invoice.Items[row];
            var colX = tableRect.Left;
            g.DrawString(item.Codigo, fontMono, Brushes.Black, new RectangleF(colX + 4f, y + 3f, columns[0] - 8f, rowHeight - 6f), leftFormat);
            colX += columns[0];
            g.DrawString(item.Descripcion, fontText, Brushes.Black, new RectangleF(colX + 4f, y + 3f, columns[1] - 8f, rowHeight - 6f), leftFormat);
            colX += columns[1];
            g.DrawString(item.Cantidad.ToString("N0"), fontText, Brushes.Black, new RectangleF(colX + 4f, y + 3f, columns[2] - 8f, rowHeight - 6f), rightFormat);
            colX += columns[2];
            g.DrawString(item.PrecioUnitario.ToString("N0"), fontText, Brushes.Black, new RectangleF(colX + 4f, y + 3f, columns[3] - 8f, rowHeight - 6f), rightFormat);
            colX += columns[3];
            g.DrawString(item.TotalLinea.ToString("N0"), fontSection, Brushes.Black, new RectangleF(colX + 4f, y + 3f, tableRect.Right - colX - 8f, rowHeight - 6f), rightFormat);
        }

        var totalBarTop = tableRect.Bottom + 4f;
        var totalBar = new RectangleF(pageLeft, totalBarTop, pageWidth, 26f);
        g.DrawRectangle(thinPen, totalBar.X, totalBar.Y, totalBar.Width, totalBar.Height);
        g.DrawString("Total Factura:", fontSection, Brushes.Black, new RectangleF(totalBar.Left + pageWidth - 220f, totalBar.Top, 110f, totalBar.Height), rightFormat);
        g.DrawString(invoice.Total.ToString("N0"), fontSection, Brushes.Black, new RectangleF(totalBar.Left + pageWidth - 105f, totalBar.Top, 90f, totalBar.Height), rightFormat);

        var bottomBoxTop = totalBar.Bottom + 4f;
        var bottomBox = new RectangleF(pageLeft, bottomBoxTop, pageWidth, 86f);
        g.DrawRectangle(pen, bottomBox.X, bottomBox.Y, bottomBox.Width, bottomBox.Height);
        g.DrawLine(thinPen, bottomBox.Left, bottomBox.Top + 24f, bottomBox.Right, bottomBox.Top + 24f);
        g.DrawLine(thinPen, bottomBox.Left + (bottomBox.Width * 0.55f), bottomBox.Top, bottomBox.Left + (bottomBox.Width * 0.55f), bottomBox.Bottom);
        g.DrawString("Forma de pago", fontSection, Brushes.Black, new RectangleF(bottomBox.Left, bottomBox.Top, bottomBox.Width * 0.55f, 24f), centerFormat);
        g.DrawString("Firma y Sello", fontSection, Brushes.Black, new RectangleF(bottomBox.Left + (bottomBox.Width * 0.55f), bottomBox.Top, bottomBox.Width * 0.45f, 24f), centerFormat);

        g.DrawLine(thinPen, bottomBox.Left + 90f, bottomBox.Top + 24f, bottomBox.Left + 90f, bottomBox.Bottom);
        g.DrawLine(thinPen, bottomBox.Left + 280f, bottomBox.Top + 24f, bottomBox.Left + 280f, bottomBox.Bottom);
        g.DrawLine(thinPen, bottomBox.Left + 400f, bottomBox.Top + 24f, bottomBox.Left + 400f, bottomBox.Bottom);

        g.DrawString("Referencia", fontText, Brushes.Black, new RectangleF(bottomBox.Left + 4f, bottomBox.Top + 26f, 82f, 18f), centerFormat);
        g.DrawString("Banco / Medio", fontText, Brushes.Black, new RectangleF(bottomBox.Left + 94f, bottomBox.Top + 26f, 182f, 18f), centerFormat);
        g.DrawString("Responsable", fontText, Brushes.Black, new RectangleF(bottomBox.Left + 284f, bottomBox.Top + 26f, 112f, 18f), centerFormat);
        g.DrawString("Valor", fontText, Brushes.Black, new RectangleF(bottomBox.Left + 404f, bottomBox.Top + 26f, 140f, 18f), centerFormat);

        g.DrawString(invoice.Numero, fontText, Brushes.Black, new RectangleF(bottomBox.Left + 4f, bottomBox.Top + 50f, 82f, 18f), centerFormat);
        g.DrawString(invoice.MetodoPago, fontText, Brushes.Black, new RectangleF(bottomBox.Left + 94f, bottomBox.Top + 50f, 182f, 18f), centerFormat);
        g.DrawString(invoice.Cliente, fontText, Brushes.Black, new RectangleF(bottomBox.Left + 284f, bottomBox.Top + 50f, 112f, 18f), centerFormat);
        g.DrawString(invoice.Total.ToString("N0"), fontText, Brushes.Black, new RectangleF(bottomBox.Left + 404f, bottomBox.Top + 50f, 140f, 18f), rightFormat);

        if (!string.IsNullOrWhiteSpace(invoice.Notas))
        {
            g.DrawString($"Notas: {invoice.Notas}", fontSmall, Brushes.Black,
                new RectangleF(bottomBox.Left + bottomBox.Width * 0.57f, bottomBox.Top + 30f, bottomBox.Width * 0.4f, 34f));
        }
    }

    private static string ConvertAmountToSpanishWords(decimal amount)
    {
        var integerPart = Math.Abs((long)Math.Round(amount, 0, MidpointRounding.AwayFromZero));
        var words = ConvertNumberToSpanish(integerPart);
        return $"{words} PESOS M/L";
    }

    private static string ConvertNumberToSpanish(long number)
    {
        if (number == 0) return "CERO";
        if (number < 0) return $"MENOS {ConvertNumberToSpanish(Math.Abs(number))}";
        if (number == 1) return "UN";
        if (number < 16)
        {
            return number switch
            {
                2 => "DOS",
                3 => "TRES",
                4 => "CUATRO",
                5 => "CINCO",
                6 => "SEIS",
                7 => "SIETE",
                8 => "OCHO",
                9 => "NUEVE",
                10 => "DIEZ",
                11 => "ONCE",
                12 => "DOCE",
                13 => "TRECE",
                14 => "CATORCE",
                15 => "QUINCE",
                _ => "UN"
            };
        }

        if (number < 20) return $"DIECI{ConvertNumberToSpanish(number - 10).ToLowerInvariant()}".ToUpperInvariant();
        if (number == 20) return "VEINTE";
        if (number < 30) return $"VEINTI{ConvertNumberToSpanish(number - 20).ToLowerInvariant()}".ToUpperInvariant();
        if (number < 100)
        {
            var tens = number / 10;
            var units = number % 10;
            var tensText = tens switch
            {
                3 => "TREINTA",
                4 => "CUARENTA",
                5 => "CINCUENTA",
                6 => "SESENTA",
                7 => "SETENTA",
                8 => "OCHENTA",
                9 => "NOVENTA",
                _ => string.Empty
            };

            return units == 0 ? tensText : $"{tensText} Y {ConvertNumberToSpanish(units)}";
        }

        if (number == 100) return "CIEN";
        if (number < 200) return $"CIENTO {ConvertNumberToSpanish(number - 100)}";
        if (number < 1000)
        {
            var hundreds = number / 100;
            var remainder = number % 100;
            var hundredsText = hundreds switch
            {
                2 => "DOSCIENTOS",
                3 => "TRESCIENTOS",
                4 => "CUATROCIENTOS",
                5 => "QUINIENTOS",
                6 => "SEISCIENTOS",
                7 => "SETECIENTOS",
                8 => "OCHOCIENTOS",
                9 => "NOVECIENTOS",
                _ => "CIENTO"
            };

            return remainder == 0 ? hundredsText : $"{hundredsText} {ConvertNumberToSpanish(remainder)}";
        }

        if (number == 1000) return "MIL";
        if (number < 2000) return $"MIL {ConvertNumberToSpanish(number - 1000)}";
        if (number < 1000000)
        {
            var thousands = number / 1000;
            var remainder = number % 1000;
            var thousandsText = $"{ConvertNumberToSpanish(thousands)} MIL";
            return remainder == 0 ? thousandsText : $"{thousandsText} {ConvertNumberToSpanish(remainder)}";
        }

        if (number == 1000000) return "UN MILLÓN";
        if (number < 2000000) return $"UN MILLÓN {ConvertNumberToSpanish(number - 1000000)}";
        if (number < 1000000000000)
        {
            var millions = number / 1000000;
            var remainder = number % 1000000;
            var millionsText = $"{ConvertNumberToSpanish(millions)} MILLONES";
            return remainder == 0 ? millionsText : $"{millionsText} {ConvertNumberToSpanish(remainder)}";
        }

        return number.ToString("N0");
    }

    private void VoidInvoice(long invoiceId)
    {
        var confirm = MessageBox.Show(
            "¿Desea anular esta factura?\n\n" +
            "Esta acción:\n" +
            "• Marcará la factura como 'Anulada'\n" +
            "• Devolverá el stock de productos al inventario\n" +
            "• Pondrá el saldo en $ 0\n\n" +
            "¿Continuar?",
            "Anular Factura",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _invoiceRepository.VoidInvoice(invoiceId);
            MessageBox.Show("Factura anulada correctamente.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadInvoicesList();
            LoadDashboard();
            LoadRecentInvoices();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DeleteInvoice(long invoiceId)
    {
        var confirm = MessageBox.Show(
            "⚠ ADVERTENCIA: ¿Desea ELIMINAR esta factura?\n\n" +
            "Esta acción:\n" +
            "• Eliminará la factura de forma PERMANENTE\n" +
            "• Eliminará todos los ítems asociados\n" +
            "• Eliminará todos los pagos registrados\n" +
            "• Devolverá el stock al inventario\n" +
            "• Esta acción es IRREVERSIBLE\n\n" +
            "¿Está completamente seguro?",
            "Eliminar Factura",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Stop);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _invoiceRepository.DeleteInvoice(invoiceId);
            MessageBox.Show("Factura eliminada correctamente.", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadInvoicesList();
            LoadDashboard();
            LoadRecentInvoices();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar factura:\\n{ex.Message}", "Facturas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
    }

    private void RemoveInvoiceItem()
    {
        if (_gridInvoiceItems.CurrentRow?.DataBoundItem is InvoiceItemDraft item)
        {
            _invoiceItems.Remove(item);
            RecalculateInvoiceTotals();
        }
    }

    private (decimal subtotal, decimal total) ComputeInvoiceTotals()
    {
        foreach (var item in _invoiceItems)
        {
            item.TotalLinea = Math.Round(item.Cantidad * item.PrecioUnitario, 0);
        }

        var subtotal = _invoiceItems.Sum(x => x.TotalLinea);
        var total = subtotal;
        return (subtotal, total);
    }

    private void RecalculateInvoiceTotals()
    {
        var totals = ComputeInvoiceTotals();
        _lblInvoiceSubtotal.Text = $"$ {totals.subtotal:N0}";
        _lblInvoiceTotal.Text = $"$ {totals.total:N0}";
        _lblInvoiceItemsCount.Text = _invoiceItems.Count == 1
            ? "1 producto"
            : $"{_invoiceItems.Count:N0} productos";
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
        _txtItemDescription.Clear();
        UpdateInvoiceSummaryDate();
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
ORDER BY f.Numero
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
ORDER BY Codigo;";

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

    private Panel BuildPaymentsListView()
    {
        var view = new Panel { Dock = DockStyle.Fill };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(12),
            Padding = new Padding(0, 72, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        view.Controls.Add(layout);

        // Summary cards
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

        _lblPaymentsTotalPagos = CreateCard(cardsPanel, 0, "Total Pagos", out _);
        _lblPaymentsMontoTotal = CreateCard(cardsPanel, 1, "Monto Recaudado", out _);
        _lblPaymentsClientes = CreateCard(cardsPanel, 2, "Clientes", out _);
        _lblPaymentsFacturas = CreateCard(cardsPanel, 3, "Facturas Pagadas", out _);

        layout.Controls.Add(cardsPanel, 0, 0);

        // Search bar
        var searchCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 0, 8)
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

        _txtSearchPayments = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar por número de factura, cliente o método de pago...",
            Font = new Font("Segoe UI", 10)
        };
        _txtSearchPayments.TextChanged += (_, _) => LoadPaymentsGrid(_txtSearchPayments.Text);

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
        btnClearSearch.Click += (_, _) => { _txtSearchPayments.Clear(); _txtSearchPayments.Focus(); };

        searchRow.Controls.Add(_txtSearchPayments, 0, 0);
        searchRow.Controls.Add(btnClearSearch, 1, 0);
        searchCard.Controls.Add(searchRow);

        layout.Controls.Add(searchCard, 0, 1);

        // Grid card with embedded title
        var tableCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var gridTitle = new Label
        {
            Text = "📋 Historial de Pagos",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        tableCard.Controls.Add(gridTitle);

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
        _gridPayments.CellContentClick += OnPaymentsCellContentClick;
        AddGridWithTopMargin(tableCard, _gridPayments, 20);

        layout.Controls.Add(tableCard, 0, 2);
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

        var paymentGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 8,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0),
            Height = 260
        };
        paymentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        paymentGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        paymentGrid.Margin = new Padding(0, 0, 0, 8);

        // Factura
        paymentGrid.Controls.Add(new Label { Text = "Factura *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 0);
        _cmbPaymentInvoice = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbPaymentInvoice.SelectedIndexChanged += (_, _) => OnInvoiceSelected();
        paymentGrid.Controls.Add(_cmbPaymentInvoice, 0, 1);

        // Fecha
        paymentGrid.Controls.Add(new Label { Text = "Fecha *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 2);
        _dtpPaymentDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        _dtpPaymentDate.ValueChanged += (_, _) => _lblPaymentSummaryDate.Text = _dtpPaymentDate.Value.ToString("d 'de' MMMM 'de' yyyy");
        paymentGrid.Controls.Add(_dtpPaymentDate, 0, 3);

        // Monto
        paymentGrid.Controls.Add(new Label { Text = "Monto *", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 4);
        _numPaymentAmount = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Maximum = 999999999, ThousandsSeparator = true };
        _numPaymentAmount.ValueChanged += (_, _) =>
        {
            _lblPaymentTotal.Text = $"$ {_numPaymentAmount.Value:N0}";
            UpdatePaymentReference();
        };
        paymentGrid.Controls.Add(_numPaymentAmount, 0, 5);

        // Método de Pago
        paymentGrid.Controls.Add(new Label { Text = "Método de Pago", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 6);
        _cmbPaymentMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        paymentGrid.Controls.Add(_cmbPaymentMethod, 0, 7);

        // Referencia y Notas
        var refNotesGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(0, 12, 0, 0),
            Padding = Padding.Empty,
            Height = 180
        };
        refNotesGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        refNotesGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        refNotesGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        refNotesGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        refNotesGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        refNotesGrid.Controls.Add(new Label { Text = "Referencia (automática)", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 0);
        _txtPaymentReference = new TextBox { Dock = DockStyle.Fill, Multiline = false, ReadOnly = true, BackColor = Color.FromArgb(245, 247, 245) };
        refNotesGrid.Controls.Add(_txtPaymentReference, 0, 1);

        refNotesGrid.Controls.Add(new Label { Text = "Tipo de pago (automático)", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = false, Height = 24 }, 0, 2);
        _txtPaymentNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(245, 247, 245) };
        refNotesGrid.Controls.Add(_txtPaymentNotes, 0, 3);

        paymentCard.Controls.Add(refNotesGrid);
        paymentCard.Controls.Add(paymentGrid);
        paymentCard.Controls.Add(paymentTitle);

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

        summaryGrid.Controls.Add(new Label { Text = "Factura:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _lblPaymentSummaryInvoice = new Label { Text = "-", Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblPaymentSummaryInvoice, 1, 0);

        summaryGrid.Controls.Add(new Label { Text = "Saldo Pendiente:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        _lblPaymentBalance = new Label { Text = "$ 0", Font = new Font("Segoe UI", 11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblPaymentBalance, 1, 1);

        summaryGrid.Controls.Add(new Label { Text = "Fecha de Pago:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        _lblPaymentSummaryDate = new Label { Text = DateTime.Today.ToString("d 'de' MMMM 'de' yyyy"), Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(33, 33, 33) };
        summaryGrid.Controls.Add(_lblPaymentSummaryDate, 1, 2);

        summaryGrid.Controls.Add(new Label { Text = "Monto a Pagar:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        _lblPaymentTotal = new Label { Text = "$ 0", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(11, 52, 22), Padding = new Padding(0, 12, 0, 0) };
        summaryGrid.Controls.Add(_lblPaymentTotal, 1, 3);

        summaryContentPanel.Controls.Add(summaryGrid);

        summaryCard.Controls.Add(summaryContentPanel);

        var btnPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 112,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(0, 12, 0, 0)
        };
        btnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        btnPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        btnPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        var btnSave = new Button
        {
            Text = "✓ Registrar Pago",
            Dock = DockStyle.Fill,
            Height = 48,
            BackColor = Color.FromArgb(33, 99, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SavePayment();
        btnPanel.Controls.Add(btnSave, 0, 0);

        var btnClear = new Button
        {
            Text = "🔄 Limpiar",
            Dock = DockStyle.Fill,
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
        btnPanel.Controls.Add(btnClear, 0, 1);

        summaryCard.Controls.Add(btnPanel);
        rightPanel.Controls.Add(summaryCard);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        return view;
    }

    private void LoadPaymentsList()
    {
        var resumen = _paymentRepository.GetResumen();
        _lblPaymentsTotalPagos.Text = resumen.TotalPagos.ToString("N0");
        _lblPaymentsMontoTotal.Text = $"$ {resumen.MontoTotal:N0}";
        _lblPaymentsClientes.Text = resumen.ClientesPagaron.ToString("N0");
        _lblPaymentsFacturas.Text = resumen.FacturasPagadas.ToString("N0");

        _txtSearchPayments.Clear();
        LoadPaymentsGrid(null);
    }

    private void LoadPaymentsGrid(string? search)
    {
        var data = _paymentRepository.GetPaymentHistory(search);
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

            EnsurePaymentActionColumn();
        }
    }

    private void EnsurePaymentActionColumn()
    {
        if (_gridPayments.Columns.Contains("AccionAnular"))
        {
            return;
        }

        _gridPayments.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "AccionAnular",
            HeaderText = "Acción",
            Text = "🗑 Anular",
            UseColumnTextForButtonValue = true,
            Width = 100,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void OnPaymentsCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (_gridPayments.Columns[e.ColumnIndex].Name != "AccionAnular")
        {
            return;
        }

        if (_gridPayments.Rows[e.RowIndex].DataBoundItem is not PaymentGridRowDto pago)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"¿Desea anular el pago de $ {pago.Valor:N0} de la factura {pago.NumeroFactura}?\n\n" +
            "El saldo será devuelto a la factura.",
            "Anular Pago",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _paymentRepository.VoidPayment(pago.Id);
            MessageBox.Show("Pago anulado correctamente.", "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadPaymentsList();
            LoadDashboard();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Pagos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        if (_pendingPaymentInvoiceId.HasValue)
        {
            for (var i = 0; i < _cmbPaymentInvoice.Items.Count; i++)
            {
                if (_cmbPaymentInvoice.Items[i] is PaymentLookupDto lookup && lookup.Id == _pendingPaymentInvoiceId.Value)
                {
                    _cmbPaymentInvoice.SelectedIndex = i;
                    break;
                }
            }

            _pendingPaymentInvoiceId = null;
        }
    }

    private void OnInvoiceSelected()
    {
        if (_cmbPaymentInvoice.SelectedItem is not PaymentLookupDto invoice)
        {
            _lblPaymentBalance.Text = "$ 0";
            _lblPaymentSummaryInvoice.Text = "-";
            _numPaymentAmount.Value = 0;
            UpdatePaymentReference();
            return;
        }

        _lblPaymentBalance.Text = $"$ {invoice.Saldo:N0}";
        _lblPaymentSummaryInvoice.Text = invoice.Numero;
        _numPaymentAmount.Value = Math.Min(_numPaymentAmount.Maximum, invoice.Saldo);
        _lblPaymentTotal.Text = $"$ {_numPaymentAmount.Value:N0}";
        UpdatePaymentReference();
    }

    private void UpdatePaymentReference()
    {
        if (_cmbPaymentInvoice.SelectedItem is not PaymentLookupDto invoice || _numPaymentAmount.Value <= 0)
        {
            _txtPaymentReference.Text = string.Empty;
            _txtPaymentNotes.Text = string.Empty;
            return;
        }

        var nextNumber = _paymentRepository.GetPaymentCount(invoice.Id) + 1;
        _txtPaymentReference.Text = $"{invoice.Numero}-{nextNumber}";

        _txtPaymentNotes.Text = _numPaymentAmount.Value >= invoice.Saldo
            ? "Pago Final"
            : "Abono Parcial";
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
            var excess = amount - invoice.Saldo;
            var confirm = MessageBox.Show(
                $"El monto ingresado ($ {amount:N0}) supera el saldo pendiente ($ {invoice.Saldo:N0}).\n" +
                $"Se generará un saldo a favor de $ {excess:N0} para el cliente.\n\n" +
                "¿Desea continuar?",
                "Saldo a Favor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }
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
        _lblPaymentSummaryInvoice.Text = "-";
        _lblPaymentSummaryDate.Text = DateTime.Today.ToString("d 'de' MMMM 'de' yyyy");
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
            RowCount = 5,
            Margin = new Padding(12),
            Padding = Padding.Empty
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        view.Controls.Add(mainLayout);

        var cardsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 8)
        };
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

        _lblCarteraClientes = CreateCard(cardsPanel, 0, "Clientes con Saldo", out _);
        _lblCarteraFacturas = CreateCard(cardsPanel, 1, "Facturas Pendientes", out _);
        _lblCarteraTotalCobrar = CreateCard(cardsPanel, 2, "Total por Cobrar", out _);
        _lblCarteraVencido = CreateCard(cardsPanel, 3, "Saldo Vencido", out _);
        _lblCarteraSaldoFavor = CreateCard(cardsPanel, 4, "Saldo a Favor", out _);

        mainLayout.Controls.Add(cardsPanel, 0, 0);

        // Search / filter bar
        var searchCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 0, 8)
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

        _txtSearchCartera = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar por número de factura o cliente...",
            Font = new Font("Segoe UI", 10)
        };
        _txtSearchCartera.TextChanged += (_, _) => LoadFacturasPendientes(_txtSearchCartera.Text);

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
        btnClearSearch.Click += (_, _) => { _txtSearchCartera.Clear(); _txtSearchCartera.Focus(); };

        searchRow.Controls.Add(_txtSearchCartera, 0, 0);
        searchRow.Controls.Add(btnClearSearch, 1, 0);
        searchCard.Controls.Add(searchRow);

        mainLayout.Controls.Add(searchCard, 0, 1);

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
        _gridFacturasPendientes.CellContentClick += OnCarteraFacturaCellContentClick;
        AddGridWithTopMargin(facturasPendientesCard, _gridFacturasPendientes, 20);

        mainLayout.Controls.Add(facturasPendientesCard, 0, 2);

        // Row 3: Clientes con Saldo + Clientes con Saldo a Favor (side by side)
        var clientesRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        clientesRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        clientesRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

        var clientesSaldoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 6, 12)
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
        ConfigureGridStyle(_gridClientesSaldo);
        _gridClientesSaldo.DefaultCellStyle.Padding = new Padding(6, 8, 6, 8);
        AddGridWithTopMargin(clientesSaldoCard, _gridClientesSaldo, 20);

        clientesRow.Controls.Add(clientesSaldoCard, 0, 0);

        var clientesSaldoFavorCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(6, 0, 0, 12)
        };

        var clientesSaldoFavorTitle = new Label
        {
            Text = "💚 Clientes con Saldo a Favor",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        clientesSaldoFavorCard.Controls.Add(clientesSaldoFavorTitle);

        _gridClientesSaldoFavor = new DataGridView
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
        ConfigureGridStyle(_gridClientesSaldoFavor);
        _gridClientesSaldoFavor.DefaultCellStyle.Padding = new Padding(6, 8, 6, 8);
        AddGridWithTopMargin(clientesSaldoFavorCard, _gridClientesSaldoFavor, 20);

        clientesRow.Controls.Add(clientesSaldoFavorCard, 1, 0);

        mainLayout.Controls.Add(clientesRow, 0, 3);

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
        AddGridWithTopMargin(edadSaldosCard, _gridEdadSaldos, 20);

        mainLayout.Controls.Add(edadSaldosCard, 0, 4);

        return view;
    }

    private void LoadCartera()
    {
        var resumen = _carteraRepository.GetResumen();
        _lblCarteraClientes.Text = resumen.ClientesConSaldo.ToString("N0");
        _lblCarteraFacturas.Text = resumen.FacturasPendientes.ToString("N0");
        _lblCarteraTotalCobrar.Text = resumen.TotalPorCobrar.ToString("C0");
        _lblCarteraVencido.Text = resumen.SaldoVencido.ToString("C0");
        _lblCarteraSaldoFavor.Text = resumen.TotalSaldoAFavor.ToString("C0");

        LoadFacturasPendientes(_txtSearchCartera.Text);

        var clientesSaldo = _carteraRepository.GetClientesConSaldo();
        _gridClientesSaldo.DataSource = clientesSaldo;

        if (_gridClientesSaldo.Columns.Count > 0)
        {
            _gridClientesSaldo.Columns["Id"]!.Visible = false;
            _gridClientesSaldo.Columns["Codigo"]!.HeaderText = "Código";
            _gridClientesSaldo.Columns["Codigo"]!.Width = 100;
            _gridClientesSaldo.Columns["Nombre"]!.HeaderText = "Cliente";
            _gridClientesSaldo.Columns["Nombre"]!.MinimumWidth = 200;
            _gridClientesSaldo.Columns["Telefono"]!.HeaderText = "Teléfono";
            _gridClientesSaldo.Columns["Telefono"]!.Width = 130;
            _gridClientesSaldo.Columns["FacturasPendientes"]!.HeaderText = "Facturas";
            _gridClientesSaldo.Columns["FacturasPendientes"]!.Width = 90;
            _gridClientesSaldo.Columns["SaldoTotal"]!.HeaderText = "Saldo Total";
            _gridClientesSaldo.Columns["SaldoTotal"]!.DefaultCellStyle.Format = "C0";
            _gridClientesSaldo.Columns["SaldoTotal"]!.Width = 130;
            _gridClientesSaldo.Columns["SaldoVencido"]!.HeaderText = "Saldo Vencido";
            _gridClientesSaldo.Columns["SaldoVencido"]!.DefaultCellStyle.Format = "C0";
            _gridClientesSaldo.Columns["SaldoVencido"]!.Width = 130;

            foreach (DataGridViewRow row in _gridClientesSaldo.Rows)
            {
                if (row.DataBoundItem is ClienteSaldoDto cliente && cliente.SaldoVencido > 0)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 230);
                }
            }
        }

        var clientesSaldoFavor = _carteraRepository.GetClientesConSaldoAFavor();
        _gridClientesSaldoFavor.DataSource = clientesSaldoFavor;

        if (_gridClientesSaldoFavor.Columns.Count > 0)
        {
            _gridClientesSaldoFavor.Columns["Id"]!.Visible = false;
            _gridClientesSaldoFavor.Columns["Codigo"]!.HeaderText = "Código";
            _gridClientesSaldoFavor.Columns["Codigo"]!.Width = 100;
            _gridClientesSaldoFavor.Columns["Nombre"]!.HeaderText = "Cliente";
            _gridClientesSaldoFavor.Columns["Nombre"]!.MinimumWidth = 180;
            _gridClientesSaldoFavor.Columns["Telefono"]!.HeaderText = "Teléfono";
            _gridClientesSaldoFavor.Columns["Telefono"]!.Width = 120;
            _gridClientesSaldoFavor.Columns["FacturasConCredito"]!.HeaderText = "Facturas";
            _gridClientesSaldoFavor.Columns["FacturasConCredito"]!.Width = 80;
            _gridClientesSaldoFavor.Columns["SaldoAFavor"]!.HeaderText = "Saldo a Favor";
            _gridClientesSaldoFavor.Columns["SaldoAFavor"]!.DefaultCellStyle.Format = "C0";
            _gridClientesSaldoFavor.Columns["SaldoAFavor"]!.Width = 130;

            foreach (DataGridViewRow row in _gridClientesSaldoFavor.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 232);
            }
        }

        var edadSaldos = _carteraRepository.GetEdadSaldos();
        _gridEdadSaldos.DataSource = edadSaldos;

        if (_gridEdadSaldos.Columns.Count > 0)
        {
            _gridEdadSaldos.Columns["RangoEdad"]!.HeaderText = "Rango de Edad";
            _gridEdadSaldos.Columns["RangoEdad"]!.MinimumWidth = 160;
            _gridEdadSaldos.Columns["CantidadFacturas"]!.HeaderText = "N° Facturas";
            _gridEdadSaldos.Columns["CantidadFacturas"]!.Width = 110;
            _gridEdadSaldos.Columns["TotalSaldo"]!.HeaderText = "Total Saldo";
            _gridEdadSaldos.Columns["TotalSaldo"]!.DefaultCellStyle.Format = "C0";
            _gridEdadSaldos.Columns["TotalSaldo"]!.MinimumWidth = 140;
            _gridEdadSaldos.Columns["PorcentajeSaldo"]!.HeaderText = "% del Total";
            _gridEdadSaldos.Columns["PorcentajeSaldo"]!.DefaultCellStyle.Format = "N1";
            _gridEdadSaldos.Columns["PorcentajeSaldo"]!.Width = 100;

            foreach (DataGridViewRow row in _gridEdadSaldos.Rows)
            {
                if (row.DataBoundItem is EdadSaldoDto edad)
                {
                    row.DefaultCellStyle.BackColor = edad.RangoEdad switch
                    {
                        "0-30 días" => Color.FromArgb(232, 245, 232),
                        "31-60 días" => Color.FromArgb(255, 255, 230),
                        "61-90 días" => Color.FromArgb(255, 240, 230),
                        _ => Color.FromArgb(255, 220, 220)
                    };

                    if (edad.RangoEdad is "61-90 días" or "Más de 90 días")
                    {
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
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
            RowCount = 4,
            Margin = new Padding(12),
            Padding = Padding.Empty
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
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

        _lblBalanceFacturado = CreateCard(cardsPanel, 0, "Total Facturado", out _);
        _lblBalanceRecaudado = CreateCard(cardsPanel, 1, "Total Recaudado", out _);
        _lblBalanceCuentasPorCobrar = CreateCard(cardsPanel, 2, "Cuentas por Cobrar", out _);
        _lblBalanceNeto = CreateCard(cardsPanel, 3, "Balance Neto", out _);

        mainLayout.Controls.Add(cardsPanel, 0, 0);

        // 2. NAVEGADOR DE PERÍODOS
        mainLayout.Controls.Add(BuildBalancePeriodNavigator(), 0, 1);

        // 3. BALANCE DETALLE Y FACTURAS/PAGOS
        var analyticsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        analyticsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

        var balanceMensualCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 6, 0)
        };

        _lblBalanceDetalleTitulo = new Label
        {
            Text = "📈 Balance Mensual (Últimos 6 Meses)",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        balanceMensualCard.Controls.Add(_lblBalanceDetalleTitulo);

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
        _gridBalanceMensual.SelectionChanged += (_, _) => OnBalanceDetalleSelected();
        AddGridWithTopMargin(balanceMensualCard, _gridBalanceMensual, 20);

        analyticsLayout.Controls.Add(balanceMensualCard, 0, 0);

        // Panel derecho: Facturas del período + Pagos de factura seleccionada
        var rightSplit = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(6, 0, 0, 0),
            Padding = Padding.Empty
        };
        rightSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        rightSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        // Facturas del período
        var facturasCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 6)
        };

        _lblBalanceFacturasTitulo = new Label
        {
            Text = "📄 Facturas del Período",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        facturasCard.Controls.Add(_lblBalanceFacturasTitulo);

        _gridBalanceFacturas = new DataGridView
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
        ConfigureGridStyle(_gridBalanceFacturas);
        _gridBalanceFacturas.SelectionChanged += (_, _) => OnBalanceFacturaSelected();
        AddGridWithTopMargin(facturasCard, _gridBalanceFacturas, 20);

        rightSplit.Controls.Add(facturasCard, 0, 0);

        // Pagos de la factura seleccionada
        var pagosCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(16),
            Margin = new Padding(0, 6, 0, 0)
        };

        _lblBalancePagosTitulo = new Label
        {
            Text = "💰 Pagos de Factura",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        pagosCard.Controls.Add(_lblBalancePagosTitulo);

        _gridBalancePagos = new DataGridView
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
        ConfigureGridStyle(_gridBalancePagos);
        AddGridWithTopMargin(pagosCard, _gridBalancePagos, 20);

        rightSplit.Controls.Add(pagosCard, 0, 1);
        analyticsLayout.Controls.Add(rightSplit, 1, 0);

        mainLayout.Controls.Add(analyticsLayout, 0, 2);

        // 4. INFORMACIÓN ADICIONAL
        var infoCard = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(14, 10, 14, 10),
            Margin = new Padding(0, 8, 0, 0)
        };

        var infoTitle = new Label
        {
            Text = "📊 Indicadores Clave del Período",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        infoCard.Controls.Add(infoTitle);

        var infoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 4, 0, 0)
        };

        _lblBalanceInfo = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(90, 90, 90),
            Text = "Cargando indicadores..."
        };
        infoPanel.Controls.Add(_lblBalanceInfo);
        infoCard.Controls.Add(infoPanel);

        mainLayout.Controls.Add(infoCard, 0, 3);

        // Cargar datos del balance
        SetBalancePeriod(BalancePeriodo.Mensual, false);
        return view;
    }

    private Control BuildBalancePeriodNavigator()
    {
        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = Color.FromArgb(222, 226, 219),
            Radius = 14,
            Padding = new Padding(8, 6, 8, 6),
            Margin = new Padding(0, 0, 0, 8)
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        AddBalancePeriodButton(flow, "Diario", BalancePeriodo.Diario);
        AddBalancePeriodButton(flow, "Quincenal", BalancePeriodo.Quincenal);
        AddBalancePeriodButton(flow, "Mensual", BalancePeriodo.Mensual);
        AddBalancePeriodButton(flow, "Anual", BalancePeriodo.Anual);
        AddBalancePeriodButton(flow, "Total", BalancePeriodo.Total);

        var separator = new Label
        {
            Text = "|",
            Width = 16,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(200, 200, 200),
            Font = new Font("Segoe UI", 10),
            Margin = new Padding(2, 0, 2, 0)
        };
        flow.Controls.Add(separator);

        var lblFecha = new Label
        {
            Text = "Día:",
            Width = 32,
            Height = 32,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 55, 45),
            Margin = new Padding(0, 0, 4, 0)
        };
        flow.Controls.Add(lblFecha);

        _dtpBalanceFecha = new DateTimePicker
        {
            Width = 130,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today,
            Font = new Font("Segoe UI", 9),
            Margin = new Padding(0, 4, 0, 0)
        };
        _dtpBalanceFecha.ValueChanged += (_, _) => OnBalanceFechaChanged();
        flow.Controls.Add(_dtpBalanceFecha);

        card.Controls.Add(flow);
        return card;
    }

    private void AddBalancePeriodButton(Control parent, string text, BalancePeriodo periodo)
    {
        var button = new Button
        {
            Text = text,
            Width = 90,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 45),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 6, 0),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 210);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 248, 244);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(238, 242, 235);
        button.Click += (_, _) => SetBalancePeriod(periodo, true);

        _balancePeriodButtons[periodo] = button;
        parent.Controls.Add(button);
    }

    private void SetBalancePeriod(BalancePeriodo periodo, bool reload)
    {
        _balancePeriodoSeleccionado = periodo;
        _balanceFechaEspecifica = null;

        foreach (var (key, button) in _balancePeriodButtons)
        {
            var selected = key == periodo;
            button.BackColor = selected ? Color.FromArgb(45, 111, 26) : Color.White;
            button.ForeColor = selected ? Color.White : Color.FromArgb(45, 55, 45);
            button.FlatAppearance.BorderColor = selected ? Color.FromArgb(45, 111, 26) : Color.FromArgb(214, 218, 210);
        }

        if (reload)
        {
            LoadBalance();
        }
    }

    private void LoadBalance()
    {
        BalanceResumenDto resumen;
        List<BalanceFacturaDto> facturas;
        List<BalanceMensualDto> balanceDetalle;
        string detalleTitulo;
        string facturasTitulo;
        string periodoNombre;

        if (_balanceFechaEspecifica is { } fecha)
        {
            resumen = _balanceRepository.GetResumen(fecha);
            facturas = _balanceRepository.GetFacturas(fecha);
            balanceDetalle = _balanceRepository.GetBalanceDetalleFecha(fecha);
            var fechaStr = fecha.ToString("dd/MM/yyyy");
            detalleTitulo = $"📈 Balance — Semana del {fechaStr}";
            facturasTitulo = $"📄 Facturas del {fechaStr}";
            periodoNombre = fechaStr;
        }
        else
        {
            resumen = _balanceRepository.GetResumen(_balancePeriodoSeleccionado);
            facturas = _balanceRepository.GetFacturas(_balancePeriodoSeleccionado);
            balanceDetalle = _balanceRepository.GetBalanceDetalle(_balancePeriodoSeleccionado);
            detalleTitulo = GetBalanceDetalleTitle(_balancePeriodoSeleccionado);
            facturasTitulo = $"📄 Facturas ({GetBalancePeriodName(_balancePeriodoSeleccionado)})";
            periodoNombre = GetBalancePeriodName(_balancePeriodoSeleccionado);
        }

        _lblBalanceFacturado.Text = resumen.TotalFacturado.ToString("C0");
        _lblBalanceRecaudado.Text = resumen.TotalRecaudado.ToString("C0");
        _lblBalanceCuentasPorCobrar.Text = resumen.CuentasPorCobrar.ToString("C0");
        _lblBalanceNeto.Text = resumen.BalanceNeto.ToString("C0");

        _lblBalanceDetalleTitulo.Text = detalleTitulo;
        _lblBalanceFacturasTitulo.Text = facturasTitulo;

        // Cargar detalle de balance
        _gridBalanceMensual.DataSource = balanceDetalle;

        if (_gridBalanceMensual.Columns.Count > 0)
        {
            _gridBalanceMensual.Columns["Periodo"]!.Visible = false;
            _gridBalanceMensual.Columns["MesNombre"]!.HeaderText = "Período";
            _gridBalanceMensual.Columns["MesNombre"]!.Width = 150;
            _gridBalanceMensual.Columns["TotalFacturado"]!.HeaderText = "Facturado";
            _gridBalanceMensual.Columns["TotalFacturado"]!.DefaultCellStyle.Format = "C0";
            _gridBalanceMensual.Columns["TotalRecaudado"]!.HeaderText = "Recaudado";
            _gridBalanceMensual.Columns["TotalRecaudado"]!.DefaultCellStyle.Format = "C0";
            _gridBalanceMensual.Columns["NumeroFacturas"]!.HeaderText = "N° Facturas";
            _gridBalanceMensual.Columns["NumeroFacturas"]!.Width = 110;

            var ultimoPeriodo = balanceDetalle.LastOrDefault()?.Periodo;
            if (!string.IsNullOrWhiteSpace(ultimoPeriodo))
            {
                foreach (DataGridViewRow row in _gridBalanceMensual.Rows)
                {
                    if (row.DataBoundItem is BalanceMensualDto balance && balance.Periodo == ultimoPeriodo)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(240, 250, 240);
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
        }

        // Limpiar pagos antes de cargar facturas
        _gridBalancePagos.DataSource = null;
        _lblBalancePagosTitulo.Text = "💰 Pagos de Factura";

        // Cargar facturas del período
        _gridBalanceFacturas.DataSource = facturas;
        ConfigureBalanceFacturasGrid();
        SelectFirstBalanceFactura();

        var promedioFactura = resumen.FacturasEmitidas > 0
            ? resumen.TotalFacturado / resumen.FacturasEmitidas
            : 0m;

        _lblBalanceInfo.Text =
            $"Período seleccionado: {periodoNombre}\n" +
            $"Facturas emitidas: {resumen.FacturasEmitidas:N0}\n" +
            $"Promedio por factura: {promedioFactura:C0}\n" +
            $"Recaudo sobre facturación: {(resumen.TotalFacturado <= 0 ? 0 : (resumen.TotalRecaudado / resumen.TotalFacturado) * 100):N2}%";
    }

    private void OnBalanceFechaChanged()
    {
        _balanceFechaEspecifica = _dtpBalanceFecha.Value.Date;

        foreach (var (_, button) in _balancePeriodButtons)
        {
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(45, 55, 45);
            button.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 210);
        }

        LoadBalance();
    }

    private void OnBalanceDetalleSelected()
    {
        if (_gridBalanceMensual.CurrentRow?.DataBoundItem is not BalanceMensualDto detalle)
        {
            return;
        }

        if (!TryParsePeriodoRange(detalle.Periodo, out var inicio, out var fin))
        {
            return;
        }

        _gridBalancePagos.DataSource = null;
        _lblBalancePagosTitulo.Text = "💰 Pagos de Factura";

        var facturas = inicio == fin
            ? _balanceRepository.GetFacturas(inicio)
            : _balanceRepository.GetFacturas(inicio, fin);

        _lblBalanceFacturasTitulo.Text = $"📄 Facturas — {detalle.MesNombre}";
        _gridBalanceFacturas.DataSource = facturas;
        ConfigureBalanceFacturasGrid();
        SelectFirstBalanceFactura();
    }

    private static bool TryParsePeriodoRange(string periodo, out DateTime inicio, out DateTime fin)
    {
        inicio = default;
        fin = default;

        // yyyy-MM-dd (día)
        if (DateTime.TryParseExact(periodo, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var fecha))
        {
            inicio = fecha;
            fin = fecha;
            return true;
        }

        // yyyy-MM-Q1 o yyyy-MM-Q2 (quincena)
        var qIndex = periodo.IndexOf("-Q");
        if (qIndex == 7 && periodo.Length > 8)
        {
            var yearMonthPart = periodo[..7];
            var qNumber = periodo[(qIndex + 2)..];
            if (DateTime.TryParseExact(yearMonthPart, "yyyy-MM",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var mesQ))
            {
                if (qNumber == "1")
                {
                    inicio = new DateTime(mesQ.Year, mesQ.Month, 1);
                    fin = new DateTime(mesQ.Year, mesQ.Month, 15);
                    return true;
                }

                if (qNumber == "2")
                {
                    inicio = new DateTime(mesQ.Year, mesQ.Month, 16);
                    fin = new DateTime(mesQ.Year, mesQ.Month, DateTime.DaysInMonth(mesQ.Year, mesQ.Month));
                    return true;
                }
            }
        }

        // yyyy-MM (mes)
        if (DateTime.TryParseExact(periodo, "yyyy-MM",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var mes))
        {
            inicio = new DateTime(mes.Year, mes.Month, 1);
            fin = new DateTime(mes.Year, mes.Month, DateTime.DaysInMonth(mes.Year, mes.Month));
            return true;
        }

        // yyyy (año)
        if (periodo.Length == 4 && int.TryParse(periodo, out var anio) && anio is >= 2000 and <= 2100)
        {
            inicio = new DateTime(anio, 1, 1);
            fin = new DateTime(anio, 12, 31);
            return true;
        }

        return false;
    }

    private void ConfigureBalanceFacturasGrid()
    {
        if (_gridBalanceFacturas.Columns.Count == 0)
        {
            return;
        }

        _gridBalanceFacturas.Columns["Id"]!.Visible = false;
        _gridBalanceFacturas.Columns["Numero"]!.HeaderText = "N° Factura";
        _gridBalanceFacturas.Columns["Numero"]!.Width = 100;
        _gridBalanceFacturas.Columns["Fecha"]!.HeaderText = "Fecha";
        _gridBalanceFacturas.Columns["Fecha"]!.Width = 90;
        _gridBalanceFacturas.Columns["Cliente"]!.HeaderText = "Cliente";
        _gridBalanceFacturas.Columns["Total"]!.HeaderText = "Total";
        _gridBalanceFacturas.Columns["Total"]!.DefaultCellStyle.Format = "C0";
        _gridBalanceFacturas.Columns["Total"]!.Width = 100;
        _gridBalanceFacturas.Columns["Saldo"]!.HeaderText = "Saldo";
        _gridBalanceFacturas.Columns["Saldo"]!.DefaultCellStyle.Format = "C0";
        _gridBalanceFacturas.Columns["Saldo"]!.Width = 100;
        _gridBalanceFacturas.Columns["Estado"]!.HeaderText = "Estado";
        _gridBalanceFacturas.Columns["Estado"]!.Width = 80;

        foreach (DataGridViewRow row in _gridBalanceFacturas.Rows)
        {
            if (row.DataBoundItem is BalanceFacturaDto factura)
            {
                row.DefaultCellStyle.BackColor = factura.Estado switch
                {
                    "Pagada" => Color.FromArgb(232, 245, 232),
                    "Vencida" => Color.FromArgb(255, 235, 235),
                    _ => Color.White
                };
            }
        }
    }

    private void SelectFirstBalanceFactura()
    {
        if (_gridBalanceFacturas.Rows.Count > 0)
        {
            _gridBalanceFacturas.ClearSelection();
            _gridBalanceFacturas.Rows[0].Selected = true;
            _gridBalanceFacturas.CurrentCell = _gridBalanceFacturas.Rows[0].Cells[
                _gridBalanceFacturas.Columns["Numero"]!.Index];
            OnBalanceFacturaSelected();
        }
    }

    private void OnBalanceFacturaSelected()
    {
        if (_gridBalanceFacturas.CurrentRow?.DataBoundItem is not BalanceFacturaDto factura)
        {
            _gridBalancePagos.DataSource = null;
            _lblBalancePagosTitulo.Text = "💰 Pagos de Factura";
            return;
        }

        _lblBalancePagosTitulo.Text = $"💰 Pagos — {factura.Numero}";

        var pagos = _balanceRepository.GetPagosFactura(factura.Id);
        _gridBalancePagos.DataSource = pagos;

        if (_gridBalancePagos.Columns.Count > 0)
        {
            _gridBalancePagos.Columns["Id"]!.Visible = false;
            _gridBalancePagos.Columns["Fecha"]!.HeaderText = "Fecha";
            _gridBalancePagos.Columns["Fecha"]!.Width = 100;
            _gridBalancePagos.Columns["Valor"]!.HeaderText = "Valor";
            _gridBalancePagos.Columns["Valor"]!.DefaultCellStyle.Format = "C0";
            _gridBalancePagos.Columns["MetodoPago"]!.HeaderText = "Método";
            _gridBalancePagos.Columns["MetodoPago"]!.Width = 100;
            _gridBalancePagos.Columns["Referencia"]!.HeaderText = "Referencia";
            _gridBalancePagos.Columns["Notas"]!.HeaderText = "Notas";
        }
    }

    private static string GetBalancePeriodName(BalancePeriodo periodo)
    {
        return periodo switch
        {
            BalancePeriodo.Diario => "Diario",
            BalancePeriodo.Quincenal => "Quincenal",
            BalancePeriodo.Mensual => "Mensual",
            BalancePeriodo.Anual => "Anual",
            BalancePeriodo.Total => "Total",
            _ => "Mensual"
        };
    }

    private static string GetBalanceDetalleTitle(BalancePeriodo periodo)
    {
        return periodo switch
        {
            BalancePeriodo.Diario => "📈 Balance Diario (Últimos 7 Días)",
            BalancePeriodo.Quincenal => "📈 Balance Quincenal (Mes Actual)",
            BalancePeriodo.Mensual => "📈 Balance Mensual (Últimos 6 Meses)",
            BalancePeriodo.Anual => "📈 Balance Anual (Últimos 5 Años)",
            BalancePeriodo.Total => "📈 Balance Histórico Total",
            _ => "📈 Balance Mensual"
        };
    }

    private void PrintBalance()
    {
        var printDoc = new System.Drawing.Printing.PrintDocument
        {
            DocumentName = "Balance - AceitesPro Facturación"
        };
        printDoc.DefaultPageSettings.Landscape = true;

        printDoc.PrintPage += (_, e) =>
        {
            var g = e.Graphics!;
            var b = e.MarginBounds;
            var y = (float)b.Top;

            using var fontTitle = new Font("Segoe UI", 14, FontStyle.Bold);
            using var fontSub = new Font("Segoe UI", 9);
            using var fontSection = new Font("Segoe UI", 10, FontStyle.Bold);
            using var fontLabel = new Font("Segoe UI", 8, FontStyle.Bold);
            using var fontValue = new Font("Segoe UI", 11, FontStyle.Bold);
            using var fontHeader = new Font("Segoe UI", 7, FontStyle.Bold);
            using var fontCell = new Font("Segoe UI", 7);
            using var penLine = new Pen(Color.FromArgb(210, 210, 210));
            using var brushGreen = new SolidBrush(Color.FromArgb(11, 52, 22));

            // Header
            g.DrawString("AceitesPro \u2014 Balance Financiero", fontTitle, Brushes.Black, b.Left, y);
            y += fontTitle.GetHeight(g) + 2;
            var periodo = _lblBalanceDetalleTitulo.Text.Replace("\uD83D\uDCC8 ", "");
            g.DrawString($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm}  \u2022  {periodo}", fontSub, Brushes.Gray, b.Left, y);
            y += fontSub.GetHeight(g) + 8;
            g.DrawLine(Pens.DarkGray, b.Left, y, b.Right, y);
            y += 12;

            // Summary cards
            var cw = b.Width / 4f;
            DrawPrintCard(g, b.Left, y, "Total Facturado", _lblBalanceFacturado.Text, fontLabel, fontValue, brushGreen);
            DrawPrintCard(g, b.Left + cw, y, "Total Recaudado", _lblBalanceRecaudado.Text, fontLabel, fontValue, brushGreen);
            DrawPrintCard(g, b.Left + cw * 2, y, "Cuentas por Cobrar", _lblBalanceCuentasPorCobrar.Text, fontLabel, fontValue, brushGreen);
            DrawPrintCard(g, b.Left + cw * 3, y, "Balance Neto", _lblBalanceNeto.Text, fontLabel, fontValue, brushGreen);
            y += fontValue.GetHeight(g) + fontLabel.GetHeight(g) + 16;

            g.DrawLine(penLine, b.Left, y, b.Right, y);
            y += 10;

            // Balance detail grid
            var detalleTitle = _lblBalanceDetalleTitulo.Text.Replace("\uD83D\uDCC8 ", "");
            y = DrawPrintGrid(g, _gridBalanceMensual, detalleTitle, b, y, fontSection, fontHeader, fontCell, penLine);
            y += 10;

            // Facturas grid
            if (y < b.Bottom - 80)
            {
                var facturasTitle = _lblBalanceFacturasTitulo.Text.Replace("\uD83D\uDCC4 ", "");
                y = DrawPrintGrid(g, _gridBalanceFacturas, facturasTitle, b, y, fontSection, fontHeader, fontCell, penLine);
                y += 10;
            }

            // Pagos grid
            if (y < b.Bottom - 80 && _gridBalancePagos.Rows.Count > 0)
            {
                var pagosTitle = _lblBalancePagosTitulo.Text.Replace("\uD83D\uDCB0 ", "");
                y = DrawPrintGrid(g, _gridBalancePagos, pagosTitle, b, y, fontSection, fontHeader, fontCell, penLine);
                y += 10;
            }

            // Indicators
            if (y < b.Bottom - 50)
            {
                g.DrawLine(penLine, b.Left, y, b.Right, y);
                y += 8;
                g.DrawString("Indicadores del Per\u00edodo", fontSection, Brushes.Black, b.Left, y);
                y += fontSection.GetHeight(g) + 4;
                g.DrawString(_lblBalanceInfo.Text, fontSub, Brushes.DimGray,
                    new RectangleF(b.Left, y, b.Width, b.Bottom - y));
            }

            e.HasMorePages = false;
        };

        using var preview = new PrintPreviewDialog
        {
            Document = printDoc,
            WindowState = FormWindowState.Maximized
        };
        preview.ShowDialog(this);
    }

    private static void DrawPrintCard(Graphics g, float x, float y, string label, string value, Font fontLabel, Font fontValue, Brush valueBrush)
    {
        g.DrawString(label, fontLabel, Brushes.Gray, x + 4, y);
        g.DrawString(value, fontValue, valueBrush, x + 4, y + fontLabel.GetHeight(g) + 2);
    }

    private static float DrawPrintGrid(Graphics g, DataGridView grid, string title, Rectangle bounds, float y, Font fontSection, Font fontHeader, Font fontCell, Pen penLine)
    {
        if (grid.Rows.Count == 0)
        {
            return y;
        }

        g.DrawString(title, fontSection, Brushes.Black, bounds.Left, y);
        y += fontSection.GetHeight(g) + 6;

        var columns = new List<(string Header, int Index, float Weight)>();
        var totalWeight = 0f;
        foreach (DataGridViewColumn col in grid.Columns)
        {
            if (col.Visible)
            {
                columns.Add((col.HeaderText, col.Index, col.Width));
                totalWeight += col.Width;
            }
        }

        if (columns.Count == 0)
        {
            return y;
        }

        var availableWidth = (float)bounds.Width;
        var colWidths = columns.Select(c => c.Weight / totalWeight * availableWidth).ToArray();

        // Header row
        var headerHeight = fontHeader.GetHeight(g) + 8;
        using var headerBrush = new SolidBrush(Color.FromArgb(235, 238, 235));
        g.FillRectangle(headerBrush, bounds.Left, y, availableWidth, headerHeight);

        var x = (float)bounds.Left;
        for (var i = 0; i < columns.Count; i++)
        {
            g.DrawString(columns[i].Header, fontHeader, Brushes.Black, x + 3, y + 4);
            x += colWidths[i];
        }
        y += headerHeight;
        g.DrawLine(penLine, bounds.Left, y, bounds.Right, y);

        // Data rows
        var rowHeight = fontCell.GetHeight(g) + 6;
        var alternate = false;
        using var altBrush = new SolidBrush(Color.FromArgb(248, 250, 248));

        foreach (DataGridViewRow row in grid.Rows)
        {
            if (y + rowHeight > bounds.Bottom - 40)
            {
                break;
            }

            if (alternate)
            {
                g.FillRectangle(altBrush, bounds.Left, y, availableWidth, rowHeight);
            }

            x = bounds.Left;
            for (var i = 0; i < columns.Count; i++)
            {
                var cellValue = row.Cells[columns[i].Index].FormattedValue?.ToString() ?? "";
                g.DrawString(cellValue, fontCell, Brushes.Black, x + 3, y + 3);
                x += colWidths[i];
            }

            y += rowHeight;
            g.DrawLine(penLine, bounds.Left, y, bounds.Right, y);
            alternate = !alternate;
        }

        return y;
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
        _gridProductosInventario.SelectionChanged += (_, _) => OnProductoInventarioSelected();
        AddGridWithTopMargin(productosCard, _gridProductosInventario, 20);

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

        _lblMovimientosTitulo = new Label
        {
            Text = "📋 Movimientos Recientes",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        movimientosCard.Controls.Add(_lblMovimientosTitulo);

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
        AddGridWithTopMargin(movimientosCard, _gridMovimientos, 20);

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
        AddGridWithTopMargin(stockBajoCard, _gridStockBajo, 20);

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
            _gridProductosInventario.Columns["StockActual"]!.Width = 100;
            _gridProductosInventario.Columns["StockMinimo"]!.HeaderText = "Stock Mínimo";
            _gridProductosInventario.Columns["StockMinimo"]!.DefaultCellStyle.Format = "N0";
            _gridProductosInventario.Columns["StockMinimo"]!.Width = 110;
            _gridProductosInventario.Columns["PrecioBase"]!.HeaderText = "Precio";
            _gridProductosInventario.Columns["PrecioBase"]!.DefaultCellStyle.Format = "C0";
            _gridProductosInventario.Columns["ValorStock"]!.HeaderText = "Valor Stock";
            _gridProductosInventario.Columns["ValorStock"]!.DefaultCellStyle.Format = "C0";
            _gridProductosInventario.Columns["ValorStock"]!.Width = 120;
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
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 230);
                            break;
                        case "Medio":
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 230);
                            break;
                    }
                }
            }
        }
    }

    private void OnProductoInventarioSelected()
    {
        if (_gridProductosInventario.CurrentRow?.DataBoundItem is ProductoInventarioDto producto)
        {
            _lblMovimientosTitulo.Text = $"📋 Movimientos — {producto.Nombre}";
            LoadMovimientos(producto.Id);
        }
        else
        {
            _lblMovimientosTitulo.Text = "📋 Movimientos Recientes";
            LoadMovimientos();
        }
    }

    private void LoadMovimientos(long? productoId = null)
    {
        var movimientos = _inventarioRepository.GetMovimientos(50, productoId);
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
        public decimal TotalLinea { get; set; }
    }
}
