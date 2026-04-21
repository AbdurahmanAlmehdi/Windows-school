using System.Drawing.Drawing2D;
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
    private GroupBox grpNewRes = null!;
    private TextBox txtPhone = null!;
    private Button btnLookup = null!;
    private Label lblGuestStatus = null!;
    private TextBox txtGuestName = null!;
    private ComboBox cmbRoom = null!;
    private DateTimePicker dtpCheckIn = null!;
    private DateTimePicker dtpCheckOut = null!;
    private Button btnCreateRes = null!;

    // Rooms controls
    private ComboBox cmbRoomFilter = null!;
    private FlowLayoutPanel flpRooms = null!;

    // Restaurant controls
    private DataGridView dgvMenu = null!;
    private GroupBox grpNewOrder = null!;
    private ComboBox cmbStay = null!;
    private ComboBox cmbMenuItem = null!;
    private NumericUpDown nudQty = null!;
    private Button btnAddLine = null!;
    private ListBox lstOrderLines = null!;
    private Button btnPlaceOrder = null!;
    private DataGridView dgvOrders = null!;
    private Button btnAdvanceOrder = null!;
    private Button btnCancelOrder = null!;

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
        tabReservations = new TabPage("Reservations") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabRooms = new TabPage("Rooms") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabRestaurant = new TabPage("Restaurant") { BackColor = AppColors.Surface, Padding = new Padding(16) };
        tabReports = new TabPage("Reports") { BackColor = AppColors.Surface, Padding = new Padding(16) };

        tabMain.TabPages.Add(tabDashboard);
        tabMain.TabPages.Add(tabReservations);
        tabMain.TabPages.Add(tabRooms);
        tabMain.TabPages.Add(tabRestaurant);
        tabMain.TabPages.Add(tabReports);
        tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

        InitDashboardTab();
        InitReservationsTab();
        InitRoomsTab();
        InitRestaurantTab();
        InitReportsTab();

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
            Height = 60,
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
            Location = new Point(20, 34)
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
        var lblResTitle = new Label
        {
            Text = "Reservations",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        tabReservations.Controls.Add(lblResTitle);

        // Filter
        var lblFilter = new Label
        {
            Text = "Filter:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(0, 42),
            AutoSize = true
        };
        tabReservations.Controls.Add(lblFilter);

        cmbResFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(50, 38),
            Size = new Size(160, 28)
        };
        cmbResFilter.Items.AddRange(new object[] { "All", "Confirmed", "CheckedIn", "Completed", "Cancelled" });
        cmbResFilter.SelectedIndex = 0;
        cmbResFilter.SelectedIndexChanged += CmbResFilter_Changed;
        tabReservations.Controls.Add(cmbResFilter);

        lblResStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Tertiary,
            Location = new Point(230, 42),
            AutoSize = true
        };
        tabReservations.Controls.Add(lblResStatus);

        // DataGridView
        dgvReservations = new DataGridView
        {
            Location = new Point(0, 75),
            Size = new Size(700, 300),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10)
        };
        dgvReservations.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvReservations.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvReservations.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvReservations.EnableHeadersVisualStyles = false;
        dgvReservations.Columns.Add("Room", "Room #");
        dgvReservations.Columns.Add("Guest", "Guest");
        dgvReservations.Columns.Add("Phone", "Phone");
        dgvReservations.Columns.Add("CheckIn", "Check In");
        dgvReservations.Columns.Add("CheckOut", "Check Out");
        dgvReservations.Columns.Add("Status", "Status");
        dgvReservations.SelectionChanged += DgvReservations_SelectionChanged;
        tabReservations.Controls.Add(dgvReservations);

        // Action buttons
        btnCheckIn = new Button
        {
            Text = "Check In",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Location = new Point(0, 385),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnCheckIn.FlatAppearance.BorderSize = 0;
        btnCheckIn.Click += BtnCheckIn_Click;
        tabReservations.Controls.Add(btnCheckIn);

        btnCheckOut = new Button
        {
            Text = "Check Out",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Location = new Point(120, 385),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnCheckOut.FlatAppearance.BorderSize = 0;
        btnCheckOut.Click += BtnCheckOut_Click;
        tabReservations.Controls.Add(btnCheckOut);

        btnCancelRes = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Location = new Point(240, 385),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnCancelRes.FlatAppearance.BorderSize = 0;
        btnCancelRes.Click += BtnCancelRes_Click;
        tabReservations.Controls.Add(btnCancelRes);

        // New reservation group
        grpNewRes = new GroupBox
        {
            Text = "New Reservation",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(0, 435),
            Size = new Size(700, 220),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        var lblPhone = new Label { Text = "Phone:", Font = new Font("Segoe UI", 10), Location = new Point(12, 30), AutoSize = true };
        txtPhone = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(80, 27), Size = new Size(150, 28) };
        btnLookup = new Button
        {
            Text = "Lookup",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 28),
            Location = new Point(240, 27),
            Cursor = Cursors.Hand
        };
        btnLookup.FlatAppearance.BorderSize = 0;
        btnLookup.Click += BtnLookup_Click;

        lblGuestStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray500,
            Location = new Point(330, 32),
            AutoSize = true
        };

        var lblName = new Label { Text = "Guest Name:", Font = new Font("Segoe UI", 10), Location = new Point(12, 65), AutoSize = true };
        txtGuestName = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(120, 62), Size = new Size(200, 28), Visible = false };

        var lblRoom2 = new Label { Text = "Room:", Font = new Font("Segoe UI", 10), Location = new Point(12, 100), AutoSize = true };
        cmbRoom = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(80, 97),
            Size = new Size(240, 28)
        };

        var lblCI = new Label { Text = "Check In:", Font = new Font("Segoe UI", 10), Location = new Point(12, 135), AutoSize = true };
        dtpCheckIn = new DateTimePicker { Font = new Font("Segoe UI", 10), Location = new Point(100, 132), Size = new Size(200, 28), Format = DateTimePickerFormat.Short };

        var lblCO = new Label { Text = "Check Out:", Font = new Font("Segoe UI", 10), Location = new Point(320, 135), AutoSize = true };
        dtpCheckOut = new DateTimePicker { Font = new Font("Segoe UI", 10), Location = new Point(415, 132), Size = new Size(200, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(1) };

        btnCreateRes = new Button
        {
            Text = "Create Reservation",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(200, 38),
            Location = new Point(12, 172),
            Cursor = Cursors.Hand
        };
        btnCreateRes.FlatAppearance.BorderSize = 0;
        btnCreateRes.Click += BtnCreateRes_Click;

        grpNewRes.Controls.AddRange(new Control[] {
            lblPhone, txtPhone, btnLookup, lblGuestStatus,
            lblName, txtGuestName,
            lblRoom2, cmbRoom,
            lblCI, dtpCheckIn, lblCO, dtpCheckOut,
            btnCreateRes
        });
        tabReservations.Controls.Add(grpNewRes);
    }

    private void InitRoomsTab()
    {
        var lblRoomsTitle = new Label
        {
            Text = "Room Management",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        tabRooms.Controls.Add(lblRoomsTitle);

        var lblRFilter = new Label
        {
            Text = "Filter:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(0, 42),
            AutoSize = true
        };
        tabRooms.Controls.Add(lblRFilter);

        cmbRoomFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(50, 38),
            Size = new Size(160, 28)
        };
        cmbRoomFilter.Items.AddRange(new object[] { "All", "Available", "Occupied", "NeedsCleaning", "OutOfService" });
        cmbRoomFilter.SelectedIndex = 0;
        cmbRoomFilter.SelectedIndexChanged += CmbRoomFilter_Changed;
        tabRooms.Controls.Add(cmbRoomFilter);

        flpRooms = new FlowLayoutPanel
        {
            Location = new Point(0, 75),
            Size = new Size(700, 500),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true,
            BackColor = AppColors.Surface
        };
        tabRooms.Controls.Add(flpRooms);
    }

    private void InitRestaurantTab()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 500,
            Orientation = Orientation.Vertical,
            BackColor = AppColors.Surface
        };

        // Left: Menu + New Order
        var lblMenuTitle = new Label
        {
            Text = "Menu & Orders",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        split.Panel1.Controls.Add(lblMenuTitle);

        dgvMenu = new DataGridView
        {
            Location = new Point(0, 40),
            Size = new Size(480, 200),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10)
        };
        dgvMenu.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvMenu.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvMenu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvMenu.EnableHeadersVisualStyles = false;
        dgvMenu.Columns.Add("Name", "Name");
        dgvMenu.Columns.Add("Category", "Category");
        dgvMenu.Columns.Add("Price", "Price");
        split.Panel1.Controls.Add(dgvMenu);

        grpNewOrder = new GroupBox
        {
            Text = "New Order",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(0, 250),
            Size = new Size(480, 340),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        var lblStay2 = new Label { Text = "Stay:", Font = new Font("Segoe UI", 10), Location = new Point(12, 28), AutoSize = true };
        cmbStay = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 25),
            Size = new Size(300, 28)
        };

        var lblItem = new Label { Text = "Item:", Font = new Font("Segoe UI", 10), Location = new Point(12, 63), AutoSize = true };
        cmbMenuItem = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 60),
            Size = new Size(250, 28)
        };

        var lblQty = new Label { Text = "Qty:", Font = new Font("Segoe UI", 10), Location = new Point(320, 63), AutoSize = true };
        nudQty = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(358, 60),
            Size = new Size(60, 28),
            Minimum = 1,
            Maximum = 20,
            Value = 1
        };

        btnAddLine = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(60, 28),
            Location = new Point(12, 98),
            Cursor = Cursors.Hand
        };
        btnAddLine.FlatAppearance.BorderSize = 0;
        btnAddLine.Click += BtnAddLine_Click;

        lstOrderLines = new ListBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(12, 135),
            Size = new Size(400, 130)
        };

        btnPlaceOrder = new Button
        {
            Text = "Place Order",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(160, 38),
            Location = new Point(12, 275),
            Cursor = Cursors.Hand
        };
        btnPlaceOrder.FlatAppearance.BorderSize = 0;
        btnPlaceOrder.Click += BtnPlaceOrder_Click;

        grpNewOrder.Controls.AddRange(new Control[] {
            lblStay2, cmbStay, lblItem, cmbMenuItem,
            lblQty, nudQty, btnAddLine, lstOrderLines, btnPlaceOrder
        });
        split.Panel1.Controls.Add(grpNewOrder);

        // Right: Active Orders
        var lblOrdersTitle = new Label
        {
            Text = "Active Orders",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        split.Panel2.Controls.Add(lblOrdersTitle);

        dgvOrders = new DataGridView
        {
            Location = new Point(0, 40),
            Size = new Size(450, 420),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10)
        };
        dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        dgvOrders.EnableHeadersVisualStyles = false;
        dgvOrders.Columns.Add("Guest", "Guest");
        dgvOrders.Columns.Add("Room", "Room");
        dgvOrders.Columns.Add("Total", "Total");
        dgvOrders.Columns.Add("Status", "Status");
        split.Panel2.Controls.Add(dgvOrders);

        btnAdvanceOrder = new Button
        {
            Text = "Advance Status",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(140, 36),
            Location = new Point(0, 470),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Cursor = Cursors.Hand
        };
        btnAdvanceOrder.FlatAppearance.BorderSize = 0;
        btnAdvanceOrder.Click += BtnAdvanceOrder_Click;
        split.Panel2.Controls.Add(btnAdvanceOrder);

        btnCancelOrder = new Button
        {
            Text = "Cancel Order",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 36),
            Location = new Point(150, 470),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Cursor = Cursors.Hand
        };
        btnCancelOrder.FlatAppearance.BorderSize = 0;
        btnCancelOrder.Click += BtnCancelOrder_Click;
        split.Panel2.Controls.Add(btnCancelOrder);

        tabRestaurant.Controls.Add(split);
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
}
