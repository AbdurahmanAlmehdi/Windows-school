using System.Drawing.Drawing2D;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    // Header
    private Panel panelHeader = null!;
    private Label lblBrand = null!;
    private Button btnLogout = null!;

    // Tab control
    private TabControl tabMain = null!;
    private TabPage tabDashboard = null!;
    private TabPage tabReservations = null!;
    private TabPage tabRooms = null!;
    private TabPage tabRestaurant = null!;
    private TabPage tabReports = null!;
    private TabPage tabUsers = null!;

    // Users tab controls
    private TabControl userSubTabs = null!;
    private TabPage subTabUsers = null!;
    private TabPage subTabRoles = null!;
    private DataGridView dgvUsers = null!;
    private DataGridView dgvRoles = null!;
    private Button btnAddUser = null!;
    private Button btnEditUser = null!;
    private Button btnRemoveUser = null!;
    private Button btnAddRole = null!;
    private Button btnEditRole = null!;
    private Button btnRemoveRole = null!;

    // Dashboard - Welcome bar
    private Panel pnlWelcomeBar = null!;
    private Label lblGreeting = null!;
    private Label lblDate = null!;
    private Button btnRefreshDash = null!;

    // Dashboard - KPI cards
    private TableLayoutPanel tblKPI = null!;
    private Panel pnlOccupancy = null!;
    private Label lblOccupancyValue = null!;
    private Label lblOccupancyTitle = null!;
    private Panel pnlAvailable = null!;
    private Label lblAvailableValue = null!;
    private Label lblAvailableTitle = null!;
    private Panel pnlOccupied = null!;
    private Label lblOccupiedValue = null!;
    private Label lblOccupiedTitle = null!;
    private Panel pnlOOS = null!;
    private Label lblOOSValue = null!;
    private Label lblOOSTitle = null!;

    // Dashboard - Action panels
    private TableLayoutPanel tblActions = null!;
    private Panel pnlArrivalsCard = null!;
    private Label lblArrivalsHeader = null!;
    private FlowLayoutPanel flpArrivals = null!;
    private Panel pnlDeparturesCard = null!;
    private Label lblDeparturesHeader = null!;
    private FlowLayoutPanel flpDepartures = null!;
    private Panel pnlHousekeepingCard = null!;
    private Label lblHousekeepingHeader = null!;
    private FlowLayoutPanel flpHousekeeping = null!;
    private Panel pnlOrdersCard = null!;
    private Label lblOrdersHeader = null!;
    private FlowLayoutPanel flpActiveOrders = null!;

    // Reservations controls
    private ComboBox cmbResFilter = null!;
    private Label lblResStatus = null!;
    private DataGridView dgvReservations = null!;
    private Button btnCheckIn = null!;
    private Button btnCheckOut = null!;
    private Button btnCancelRes = null!;
    private Button btnNewReservation = null!;
    // Reservation KPI labels
    private Label lblResKpiArrivals = null!;
    private Label lblResKpiActive = null!;
    private Label lblResKpiPending = null!;
    private Label lblResKpiCompleted = null!;

    // Finances tab
    private TabPage tabFinances = null!;
    private Label lblFinTotalRev = null!;
    private Label lblFinUnpaid = null!;
    private Label lblFinPaidToday = null!;
    private Label lblFinOutstanding = null!;
    private ComboBox cmbFinFilter = null!;
    private DataGridView dgvInvoices = null!;
    private Button btnViewInvoice = null!;
    private Button btnMarkPaid = null!;

    // Rooms controls
    private ComboBox cmbRoomTypeFilter = null!;
    private ComboBox cmbRoomStatusFilter = null!;
    private FlowLayoutPanel flpRooms = null!;
    // Room KPI labels
    private Label lblRoomOccupancyValue = null!;
    private Label lblRoomAvailableValue = null!;
    private Label lblRoomCleaningValue = null!;
    private Label lblRoomOOSValue = null!;
    // Room detail panel
    private Panel pnlRoomDetail = null!;
    private Label lblRoomDetailTitle = null!;
    private Label lblRoomDetailType = null!;
    private Label lblRoomDetailRate = null!;
    private Label lblRoomDetailStatus = null!;
    private Label lblRoomDetailGuest = null!;
    private Label lblRoomDetailMaintenance = null!;
    // Room condition action buttons
    private Button btnMarkClean = null!;
    private Button btnMarkNeedsCleaning = null!;
    private Button btnMarkOutOfService = null!;
    // Room management buttons (manager-only)
    private Button btnAddRoom = null!;
    private Button btnEditRoom = null!;
    private Button btnRemoveRoom = null!;

    // Restaurant controls
    private TabControl restSubTabs = null!;
    private TabPage subTabOrder = null!;
    private TabPage subTabActive = null!;
    private FlowLayoutPanel flpMenuCards = null!;
    private ComboBox cmbStay = null!;
    private Button btnPlaceOrder = null!;
    private DataGridView dgvOrders = null!;
    private Button btnAdvanceOrder = null!;
    private Button btnCancelOrder = null!;
    // Restaurant KPI labels
    private Label lblRestKpiActive = null!;
    private Label lblRestKpiReady = null!;
    private Label lblRestKpiRevenue = null!;
    private Label lblRestKpiItems = null!;
    // Restaurant category tabs
    private FlowLayoutPanel flpCategoryTabs = null!;
    private string _selectedMenuCategory = "All";
    // Restaurant order filter
    private ComboBox cmbOrderFilter = null!;
    // Restaurant new order card controls
    private DataGridView dgvCurrentOrderLines = null!;
    private Label lblRunningTotal = null!;
    private Button btnClearOrder = null!;
    // Restaurant order detail panel
    private Panel pnlOrderDetail = null!;
    private DataGridView dgvOrderLines = null!;
    private Label lblOrderDetailGuest = null!;
    private Label lblOrderDetailStatus = null!;
    private Panel pnlStatusProgression = null!;
    private Label lblOrderTotal = null!;
    // Restaurant menu management (manager only)
    private Button btnAddMenuItem = null!;
    private Button btnEditMenuItem = null!;
    private Button btnToggleAvail = null!;
    private Button btnRemoveMenuItem = null!;
    private Button btnAddItemsToOrder = null!;

    // Reports controls
    private Button btnRefreshReports = null!;
    private Panel pnlOccReport = null!;
    private Label lblOccReportValue = null!;
    private Panel pnlAvgStay = null!;
    private Label lblAvgStayValue = null!;
    private Panel pnlRepeatGuest = null!;
    private Label lblRepeatGuestValue = null!;
    private GroupBox grpRevRoom = null!;
    private Label lblRevRoom = null!;
    private GroupBox grpRevRestaurant = null!;
    private Label lblRevRestaurant = null!;
    private GroupBox grpTopItems = null!;
    private Label lblTopItems = null!;
    private Panel pnlTotalRevenue = null!;
    private Label lblTotalRevenueValue = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        // === Form ===
        Text = "The Grand Hotel - Management System";
        Size = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Surface;
        MinimumSize = new Size(1024, 700);

        // === Header ===
        panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = AppColors.Primary
        };

        lblBrand = new Label
        {
            Text = "THE GRAND HOTEL",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(16, 10)
        };

        btnLogout = new Button
        {
            Text = "Logout",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(60, 255, 255, 255),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(90, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += BtnLogout_Click;

        panelHeader.Controls.Add(lblBrand);
        panelHeader.Controls.Add(btnLogout);

        // === TabControl ===
        tabMain = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11),
            Padding = new Point(16, 6)
        };

        tabDashboard = new TabPage("Dashboard") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabReservations = new TabPage("Reservations") { BackColor = AppColors.Surface, Padding = new Padding(0) };
        tabRooms = new TabPage("Rooms") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabRestaurant = new TabPage("Restaurant") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabFinances = new TabPage("Finances") { BackColor = AppColors.Surface, Padding = new Padding(0) };
        tabReports = new TabPage("Reports") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabUsers = new TabPage("Users") { BackColor = AppColors.Surface, Padding = new Padding(0) };

        tabMain.TabPages.Add(tabDashboard);
        tabMain.TabPages.Add(tabReservations);
        tabMain.TabPages.Add(tabRooms);
        tabMain.TabPages.Add(tabRestaurant);
        tabMain.TabPages.Add(tabFinances);
        tabMain.TabPages.Add(tabReports);
        tabMain.TabPages.Add(tabUsers);
        tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

        InitDashboardTab();
        InitReservationsTab();
        InitRoomsTab();
        InitRestaurantTab();
        InitFinancesTab();
        InitReportsTab();
        InitUsersTab();

        Controls.Add(tabMain);
        Controls.Add(panelHeader);

        ResumeLayout(false);
        PerformLayout();
    }

    private Panel CreateMetricCard(string title, Color bgColor, out Label valueLabel, out Label titleLabel, Size size)
    {
        var panel = new Panel
        {
            Size = size,
            BackColor = bgColor,
            Padding = new Padding(12)
        };
        panel.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };

        titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(200, 255, 255, 255),
            AutoSize = true,
            Location = new Point(14, 14)
        };

        valueLabel = new Label
        {
            Text = "0",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(14, 40)
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(valueLabel);
        return panel;
    }

    private void InitDashboardTab()
    {
        tabDashboard.Padding = new Padding(0);

        // === Welcome Bar ===
        pnlWelcomeBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = AppColors.Primary,
            Padding = new Padding(20, 0, 20, 0)
        };

        lblGreeting = new Label
        {
            Text = "Welcome",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 8)
        };

        lblDate = new Label
        {
            Text = DateTime.Today.ToString("dddd, MMM dd, yyyy"),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = true,
            Location = new Point(20, 40)
        };

        btnRefreshDash = new Button
        {
            Text = "Refresh",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(50, 255, 255, 255),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand
        };
        btnRefreshDash.FlatAppearance.BorderSize = 0;
        btnRefreshDash.Click += (s, e) => RefreshDashboard();

        pnlWelcomeBar.Controls.Add(lblGreeting);
        pnlWelcomeBar.Controls.Add(lblDate);
        pnlWelcomeBar.Controls.Add(btnRefreshDash);
        pnlWelcomeBar.Resize += (s, e) =>
        {
            btnRefreshDash.Location = new Point(pnlWelcomeBar.Width - btnRefreshDash.Width - 20, 15);
        };

        // === KPI Row ===
        tblKPI = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 140,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 12, 12, 0)
        };
        tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        pnlOccupancy = CreateKPICard("Occupancy", "0%", AppColors.Accent, out lblOccupancyValue, out lblOccupancyTitle, true);
        pnlAvailable = CreateKPICard("Available", "0", AppColors.Tertiary, out lblAvailableValue, out lblAvailableTitle, false);
        pnlOccupied = CreateKPICard("Occupied", "0", AppColors.Primary, out lblOccupiedValue, out lblOccupiedTitle, false);
        pnlOOS = CreateKPICard("Out of Service", "0", AppColors.StatusOOS, out lblOOSValue, out lblOOSTitle, false);

        tblKPI.Controls.Add(pnlOccupancy, 0, 0);
        tblKPI.Controls.Add(pnlAvailable, 1, 0);
        tblKPI.Controls.Add(pnlOccupied, 2, 0);
        tblKPI.Controls.Add(pnlOOS, 3, 0);

        // === Action Grid (2x2) ===
        tblActions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 12)
        };
        tblActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tblActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tblActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        tblActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        pnlArrivalsCard = CreateDashboardCard("Arrivals Today", AppColors.Tertiary, out lblArrivalsHeader, out flpArrivals);
        pnlDeparturesCard = CreateDashboardCard("Departures Today", AppColors.Primary, out lblDeparturesHeader, out flpDepartures);
        pnlHousekeepingCard = CreateDashboardCard("Housekeeping", AppColors.StatusClean, out lblHousekeepingHeader, out flpHousekeeping);
        pnlOrdersCard = CreateDashboardCard("Active Orders", AppColors.Accent, out lblOrdersHeader, out flpActiveOrders);

        tblActions.Controls.Add(pnlArrivalsCard, 0, 0);
        tblActions.Controls.Add(pnlDeparturesCard, 1, 0);
        tblActions.Controls.Add(pnlHousekeepingCard, 0, 1);
        tblActions.Controls.Add(pnlOrdersCard, 1, 1);

        // Add in reverse dock order (Fill first, then Top items)
        tabDashboard.Controls.Add(tblActions);
        tabDashboard.Controls.Add(tblKPI);
        tabDashboard.Controls.Add(pnlWelcomeBar);
    }

    private Panel CreateKPICard(string title, string defaultValue, Color accentColor,
        out Label valueLabel, out Label titleLabel, bool hasArc)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            BackColor = Color.Transparent
        };
        panel.Paint += DrawingUtilities.PaintCardBackground;

        titleLabel = new Label
        {
            Text = title.ToUpper(),
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = AppColors.Gray500,
            AutoSize = true,
            Location = new Point(16, 16)
        };

        valueLabel = new Label
        {
            Text = defaultValue,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = accentColor,
            AutoSize = true,
            Location = new Point(16, 42)
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(valueLabel);

        if (hasArc)
        {
            panel.Paint += (s, e) =>
            {
                if (float.TryParse(lblOccupancyValue.Text.TrimEnd('%'), out var pct))
                {
                    var arcRect = new Rectangle(panel.Width - 90, 20, 70, 70);
                    DrawingUtilities.DrawProgressArc(e.Graphics, arcRect, pct,
                        AppColors.Accent, AppColors.Gray200, 6);
                }
            };
        }

        return panel;
    }

    private Panel CreateDashboardCard(string title, Color headerColor,
        out Label headerLabel, out FlowLayoutPanel contentPanel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            BackColor = Color.Transparent
        };
        card.Paint += DrawingUtilities.PaintCardBackground;

        var headerBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.Transparent
        };

        var hdrLbl = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(16, 10)
        };
        headerLabel = hdrLbl;

        var badge = new Label
        {
            Text = "0",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = headerColor,
            AutoSize = false,
            Size = new Size(26, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(hdrLbl.Right + 8, 12)
        };
        badge.Paint += (s, e) =>
        {
            using var path = DrawingUtilities.CreateRoundedRect(new Rectangle(0, 0, badge.Width - 1, badge.Height - 1), 8);
            using var brush = new SolidBrush(headerColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(brush, path);
            TextRenderer.DrawText(e.Graphics, badge.Text, badge.Font, new Rectangle(0, 0, badge.Width, badge.Height),
                Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        badge.Tag = "badge";

        headerBar.Controls.Add(hdrLbl);
        headerBar.Controls.Add(badge);

        hdrLbl.TextChanged += (s, e) =>
        {
            badge.Location = new Point(hdrLbl.Right + 8, 12);
        };

        contentPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(12, 4, 12, 8)
        };

        card.Controls.Add(contentPanel);
        card.Controls.Add(headerBar);
        return card;
    }

    private void InitReservationsTab()
    {
        // === Navy Header Bar ===
        var pnlResHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppColors.Primary
        };

        var lblResTitle = new Label
        {
            Text = "Reservations",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 12)
        };
        pnlResHeader.Controls.Add(lblResTitle);

        btnNewReservation = new Button
        {
            Text = "+ New Reservation",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(170, 32),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnNewReservation.FlatAppearance.BorderSize = 0;
        btnNewReservation.Click += BtnNewReservation_Click;
        pnlResHeader.Controls.Add(btnNewReservation);

        cmbResFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(150, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cmbResFilter.Items.AddRange(new object[] { "All", "Confirmed", "CheckedIn", "Completed", "Cancelled", "Pending" });
        cmbResFilter.SelectedIndex = 0;
        cmbResFilter.SelectedIndexChanged += CmbResFilter_Changed;
        pnlResHeader.Controls.Add(cmbResFilter);

        var lblFilterLbl = new Label
        {
            Text = "Filter:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        pnlResHeader.Controls.Add(lblFilterLbl);

        pnlResHeader.Resize += (s, e) =>
        {
            btnNewReservation.Location = new Point(pnlResHeader.Width - btnNewReservation.Width - 20, 12);
            cmbResFilter.Location = new Point(btnNewReservation.Left - cmbResFilter.Width - 20, 14);
            lblFilterLbl.Location = new Point(cmbResFilter.Left - 45, 18);
        };

        // === KPI Row ===
        var tblResKPI = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 0)
        };
        tblResKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblResKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblResKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblResKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        Label _discard;
        var kpiArr = CreateKPICard("Arrivals Today", "0", AppColors.Tertiary, out lblResKpiArrivals, out _discard, false);
        var kpiAct = CreateKPICard("Active Stays", "0", AppColors.Primary, out lblResKpiActive, out _discard, false);
        var kpiPend = CreateKPICard("Pending Res.", "0", AppColors.Accent, out lblResKpiPending, out _discard, false);
        var kpiComp = CreateKPICard("Completed Today", "0", AppColors.Gray500, out lblResKpiCompleted, out _discard, false);

        tblResKPI.Controls.Add(kpiArr, 0, 0);
        tblResKPI.Controls.Add(kpiAct, 1, 0);
        tblResKPI.Controls.Add(kpiPend, 2, 0);
        tblResKPI.Controls.Add(kpiComp, 3, 0);

        // === Grid Container ===
        var pnlGrid = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 12)
        };

        lblResStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Tertiary,
            Dock = DockStyle.Top,
            Height = 22,
            TextAlign = ContentAlignment.MiddleLeft
        };

        dgvReservations = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 255) }
        };
        dgvReservations.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvReservations.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvReservations.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvReservations.EnableHeadersVisualStyles = false;
        dgvReservations.Columns.Add("Room", "Room #");
        dgvReservations.Columns.Add("Guest", "Guest");
        dgvReservations.Columns.Add("Phone", "Phone");
        dgvReservations.Columns.Add("Passport", "Passport");
        dgvReservations.Columns.Add("Party", "Party");
        dgvReservations.Columns.Add("CheckIn", "Check In");
        dgvReservations.Columns.Add("CheckOut", "Check Out");
        dgvReservations.Columns.Add("Status", "Status");
        dgvReservations.SelectionChanged += DgvReservations_SelectionChanged;

        var statusColumnIndex = dgvReservations.Columns.Count - 1;
        dgvReservations.CellPainting += (s, e) =>
        {
            if (e.ColumnIndex == statusColumnIndex && e.RowIndex >= 0 && e.Value != null)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                var statusText = e.Value.ToString()!;
                if (Enum.TryParse<ReservationStatus>(statusText, out var status))
                {
                    var color = AppColors.GetReservationStatusColor(status);
                    var badgeRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + 6,
                        e.CellBounds.Width - 12, e.CellBounds.Height - 12);
                    using var brush = new SolidBrush(color);
                    e.Graphics!.FillRectangle(brush, badgeRect);
                    TextRenderer.DrawText(e.Graphics, statusText, new Font("Segoe UI", 8, FontStyle.Bold),
                        badgeRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        };

        // Action buttons panel
        var pnlResActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = AppColors.Surface,
            Padding = new Padding(0, 8, 0, 0)
        };

        btnCheckIn = new Button
        {
            Text = "Check In",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Enabled = false,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnCheckIn.FlatAppearance.BorderSize = 0;
        btnCheckIn.Click += BtnCheckIn_Click;

        btnCheckOut = new Button
        {
            Text = "Check Out",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Enabled = false,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnCheckOut.FlatAppearance.BorderSize = 0;
        btnCheckOut.Click += BtnCheckOut_Click;

        btnCancelRes = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnCancelRes.FlatAppearance.BorderSize = 0;
        btnCancelRes.Click += BtnCancelRes_Click;

        pnlResActions.Controls.AddRange(new Control[] { btnCheckIn, btnCheckOut, btnCancelRes });

        // Order matters: Fill must be added last so it gets the remaining space
        pnlGrid.Controls.Add(dgvReservations);
        pnlGrid.Controls.Add(pnlResActions);
        pnlGrid.Controls.Add(lblResStatus);

        // Add in reverse dock order
        tabReservations.Controls.Add(pnlGrid);
        tabReservations.Controls.Add(tblResKPI);
        tabReservations.Controls.Add(pnlResHeader);
    }

    private void InitRoomsTab()
    {
        tabRooms.Padding = new Padding(0);

        // === Navy Header Bar ===
        var pnlRoomHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppColors.Primary
        };

        var lblRoomsTitle = new Label
        {
            Text = "Room Management",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 12)
        };
        pnlRoomHeader.Controls.Add(lblRoomsTitle);

        cmbRoomStatusFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(150, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cmbRoomStatusFilter.Items.AddRange(new object[] { "All Status", "Available", "Occupied", "Needs Cleaning", "Out of Service" });
        cmbRoomStatusFilter.SelectedIndex = 0;
        cmbRoomStatusFilter.SelectedIndexChanged += CmbRoomFilter_Changed;
        pnlRoomHeader.Controls.Add(cmbRoomStatusFilter);

        cmbRoomTypeFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(120, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cmbRoomTypeFilter.Items.Add("All Types");
        foreach (var rt in Enum.GetValues<RoomType>())
            cmbRoomTypeFilter.Items.Add(rt.ToString());
        cmbRoomTypeFilter.SelectedIndex = 0;
        cmbRoomTypeFilter.SelectedIndexChanged += CmbRoomFilter_Changed;
        pnlRoomHeader.Controls.Add(cmbRoomTypeFilter);

        var lblTypeLbl = new Label
        {
            Text = "Type:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        pnlRoomHeader.Controls.Add(lblTypeLbl);

        var lblStatusLbl = new Label
        {
            Text = "Status:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        pnlRoomHeader.Controls.Add(lblStatusLbl);

        pnlRoomHeader.Resize += (s, e) =>
        {
            cmbRoomStatusFilter.Location = new Point(pnlRoomHeader.Width - cmbRoomStatusFilter.Width - 20, 14);
            lblStatusLbl.Location = new Point(cmbRoomStatusFilter.Left - 55, 18);
            cmbRoomTypeFilter.Location = new Point(lblStatusLbl.Left - cmbRoomTypeFilter.Width - 10, 14);
            lblTypeLbl.Location = new Point(cmbRoomTypeFilter.Left - 45, 18);
        };

        // === KPI Row ===
        var tblRoomKPI = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 0)
        };
        tblRoomKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblRoomKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblRoomKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblRoomKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        Label _rd;
        var kpiOcc = CreateKPICard("Occupancy Rate", "0%", AppColors.Accent, out lblRoomOccupancyValue, out _rd, false);
        var kpiAvail = CreateKPICard("Available", "0", AppColors.Tertiary, out lblRoomAvailableValue, out _rd, false);
        var kpiClean = CreateKPICard("Needs Cleaning", "0", AppColors.StatusClean, out lblRoomCleaningValue, out _rd, false);
        var kpiOOS = CreateKPICard("Out of Service", "0", AppColors.StatusOOS, out lblRoomOOSValue, out _rd, false);

        tblRoomKPI.Controls.Add(kpiOcc, 0, 0);
        tblRoomKPI.Controls.Add(kpiAvail, 1, 0);
        tblRoomKPI.Controls.Add(kpiClean, 2, 0);
        tblRoomKPI.Controls.Add(kpiOOS, 3, 0);

        // === SplitContainer ===
        var splitRooms = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 620,
            Orientation = Orientation.Vertical,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 12)
        };

        // LEFT: Room cards + manager buttons
        flpRooms = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppColors.Surface,
            Padding = new Padding(4)
        };

        var pnlRoomMgmtButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };

        btnAddRoom = new Button
        {
            Text = "Add Room",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 34),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnAddRoom.FlatAppearance.BorderSize = 0;
        btnAddRoom.Click += BtnAddRoom_Click;

        btnEditRoom = new Button
        {
            Text = "Edit Room",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 34),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnEditRoom.FlatAppearance.BorderSize = 0;
        btnEditRoom.Click += BtnEditRoom_Click;

        btnRemoveRoom = new Button
        {
            Text = "Remove Room",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 34),
            Cursor = Cursors.Hand
        };
        btnRemoveRoom.FlatAppearance.BorderSize = 0;
        btnRemoveRoom.Click += BtnRemoveRoom_Click;

        pnlRoomMgmtButtons.Controls.AddRange(new Control[] { btnAddRoom, btnEditRoom, btnRemoveRoom });

        splitRooms.Panel1.Controls.Add(flpRooms);
        splitRooms.Panel1.Controls.Add(pnlRoomMgmtButtons);

        // RIGHT: Room detail panel
        pnlRoomDetail = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(8)
        };
        pnlRoomDetail.Paint += DrawingUtilities.PaintCardBackground;

        lblRoomDetailTitle = new Label
        {
            Text = "Select a room to view details",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(20, 16)
        };

        lblRoomDetailType = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 11),
            ForeColor = AppColors.Gray600,
            AutoSize = true,
            Location = new Point(20, 48)
        };

        lblRoomDetailRate = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 76)
        };

        lblRoomDetailStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(200, 26),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 110),
            Visible = false
        };

        lblRoomDetailGuest = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(20, 146),
            Visible = false
        };

        lblRoomDetailMaintenance = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10),
            ForeColor = AppColors.Gray500,
            AutoSize = true,
            MaximumSize = new Size(350, 0),
            Location = new Point(20, 176),
            Visible = false
        };

        // Condition action buttons
        btnMarkClean = new Button
        {
            Text = "Mark Clean",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 36),
            Location = new Point(20, 220),
            Cursor = Cursors.Hand,
            Visible = false
        };
        btnMarkClean.FlatAppearance.BorderSize = 0;
        btnMarkClean.Click += BtnRoomMarkClean_Click;

        btnMarkNeedsCleaning = new Button
        {
            Text = "Needs Cleaning",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.StatusClean,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(150, 36),
            Location = new Point(158, 220),
            Cursor = Cursors.Hand,
            Visible = false
        };
        btnMarkNeedsCleaning.FlatAppearance.BorderSize = 0;
        btnMarkNeedsCleaning.Click += BtnRoomMarkNeedsCleaning_Click;

        btnMarkOutOfService = new Button
        {
            Text = "Out of Service",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(140, 36),
            Location = new Point(20, 264),
            Cursor = Cursors.Hand,
            Visible = false
        };
        btnMarkOutOfService.FlatAppearance.BorderSize = 0;
        btnMarkOutOfService.Click += BtnRoomMarkOutOfService_Click;

        pnlRoomDetail.Controls.AddRange(new Control[] {
            lblRoomDetailTitle, lblRoomDetailType, lblRoomDetailRate,
            lblRoomDetailStatus, lblRoomDetailGuest, lblRoomDetailMaintenance,
            btnMarkClean, btnMarkNeedsCleaning, btnMarkOutOfService
        });

        splitRooms.Panel2.Controls.Add(pnlRoomDetail);

        // Add in reverse dock order
        tabRooms.Controls.Add(splitRooms);
        tabRooms.Controls.Add(tblRoomKPI);
        tabRooms.Controls.Add(pnlRoomHeader);
    }

    private void InitRestaurantTab()
    {
        tabRestaurant.Padding = new Padding(0);

        // === Navy Header Bar ===
        var pnlRestHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppColors.Primary
        };
        var lblRestTitle = new Label
        {
            Text = "Restaurant",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 12)
        };
        pnlRestHeader.Controls.Add(lblRestTitle);

        // === KPI Row ===
        var tblRestKPI = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 0)
        };
        tblRestKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblRestKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblRestKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblRestKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        Label _rd;
        var kpiActive = CreateKPICard("Active Orders", "0", AppColors.Accent, out lblRestKpiActive, out _rd, false);
        var kpiReady = CreateKPICard("Awaiting Pickup", "0", AppColors.Tertiary, out lblRestKpiReady, out _rd, false);
        var kpiRevenue = CreateKPICard("Today Revenue", "$0", AppColors.Primary, out lblRestKpiRevenue, out _rd, false);
        var kpiItems = CreateKPICard("Menu Items", "0", AppColors.Gray500, out lblRestKpiItems, out _rd, false);

        tblRestKPI.Controls.Add(kpiActive, 0, 0);
        tblRestKPI.Controls.Add(kpiReady, 1, 0);
        tblRestKPI.Controls.Add(kpiRevenue, 2, 0);
        tblRestKPI.Controls.Add(kpiItems, 3, 0);

        // === Nested sub-tabs ===
        restSubTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Point(16, 8)
        };
        subTabOrder = new TabPage("Place Orders") { BackColor = AppColors.Surface, Padding = new Padding(12, 8, 12, 12) };
        subTabActive = new TabPage("Active Orders") { BackColor = AppColors.Surface, Padding = new Padding(12, 8, 12, 12) };
        restSubTabs.TabPages.Add(subTabOrder);
        restSubTabs.TabPages.Add(subTabActive);

        BuildPlaceOrdersSubTab(subTabOrder);
        BuildActiveOrdersSubTab(subTabActive);

        // Add to tab in reverse dock order
        tabRestaurant.Controls.Add(restSubTabs);
        tabRestaurant.Controls.Add(tblRestKPI);
        tabRestaurant.Controls.Add(pnlRestHeader);
    }

    private void BuildPlaceOrdersSubTab(TabPage tab)
    {
        // Layout: right sidebar (order builder) docked at fixed width; main area fills the rest.

        // --- Right: order builder card (built first so it docks before main fills) ---
        var pnlNewOrderCard = new Panel
        {
            Dock = DockStyle.Right,
            Width = 400,
            BackColor = Color.Transparent,
            Padding = new Padding(8),
            AutoScroll = true,
            AutoScrollMinSize = new Size(0, 480)
        };
        pnlNewOrderCard.Paint += DrawingUtilities.PaintCardBackground;

        var lblNewOrderTitle = new Label
        {
            Text = "NEW ORDER",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(16, 12)
        };

        var lblStay2 = new Label { Text = "Stay:", Font = new Font("Segoe UI", 10), Location = new Point(16, 44), AutoSize = true };
        cmbStay = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 41),
            Size = new Size(316, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        dgvCurrentOrderLines = new DataGridView
        {
            Location = new Point(16, 84),
            Size = new Size(360, 280),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9),
            ScrollBars = ScrollBars.Vertical
        };
        dgvCurrentOrderLines.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvCurrentOrderLines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvCurrentOrderLines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvCurrentOrderLines.EnableHeadersVisualStyles = false;
        dgvCurrentOrderLines.Columns.Add("Item", "Item");
        dgvCurrentOrderLines.Columns.Add("Qty", "Qty");
        dgvCurrentOrderLines.Columns["Qty"].Width = 40;
        dgvCurrentOrderLines.Columns.Add("Total", "Total");
        dgvCurrentOrderLines.Columns["Total"].Width = 70;
        var removeCol = new DataGridViewButtonColumn
        {
            Name = "Remove",
            Text = "X",
            UseColumnTextForButtonValue = true,
            Width = 30,
            FlatStyle = FlatStyle.Flat
        };
        dgvCurrentOrderLines.Columns.Add(removeCol);
        dgvCurrentOrderLines.CellClick += DgvCurrentOrderLines_CellClick;

        lblRunningTotal = new Label
        {
            Text = "Total: $0.00",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(16, 374)
        };

        btnPlaceOrder = new Button
        {
            Text = "Place Order",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(160, 40),
            Location = new Point(16, 402),
            Cursor = Cursors.Hand
        };
        btnPlaceOrder.FlatAppearance.BorderSize = 0;
        btnPlaceOrder.Click += BtnPlaceOrder_Click;

        btnClearOrder = new Button
        {
            Text = "Clear",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Gray400,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(90, 40),
            Location = new Point(184, 402),
            Cursor = Cursors.Hand
        };
        btnClearOrder.FlatAppearance.BorderSize = 0;
        btnClearOrder.Click += BtnClearOrder_Click;

        pnlNewOrderCard.Controls.AddRange(new Control[] {
            lblNewOrderTitle, lblStay2, cmbStay,
            dgvCurrentOrderLines, lblRunningTotal,
            btnPlaceOrder, btnClearOrder
        });

        // --- Main: categories + cards + menu management (fills remaining space) ---
        var pnlMain = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Surface,
            Padding = new Padding(0, 0, 8, 0)
        };

        flpCategoryTabs = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 4)
        };

        flpMenuCards = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            BackColor = AppColors.Surface,
            Padding = new Padding(0, 8, 0, 8)
        };

        // Menu management buttons (manager-only, kept in same spot at bottom)
        var pnlMenuActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };

        btnAddMenuItem = new Button
        {
            Text = "Add Item",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(90, 32),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 4, 0)
        };
        btnAddMenuItem.FlatAppearance.BorderSize = 0;
        btnAddMenuItem.Click += BtnAddMenuItem_Click;

        btnEditMenuItem = new Button
        {
            Text = "Edit Item",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(90, 32),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 4, 0)
        };
        btnEditMenuItem.FlatAppearance.BorderSize = 0;
        btnEditMenuItem.Click += BtnEditMenuItem_Click;

        btnToggleAvail = new Button
        {
            Text = "Toggle Avail.",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 32),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 4, 0)
        };
        btnToggleAvail.FlatAppearance.BorderSize = 0;
        btnToggleAvail.Click += BtnToggleAvail_Click;

        btnRemoveMenuItem = new Button
        {
            Text = "Remove",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 32),
            Cursor = Cursors.Hand
        };
        btnRemoveMenuItem.FlatAppearance.BorderSize = 0;
        btnRemoveMenuItem.Click += BtnRemoveMenuItem_Click;

        pnlMenuActions.Controls.AddRange(new Control[] { btnAddMenuItem, btnEditMenuItem, btnToggleAvail, btnRemoveMenuItem });

        // Order matters: Fill must be added LAST so it gets the remaining space.
        pnlMain.Controls.Add(flpMenuCards);
        pnlMain.Controls.Add(pnlMenuActions);
        pnlMain.Controls.Add(flpCategoryTabs);

        // Add to tab — Right docks first so it claims its width, then Fill gets the rest.
        tab.Controls.Add(pnlMain);
        tab.Controls.Add(pnlNewOrderCard);
    }

    private void BuildActiveOrdersSubTab(TabPage tab)
    {
        // Layout: right sidebar (order detail + action buttons) docked at fixed width; orders grid fills.

        // --- Right: detail panel + action buttons (built first so it docks before main fills) ---
        var pnlOrderRight = new Panel
        {
            Dock = DockStyle.Right,
            Width = 480,
            BackColor = AppColors.Surface
        };

        // --- Left: filter + orders grid (will be added with Dock=Fill at the end) ---
        var pnlOrdersLeft = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface };

        var pnlFilterRow = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = AppColors.Surface
        };
        var lblOrdFilterLbl = new Label
        {
            Text = "Status:",
            Font = new Font("Segoe UI", 10),
            ForeColor = AppColors.Gray700,
            AutoSize = true,
            Location = new Point(0, 10)
        };
        cmbOrderFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(150, 28),
            Location = new Point(54, 6)
        };
        cmbOrderFilter.Items.AddRange(new object[] { "All", "Placed", "Preparing", "Ready", "Served", "Cancelled" });
        cmbOrderFilter.SelectedIndex = 0;
        cmbOrderFilter.SelectedIndexChanged += CmbOrderFilter_Changed;
        pnlFilterRow.Controls.Add(lblOrdFilterLbl);
        pnlFilterRow.Controls.Add(cmbOrderFilter);

        dgvOrders = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 255) }
        };
        dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvOrders.EnableHeadersVisualStyles = false;
        dgvOrders.Columns.Add("Guest", "Guest");
        dgvOrders.Columns.Add("Room", "Room");
        dgvOrders.Columns["Room"].Width = 60;
        dgvOrders.Columns.Add("Items", "Items");
        dgvOrders.Columns["Items"].Width = 50;
        dgvOrders.Columns.Add("Total", "Total");
        dgvOrders.Columns["Total"].Width = 70;
        dgvOrders.Columns.Add("Status", "Status");
        dgvOrders.Columns["Status"].Width = 80;
        dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

        dgvOrders.CellPainting += (s, e) =>
        {
            if (e.ColumnIndex == 4 && e.RowIndex >= 0 && e.Value != null)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                var statusText = e.Value.ToString()!;
                if (Enum.TryParse<OrderStatus>(statusText, out var status))
                {
                    var color = AppColors.GetOrderStatusColor(status);
                    var badgeRect = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 5,
                        e.CellBounds.Width - 8, e.CellBounds.Height - 10);
                    using var brush = new SolidBrush(color);
                    e.Graphics!.FillRectangle(brush, badgeRect);
                    TextRenderer.DrawText(e.Graphics, statusText, new Font("Segoe UI", 8, FontStyle.Bold),
                        badgeRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        };

        pnlOrdersLeft.Controls.Add(dgvOrders);
        pnlOrdersLeft.Controls.Add(pnlFilterRow);

        // --- Right side contents (detail card on top, actions docked at bottom) ---
        pnlOrderDetail = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(8),
            AutoScroll = true,
            AutoScrollMinSize = new Size(0, 320)
        };
        pnlOrderDetail.Paint += DrawingUtilities.PaintCardBackground;

        lblOrderDetailGuest = new Label
        {
            Text = "Select an order to view details",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(16, 12)
        };

        lblOrderDetailStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(80, 22),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(16, 38),
            Visible = false
        };

        pnlStatusProgression = new Panel
        {
            Location = new Point(16, 64),
            Size = new Size(350, 30),
            BackColor = Color.Transparent,
            Visible = false
        };
        pnlStatusProgression.Paint += PaintStatusProgression;

        dgvOrderLines = new DataGridView
        {
            Location = new Point(16, 100),
            Size = new Size(440, 160),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9),
            Visible = false,
            ScrollBars = ScrollBars.Vertical
        };
        dgvOrderLines.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvOrderLines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvOrderLines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvOrderLines.EnableHeadersVisualStyles = false;
        dgvOrderLines.Columns.Add("Item", "Item");
        dgvOrderLines.Columns.Add("Qty", "Qty");
        dgvOrderLines.Columns["Qty"].Width = 40;
        dgvOrderLines.Columns.Add("Notes", "Notes");
        dgvOrderLines.Columns.Add("Total", "Total");
        dgvOrderLines.Columns["Total"].Width = 70;

        lblOrderTotal = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(16, 270),
            Visible = false
        };

        pnlOrderDetail.Controls.AddRange(new Control[] {
            lblOrderDetailGuest, lblOrderDetailStatus,
            pnlStatusProgression, dgvOrderLines, lblOrderTotal
        });

        var pnlOrderActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };

        btnAdvanceOrder = new Button
        {
            Text = "Advance Status",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(150, 36),
            Enabled = false,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnAdvanceOrder.FlatAppearance.BorderSize = 0;
        btnAdvanceOrder.Click += BtnAdvanceOrder_Click;

        btnAddItemsToOrder = new Button
        {
            Text = "Add Items",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Enabled = false,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnAddItemsToOrder.FlatAppearance.BorderSize = 0;
        btnAddItemsToOrder.Click += BtnAddItemsToOrder_Click;

        btnCancelOrder = new Button
        {
            Text = "Cancel Order",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 36),
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnCancelOrder.FlatAppearance.BorderSize = 0;
        btnCancelOrder.Click += BtnCancelOrder_Click;

        pnlOrderActions.Controls.AddRange(new Control[] { btnAdvanceOrder, btnAddItemsToOrder, btnCancelOrder });

        // Right sidebar: Fill (detail with scroll) on top of Bottom-docked actions.
        pnlOrderRight.Controls.Add(pnlOrderDetail);
        pnlOrderRight.Controls.Add(pnlOrderActions);

        // Right first (claims fixed width), then Fill grabs the rest.
        tab.Controls.Add(pnlOrdersLeft);
        tab.Controls.Add(pnlOrderRight);
    }

    private void InitFinancesTab()
    {
        // === Navy Header ===
        var pnlFinHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppColors.Primary
        };
        var lblFinTitle = new Label
        {
            Text = "Finances & Billing",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 12)
        };
        pnlFinHeader.Controls.Add(lblFinTitle);

        // === KPI Row ===
        var tblFinKPI = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = AppColors.Surface,
            Padding = new Padding(12, 8, 12, 0)
        };
        tblFinKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblFinKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblFinKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tblFinKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        Label _d;
        var kpiTotalRev = CreateKPICard("Total Revenue", "$0", AppColors.Tertiary, out lblFinTotalRev, out _d, false);
        var kpiUnpaid = CreateKPICard("Unpaid Bills", "0", AppColors.StatusOOS, out lblFinUnpaid, out _d, false);
        var kpiPaidToday = CreateKPICard("Paid Today", "$0", AppColors.Accent, out lblFinPaidToday, out _d, false);
        var kpiOutstanding = CreateKPICard("Outstanding", "$0", AppColors.Primary, out lblFinOutstanding, out _d, false);

        tblFinKPI.Controls.Add(kpiTotalRev, 0, 0);
        tblFinKPI.Controls.Add(kpiUnpaid, 1, 0);
        tblFinKPI.Controls.Add(kpiPaidToday, 2, 0);
        tblFinKPI.Controls.Add(kpiOutstanding, 3, 0);

        // === Content ===
        var pnlFinContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 16)
        };

        // Filter
        var lblFinFilterLbl = new Label
        {
            Text = "Filter:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(0, 4),
            AutoSize = true
        };
        pnlFinContent.Controls.Add(lblFinFilterLbl);

        cmbFinFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(50, 0),
            Size = new Size(140, 28)
        };
        cmbFinFilter.Items.AddRange(new object[] { "All", "Paid", "Pending", "Refunded" });
        cmbFinFilter.SelectedIndex = 0;
        cmbFinFilter.SelectedIndexChanged += CmbFinFilter_Changed;
        pnlFinContent.Controls.Add(cmbFinFilter);

        // Invoice grid
        dgvInvoices = new DataGridView
        {
            Location = new Point(0, 36),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 255) }
        };
        dgvInvoices.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvInvoices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvInvoices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvInvoices.EnableHeadersVisualStyles = false;
        dgvInvoices.Columns.Add("InvoiceNum", "Invoice #");
        dgvInvoices.Columns.Add("Guest", "Guest");
        dgvInvoices.Columns.Add("Room", "Room");
        dgvInvoices.Columns.Add("Total", "Total");
        dgvInvoices.Columns.Add("Status", "Status");
        dgvInvoices.SelectionChanged += DgvInvoices_SelectionChanged;

        // Paint status badges
        dgvInvoices.CellPainting += (s, e) =>
        {
            if (e.ColumnIndex == 4 && e.RowIndex >= 0 && e.Value != null)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                var statusText = e.Value.ToString()!;
                if (Enum.TryParse<PaymentStatus>(statusText, out var status))
                {
                    var color = AppColors.GetPaymentStatusColor(status);
                    var badgeRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + 6,
                        e.CellBounds.Width - 12, e.CellBounds.Height - 12);
                    using var brush = new SolidBrush(color);
                    e.Graphics!.FillRectangle(brush, badgeRect);
                    var prefix = status == PaymentStatus.Paid ? "\u2713 " : status == PaymentStatus.Pending ? "\u25CB " : "";
                    TextRenderer.DrawText(e.Graphics, prefix + statusText, new Font("Segoe UI", 8, FontStyle.Bold),
                        badgeRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        };

        pnlFinContent.Controls.Add(dgvInvoices);
        pnlFinContent.Resize += (s, e) =>
        {
            dgvInvoices.Size = new Size(pnlFinContent.Width, pnlFinContent.Height - 86);
        };

        // Buttons
        var pnlFinActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        btnViewInvoice = new Button
        {
            Text = "View Invoice",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 36),
            Enabled = false,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        btnViewInvoice.FlatAppearance.BorderSize = 0;
        btnViewInvoice.Click += BtnViewInvoice_Click;

        btnMarkPaid = new Button
        {
            Text = "Mark as Paid",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 36),
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnMarkPaid.FlatAppearance.BorderSize = 0;
        btnMarkPaid.Click += BtnMarkPaid_Click;

        pnlFinActions.Controls.AddRange(new Control[] { btnViewInvoice, btnMarkPaid });
        pnlFinContent.Controls.Add(pnlFinActions);

        // Add in reverse dock order
        tabFinances.Controls.Add(pnlFinContent);
        tabFinances.Controls.Add(tblFinKPI);
        tabFinances.Controls.Add(pnlFinHeader);
    }

    private void InitReportsTab()
    {
        var pnlRepHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = AppColors.Surface
        };

        var lblRepTitle = new Label
        {
            Text = "Reports & Analytics",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        pnlRepHeader.Controls.Add(lblRepTitle);

        btnRefreshReports = new Button
        {
            Text = "Refresh",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 34),
            Location = new Point(0, 40),
            Cursor = Cursors.Hand
        };
        btnRefreshReports.FlatAppearance.BorderSize = 0;
        btnRefreshReports.Click += BtnRefreshReports_Click;
        pnlRepHeader.Controls.Add(btnRefreshReports);

        tabReports.Controls.Add(pnlRepHeader);

        // Scrollable content area
        var flpReports = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(0, 5, 0, 0),
            BackColor = AppColors.Surface
        };

        // KPI panels
        pnlOccReport = CreateMetricCard("Occupancy Rate", AppColors.Primary, out lblOccReportValue, out _, new Size(200, 90));
        pnlOccReport.Margin = new Padding(0, 0, 10, 10);

        pnlAvgStay = CreateMetricCard("Avg Stay Duration", AppColors.Tertiary, out lblAvgStayValue, out _, new Size(200, 90));
        pnlAvgStay.Margin = new Padding(0, 0, 10, 10);

        pnlRepeatGuest = CreateMetricCard("Repeat Guests", AppColors.StatusClean, out lblRepeatGuestValue, out _, new Size(200, 90));
        pnlRepeatGuest.Margin = new Padding(0, 0, 10, 10);

        pnlTotalRevenue = CreateMetricCard("Total Revenue", AppColors.Accent, out lblTotalRevenueValue, out _, new Size(200, 90));
        lblTotalRevenueValue.ForeColor = AppColors.Primary;
        pnlTotalRevenue.Margin = new Padding(0, 0, 10, 10);

        // GroupBoxes
        grpRevRoom = new GroupBox
        {
            Text = "Revenue by Room Type",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Size = new Size(280, 200),
            Margin = new Padding(0, 0, 10, 10)
        };
        lblRevRoom = new Label { Font = new Font("Segoe UI", 10), ForeColor = AppColors.Gray700, Location = new Point(12, 28), AutoSize = true };
        grpRevRoom.Controls.Add(lblRevRoom);

        grpRevRestaurant = new GroupBox
        {
            Text = "Restaurant Revenue by Category",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Size = new Size(280, 200),
            Margin = new Padding(0, 0, 10, 10)
        };
        lblRevRestaurant = new Label { Font = new Font("Segoe UI", 10), ForeColor = AppColors.Gray700, Location = new Point(12, 28), AutoSize = true };
        grpRevRestaurant.Controls.Add(lblRevRestaurant);

        grpTopItems = new GroupBox
        {
            Text = "Top Menu Items",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Size = new Size(280, 200),
            Margin = new Padding(0, 0, 10, 10)
        };
        lblTopItems = new Label { Font = new Font("Segoe UI", 10), ForeColor = AppColors.Gray700, Location = new Point(12, 28), AutoSize = true };
        grpTopItems.Controls.Add(lblTopItems);

        flpReports.Controls.AddRange(new Control[] {
            pnlOccReport, pnlAvgStay, pnlRepeatGuest, pnlTotalRevenue,
            grpRevRoom, grpRevRestaurant, grpTopItems
        });

        tabReports.Controls.Add(flpReports);
    }

    private void InitUsersTab()
    {
        // Header
        var pnlUsersHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppColors.Primary
        };
        pnlUsersHeader.Controls.Add(new Label
        {
            Text = "Users & Roles",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 12)
        });

        // Sub-tabs
        userSubTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Point(16, 8)
        };
        subTabUsers = new TabPage("Users") { BackColor = AppColors.Surface, Padding = new Padding(12) };
        subTabRoles = new TabPage("Roles") { BackColor = AppColors.Surface, Padding = new Padding(12) };
        userSubTabs.TabPages.Add(subTabUsers);
        userSubTabs.TabPages.Add(subTabRoles);

        BuildUsersSubTab(subTabUsers);
        BuildRolesSubTab(subTabRoles);

        tabUsers.Controls.Add(userSubTabs);
        tabUsers.Controls.Add(pnlUsersHeader);
    }

    private void BuildUsersSubTab(TabPage tab)
    {
        var pnlActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        btnAddUser = MakeActionButton("+ Add User", AppColors.Tertiary, Color.White);
        btnAddUser.Click += BtnAddUser_Click;

        btnEditUser = MakeActionButton("Edit", AppColors.Primary, Color.White);
        btnEditUser.Click += BtnEditUser_Click;

        btnRemoveUser = MakeActionButton("Remove", AppColors.StatusOOS, Color.White);
        btnRemoveUser.Click += BtnRemoveUser_Click;

        pnlActions.Controls.AddRange(new Control[] { btnAddUser, btnEditUser, btnRemoveUser });

        dgvUsers = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 255) }
        };
        dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvUsers.EnableHeadersVisualStyles = false;
        dgvUsers.Columns.Add("Username", "Username");
        dgvUsers.Columns.Add("Role", "Role");
        dgvUsers.Columns.Add("Permissions", "Permissions");

        tab.Controls.Add(dgvUsers);
        tab.Controls.Add(pnlActions);
    }

    private void BuildRolesSubTab(TabPage tab)
    {
        var pnlActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        btnAddRole = MakeActionButton("+ Add Role", AppColors.Tertiary, Color.White);
        btnAddRole.Click += BtnAddRole_Click;

        btnEditRole = MakeActionButton("Edit", AppColors.Primary, Color.White);
        btnEditRole.Click += BtnEditRole_Click;

        btnRemoveRole = MakeActionButton("Remove", AppColors.StatusOOS, Color.White);
        btnRemoveRole.Click += BtnRemoveRole_Click;

        pnlActions.Controls.AddRange(new Control[] { btnAddRole, btnEditRole, btnRemoveRole });

        dgvRoles = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 255) }
        };
        dgvRoles.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvRoles.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvRoles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvRoles.EnableHeadersVisualStyles = false;
        dgvRoles.Columns.Add("Name", "Role");
        dgvRoles.Columns.Add("System", "System");
        dgvRoles.Columns.Add("Permissions", "Permissions");

        tab.Controls.Add(dgvRoles);
        tab.Controls.Add(pnlActions);
    }

    private static Button MakeActionButton(string text, Color bg, Color fg)
    {
        var b = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = bg,
            ForeColor = fg,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 34),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }
}
