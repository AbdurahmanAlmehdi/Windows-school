using System.ComponentModel;
using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;
using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly AuthService _authService;
    private readonly RoomService _roomService;
    private readonly BookingService _bookingService;
    private readonly RestaurantService _restaurantService;
    private readonly ReportService _reportService;
    private readonly InvoiceService _invoiceService;
    private readonly UserService _userService;
    private readonly DataStore _store;

    private Room? _selectedRoom;
    private Models.MenuItem? _selectedMenuItem;
    private readonly BindingList<OrderLine> _currentOrderLines = new();

    public MainForm(
        AuthService authService,
        RoomService roomService,
        BookingService bookingService,
        RestaurantService restaurantService,
        ReportService reportService,
        InvoiceService invoiceService,
        UserService userService,
        DataStore dataStore)
    {
        _authService = authService;
        _roomService = roomService;
        _bookingService = bookingService;
        _restaurantService = restaurantService;
        _reportService = reportService;
        _invoiceService = invoiceService;
        _userService = userService;
        _store = dataStore;

        InitializeComponent();

        WindowState = FormWindowState.Maximized;

        // Position logout button
        btnLogout.Location = new Point(panelHeader.Width - btnLogout.Width - 12, 8);

        // Hide Reports tab for now
        tabMain.TabPages.Remove(tabReports);

        // Hide the Users tab when the current role can't even read users.
        if (!_authService.Can(PermissionResource.Users, PermissionAction.Read))
            tabMain.TabPages.Remove(tabUsers);

        // Gate management controls based on per-resource CRUD permissions.
        btnAddRoom.Visible    = _authService.Can(PermissionResource.Rooms, PermissionAction.Create);
        btnEditRoom.Visible   = _authService.Can(PermissionResource.Rooms, PermissionAction.Update);
        btnRemoveRoom.Visible = _authService.Can(PermissionResource.Rooms, PermissionAction.Delete);

        btnAddMenuItem.Visible    = _authService.Can(PermissionResource.MenuItems, PermissionAction.Create);
        btnEditMenuItem.Visible   = _authService.Can(PermissionResource.MenuItems, PermissionAction.Update);
        btnToggleAvail.Visible    = _authService.Can(PermissionResource.MenuItems, PermissionAction.Update);
        btnRemoveMenuItem.Visible = _authService.Can(PermissionResource.MenuItems, PermissionAction.Delete);

        Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        RefreshDashboard();
        RefreshReservations();
        RefreshRooms();
        RefreshRestaurant();
    }

    private void TabMain_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tabMain.SelectedTab == tabDashboard) RefreshDashboard();
        else if (tabMain.SelectedTab == tabReservations) RefreshReservations();
        else if (tabMain.SelectedTab == tabRooms) RefreshRooms();
        else if (tabMain.SelectedTab == tabRestaurant) RefreshRestaurant();
        else if (tabMain.SelectedTab == tabFinances) RefreshFinances();
        else if (tabMain.SelectedTab == tabReports) RefreshReports();
        else if (tabMain.SelectedTab == tabUsers) RefreshUsers();
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        _authService.Logout();
        Close();
    }

    // ===================== DASHBOARD =====================

    private void RefreshDashboard()
    {
        UpdateWelcomeBar();
        UpdateKPICards();
        RefreshArrivalsPanel();
        RefreshDeparturesPanel();
        RefreshHousekeepingPanel();
        RefreshActiveOrdersPanel();
    }

    private void UpdateWelcomeBar()
    {
        var hour = DateTime.Now.Hour;
        var greeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
        var username = _authService.CurrentUser?.Username ?? "user";
        lblGreeting.Text = $"{greeting}, {username}";
        lblDate.Text = DateTime.Today.ToString("dddd, MMM dd, yyyy");
    }

    private void UpdateKPICards()
    {
        var occupancyRate = _reportService.GetOccupancyRate();
        lblOccupancyValue.Text = $"{occupancyRate:F1}%";

        var available = _store.Rooms.Count(r => r.IsAvailable);
        lblAvailableValue.Text = available.ToString();

        var occupied = _store.Rooms.Count(r => r.IsOccupied);
        lblOccupiedValue.Text = occupied.ToString();

        var oos = _store.Rooms.Count(r => r.Condition == RoomCondition.OutOfService);
        lblOOSValue.Text = oos.ToString();

        pnlOccupancy.Invalidate();
    }

    private void RefreshArrivalsPanel()
    {
        flpArrivals.Controls.Clear();
        var today = DateTime.Today;
        var arrivals = _store.Reservations
            .Where(r => r.CheckInDate.Date == today && r.Status == ReservationStatus.Confirmed)
            .ToList();

        UpdateCardBadge(pnlArrivalsCard, arrivals.Count);
        lblArrivalsHeader.Text = $"Arrivals Today";

        if (arrivals.Count == 0)
        {
            flpArrivals.Controls.Add(CreateEmptyState("No arrivals today"));
            return;
        }

        foreach (var res in arrivals)
            flpArrivals.Controls.Add(CreateArrivalRow(res));
    }

    private void RefreshDeparturesPanel()
    {
        flpDepartures.Controls.Clear();
        var today = DateTime.Today;
        var departures = _store.Stays
            .Where(s => s.ExpectedCheckOut.Date == today && s.Status == StayStatus.Active)
            .ToList();

        UpdateCardBadge(pnlDeparturesCard, departures.Count);
        lblDeparturesHeader.Text = $"Departures Today";

        if (departures.Count == 0)
        {
            flpDepartures.Controls.Add(CreateEmptyState("No departures today"));
            return;
        }

        foreach (var stay in departures)
            flpDepartures.Controls.Add(CreateDepartureRow(stay));
    }

    private void RefreshHousekeepingPanel()
    {
        flpHousekeeping.Controls.Clear();
        var dirtyRooms = _store.Rooms
            .Where(r => r.Condition == RoomCondition.NeedsCleaning)
            .ToList();

        UpdateCardBadge(pnlHousekeepingCard, dirtyRooms.Count);
        lblHousekeepingHeader.Text = $"Housekeeping";

        if (dirtyRooms.Count == 0)
        {
            flpHousekeeping.Controls.Add(CreateEmptyState("All rooms clean!"));
            return;
        }

        foreach (var room in dirtyRooms)
            flpHousekeeping.Controls.Add(CreateHousekeepingRow(room));
    }

    private void RefreshActiveOrdersPanel()
    {
        flpActiveOrders.Controls.Clear();
        var orders = _store.Orders
            .Where(o => o.Status is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.Ready)
            .ToList();

        UpdateCardBadge(pnlOrdersCard, orders.Count);
        lblOrdersHeader.Text = $"Active Orders";

        if (orders.Count == 0)
        {
            flpActiveOrders.Controls.Add(CreateEmptyState("No active orders"));
            return;
        }

        foreach (var order in orders)
            flpActiveOrders.Controls.Add(CreateOrderRow(order));
    }

    private void UpdateCardBadge(Panel card, int count)
    {
        foreach (Control c in card.Controls)
        {
            if (c is Panel header)
            {
                foreach (Control hc in header.Controls)
                {
                    if (hc is Label lbl && lbl.Tag?.ToString() == "badge")
                    {
                        lbl.Text = count.ToString();
                        lbl.Invalidate();
                    }
                }
            }
        }
    }

    // --- Row builders ---

    private Panel CreateArrivalRow(Reservation res)
    {
        var row = new Panel
        {
            Size = new Size(flpArrivals.Width - 32, 60),
            BackColor = Color.FromArgb(245, 248, 255),
            Margin = new Padding(0, 2, 0, 2)
        };
        row.Paint += (s, e) =>
        {
            using var pen = new Pen(AppColors.Gray200);
            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
        };

        var lblName = new Label
        {
            Text = res.Guest.Name,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(10, 6),
            AutoSize = true
        };

        var lblInfo = new Label
        {
            Text = $"Room {res.Room.Number} {res.Room.Type}",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray500,
            Location = new Point(10, 28),
            AutoSize = true
        };

        var btn = new Button
        {
            Text = "Check In",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(75, 28),
            Location = new Point(row.Width - 90, 16),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand,
            Tag = res
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += BtnQuickCheckIn_Click;

        row.Controls.AddRange(new Control[] { lblName, lblInfo, btn });
        return row;
    }

    private Panel CreateDepartureRow(Stay stay)
    {
        var row = new Panel
        {
            Size = new Size(flpDepartures.Width - 32, 60),
            BackColor = Color.FromArgb(245, 248, 255),
            Margin = new Padding(0, 2, 0, 2)
        };
        row.Paint += (s, e) =>
        {
            using var pen = new Pen(AppColors.Gray200);
            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
        };

        var lblName = new Label
        {
            Text = stay.Guest.Name,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(10, 6),
            AutoSize = true
        };

        var lblInfo = new Label
        {
            Text = $"Room {stay.Room.Number} {stay.Room.Type}  ${stay.TotalCharges:F2}",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray500,
            Location = new Point(10, 28),
            AutoSize = true
        };

        var btn = new Button
        {
            Text = "Check Out",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 28),
            Location = new Point(row.Width - 95, 16),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand,
            Tag = stay
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += BtnQuickCheckOut_Click;

        row.Controls.AddRange(new Control[] { lblName, lblInfo, btn });
        return row;
    }

    private Panel CreateHousekeepingRow(Room room)
    {
        var row = new Panel
        {
            Size = new Size(flpHousekeeping.Width - 32, 50),
            BackColor = Color.FromArgb(255, 250, 240),
            Margin = new Padding(0, 2, 0, 2)
        };
        row.Paint += (s, e) =>
        {
            using var pen = new Pen(AppColors.Gray200);
            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
        };

        var lblRoom = new Label
        {
            Text = $"Room {room.Number}",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(10, 6),
            AutoSize = true
        };

        var lblType = new Label
        {
            Text = room.Type.ToString(),
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray500,
            Location = new Point(10, 26),
            AutoSize = true
        };

        var btn = new Button
        {
            Text = "Mark Clean",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            BackColor = AppColors.StatusClean,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(82, 28),
            Location = new Point(row.Width - 97, 11),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand,
            Tag = room
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += BtnMarkClean_Click;

        row.Controls.AddRange(new Control[] { lblRoom, lblType, btn });
        return row;
    }

    private Panel CreateOrderRow(RestaurantOrder order)
    {
        var row = new Panel
        {
            Size = new Size(flpActiveOrders.Width - 32, 50),
            BackColor = Color.FromArgb(255, 252, 240),
            Margin = new Padding(0, 2, 0, 2)
        };
        row.Paint += (s, e) =>
        {
            using var pen = new Pen(AppColors.Gray200);
            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
        };

        var lblGuest = new Label
        {
            Text = $"{order.Stay.Guest.Name} - Rm {order.Stay.Room.Number}",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(10, 6),
            AutoSize = true
        };

        var lblTotal = new Label
        {
            Text = $"${order.Total:F2}",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray600,
            Location = new Point(10, 26),
            AutoSize = true
        };

        var statusColor = AppColors.GetOrderStatusColor(order.Status);
        var lblStatus = new Label
        {
            Text = order.Status.ToString(),
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = statusColor,
            AutoSize = false,
            Size = new Size(70, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(row.Width - 85, 15),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        row.Controls.AddRange(new Control[] { lblGuest, lblTotal, lblStatus });
        return row;
    }

    private Label CreateEmptyState(string message)
    {
        return new Label
        {
            Text = message,
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            ForeColor = AppColors.Gray400,
            AutoSize = true,
            Padding = new Padding(4, 12, 0, 0)
        };
    }

    // --- Quick action handlers ---

    private void BtnQuickCheckIn_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Reservation res) return;

        _bookingService.CheckIn(res);
        MessageBox.Show($"Guest {res.Guest.Name} checked into Room {res.Room.Number}.",
            "Check In", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshDashboard();
    }

    private void BtnQuickCheckOut_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Stay stay) return;

        _bookingService.CheckOut(stay);
        var invoice = _invoiceService.GenerateInvoice(stay);
        using var form = new CheckoutForm(invoice);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _invoiceService.MarkPaid(invoice, form.SelectedPaymentMethod);
        }
        RefreshDashboard();
    }

    private void BtnMarkClean_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Room room) return;

        _roomService.MarkClean(room);
        RefreshDashboard();
    }

    // ===================== RESERVATIONS =====================

    private List<Reservation> _filteredReservations = new();

    private void RefreshReservations()
    {
        var filter = cmbResFilter.SelectedItem?.ToString() ?? "All";

        _filteredReservations = filter == "All"
            ? _store.Reservations.ToList()
            : _store.Reservations
                .Where(r => r.Status.ToString() == filter)
                .ToList();

        dgvReservations.Rows.Clear();
        foreach (var r in _filteredReservations)
        {
            var partyText = r.ChildCount > 0
                ? $"{r.AdultCount}A + {r.ChildCount}C"
                : $"{r.AdultCount}A";
            dgvReservations.Rows.Add(
                r.Room.Number,
                r.Guest.Name,
                r.Guest.Contact,
                string.IsNullOrEmpty(r.Guest.Passport) ? "—" : r.Guest.Passport,
                partyText,
                r.CheckInDate.ToShortDateString(),
                r.CheckOutDate.ToShortDateString(),
                r.Status.ToString());
        }

        lblResStatus.Text = $"{_filteredReservations.Count} reservation(s)";
        UpdateResButtons();
        RefreshResKPIs();
    }

    private void RefreshResKPIs()
    {
        var today = DateTime.Today;
        var arrivalsToday = _store.Reservations.Count(r => r.CheckInDate.Date == today && r.Status == ReservationStatus.Confirmed);
        var activeStays = _store.Stays.Count(s => s.Status == StayStatus.Active);
        var pendingRes = _store.Reservations.Count(r => r.Status is ReservationStatus.Confirmed or ReservationStatus.Pending);
        var completedToday = _store.Reservations.Count(r => r.Status == ReservationStatus.Completed &&
            _store.Stays.Any(s => s.Guest == r.Guest && s.Room == r.Room && s.ActualCheckOut?.Date == today));

        lblResKpiArrivals.Text = arrivalsToday.ToString();
        lblResKpiActive.Text = activeStays.ToString();
        lblResKpiPending.Text = pendingRes.ToString();
        lblResKpiCompleted.Text = completedToday.ToString();
    }

    private void CmbResFilter_Changed(object? sender, EventArgs e) => RefreshReservations();

    private void BtnNewReservation_Click(object? sender, EventArgs e)
    {
        using var dlg = new ReservationDialog(_store, _roomService, _bookingService);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            RefreshReservations();
        }
    }

    private void DgvReservations_SelectionChanged(object? sender, EventArgs e) => UpdateResButtons();

    private void UpdateResButtons()
    {
        var res = GetSelectedReservation();
        btnCheckIn.Enabled = res != null && res.Status == ReservationStatus.Confirmed;
        btnCancelRes.Enabled = res != null &&
            res.Status is ReservationStatus.Confirmed or ReservationStatus.Pending;

        // Check Out is for active stays associated with a checked-in reservation
        if (res != null && res.Status == ReservationStatus.CheckedIn)
        {
            var stay = _store.Stays.FirstOrDefault(s =>
                s.Guest == res.Guest && s.Room == res.Room && s.Status == StayStatus.Active);
            btnCheckOut.Enabled = stay != null;
        }
        else
        {
            btnCheckOut.Enabled = false;
        }
    }

    private Reservation? GetSelectedReservation()
    {
        if (dgvReservations.CurrentRow == null) return null;
        var idx = dgvReservations.CurrentRow.Index;
        return idx >= 0 && idx < _filteredReservations.Count ? _filteredReservations[idx] : null;
    }

    private void BtnCheckIn_Click(object? sender, EventArgs e)
    {
        var res = GetSelectedReservation();
        if (res == null) return;

        _bookingService.CheckIn(res);
        MessageBox.Show($"Guest {res.Guest.Name} checked into Room {res.Room.Number}.",
            "Check In", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshReservations();
    }

    private void BtnCheckOut_Click(object? sender, EventArgs e)
    {
        var res = GetSelectedReservation();
        if (res == null) return;

        var stay = _store.Stays.FirstOrDefault(s =>
            s.Guest == res.Guest && s.Room == res.Room && s.Status == StayStatus.Active);
        if (stay == null) return;

        _bookingService.CheckOut(stay);
        var invoice = _invoiceService.GenerateInvoice(stay);
        using var form = new CheckoutForm(invoice);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _invoiceService.MarkPaid(invoice, form.SelectedPaymentMethod);
        }
        RefreshReservations();
    }

    private void BtnCancelRes_Click(object? sender, EventArgs e)
    {
        var res = GetSelectedReservation();
        if (res == null) return;

        if (MessageBox.Show($"Cancel reservation for {res.Guest.Name}?",
            "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _bookingService.Cancel(res);
            RefreshReservations();
        }
    }

    // ===================== ROOMS =====================

    private void RefreshRooms()
    {
        RefreshRoomKPIs();
        RefreshRoomCards();
        RefreshRoomDetail();
    }

    private void RefreshRoomKPIs()
    {
        var total = _store.Rooms.Count;
        var occupied = _store.Rooms.Count(r => r.IsOccupied);
        var available = _store.Rooms.Count(r => r.IsAvailable);
        var cleaning = _store.Rooms.Count(r => r.Condition == RoomCondition.NeedsCleaning);
        var oos = _store.Rooms.Count(r => r.Condition == RoomCondition.OutOfService);

        var rate = total > 0 ? (double)occupied / total * 100 : 0;
        lblRoomOccupancyValue.Text = $"{rate:F1}%";
        lblRoomAvailableValue.Text = available.ToString();
        lblRoomCleaningValue.Text = cleaning.ToString();
        lblRoomOOSValue.Text = oos.ToString();
    }

    private void RefreshRoomCards()
    {
        flpRooms.Controls.Clear();
        var typeFilter = cmbRoomTypeFilter.SelectedItem?.ToString() ?? "All Types";
        var statusFilter = cmbRoomStatusFilter.SelectedItem?.ToString() ?? "All Status";

        var rooms = _store.Rooms.AsEnumerable();

        if (typeFilter != "All Types")
            rooms = rooms.Where(r => r.Type.ToString() == typeFilter);

        if (statusFilter != "All Status")
            rooms = rooms.Where(r => r.DisplayStatus == statusFilter);

        foreach (var room in rooms.ToList())
        {
            var card = CreateRoomCard(room);
            flpRooms.Controls.Add(card);
        }
    }

    private Panel CreateRoomCard(Room room)
    {
        var card = new Panel
        {
            Size = new Size(180, 160),
            BackColor = Color.White,
            Margin = new Padding(8),
            Cursor = Cursors.Hand,
            Tag = room
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(
                _selectedRoom == room ? AppColors.Accent : AppColors.Gray200,
                _selectedRoom == room ? 2 : 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var statusColor = AppColors.GetRoomCardColor(room);

        var statusBar = new Panel
        {
            Size = new Size(card.Width, 5),
            BackColor = statusColor,
            Dock = DockStyle.Top
        };

        var lblNum = new Label
        {
            Text = $"Room {room.Number}",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(10, 14),
            AutoSize = true
        };

        var lblType = new Label
        {
            Text = $"{room.Type} · Floor {room.Floor}",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray500,
            Location = new Point(10, 42),
            AutoSize = true
        };

        var lblRate = new Label
        {
            Text = $"${room.Rate}/night",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            Location = new Point(10, 62),
            AutoSize = true
        };

        var displayStatus = room.DisplayStatus;
        var badgeColor = AppColors.GetRoomStatusBadgeColor(displayStatus);
        var lblStatus = new Label
        {
            Text = displayStatus,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = badgeColor,
            AutoSize = false,
            Size = new Size(160, 22),
            Location = new Point(10, 90),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Show guest name if occupied
        var stay = room.IsOccupied ? _roomService.GetCurrentStay(room) : null;
        var lblGuest = new Label
        {
            Text = stay != null ? $"Guest: {stay.Guest.Name}" : "",
            Font = new Font("Segoe UI", 8),
            ForeColor = AppColors.Gray600,
            Location = new Point(10, 118),
            AutoSize = true,
            MaximumSize = new Size(160, 0)
        };

        card.Controls.Add(statusBar);
        card.Controls.Add(lblNum);
        card.Controls.Add(lblType);
        card.Controls.Add(lblRate);
        card.Controls.Add(lblStatus);
        card.Controls.Add(lblGuest);

        // Click handler for card and all child controls
        void onClick(object? s, EventArgs e) { RoomCard_Click(room); }
        card.Click += onClick;
        foreach (Control c in card.Controls)
            c.Click += onClick;

        return card;
    }

    private void RoomCard_Click(Room room)
    {
        _selectedRoom = room;
        RefreshRoomCards();
        RefreshRoomDetail();
    }

    private void RefreshRoomDetail()
    {
        if (_selectedRoom == null)
        {
            lblRoomDetailTitle.Text = "Select a room to view details";
            lblRoomDetailType.Text = "";
            lblRoomDetailRate.Text = "";
            lblRoomDetailStatus.Visible = false;
            lblRoomDetailGuest.Visible = false;
            lblRoomDetailMaintenance.Visible = false;
            btnMarkClean.Visible = false;
            btnMarkNeedsCleaning.Visible = false;
            btnMarkOutOfService.Visible = false;
            return;
        }

        var room = _selectedRoom;
        lblRoomDetailTitle.Text = $"Room {room.Number} - {room.Type}";
        lblRoomDetailType.Text = $"Type: {room.Type}  ·  Floor {room.Floor}";
        lblRoomDetailRate.Text = $"${room.Rate:F2}/night";

        var displayStatus = room.DisplayStatus;
        lblRoomDetailStatus.Text = displayStatus;
        lblRoomDetailStatus.BackColor = AppColors.GetRoomStatusBadgeColor(displayStatus);
        lblRoomDetailStatus.Visible = true;

        var stay = room.IsOccupied ? _roomService.GetCurrentStay(room) : null;
        if (stay != null)
        {
            lblRoomDetailGuest.Text = $"Guest: {stay.Guest.Name}  |  Check-out: {stay.ExpectedCheckOut:MMM dd}";
            lblRoomDetailGuest.Visible = true;
        }
        else
        {
            lblRoomDetailGuest.Visible = false;
        }

        if (!string.IsNullOrEmpty(room.MaintenanceLog))
        {
            lblRoomDetailMaintenance.Text = $"Maintenance: {room.MaintenanceLog}";
            lblRoomDetailMaintenance.Visible = true;
        }
        else
        {
            lblRoomDetailMaintenance.Text = "Maintenance: —";
            lblRoomDetailMaintenance.Visible = true;
        }

        // Show condition buttons
        btnMarkClean.Visible = true;
        btnMarkNeedsCleaning.Visible = true;
        btnMarkOutOfService.Visible = true;
    }

    private void BtnRoomMarkClean_Click(object? sender, EventArgs e)
    {
        if (_selectedRoom == null) return;
        _roomService.MarkClean(_selectedRoom);
        RefreshRooms();
    }

    private void BtnRoomMarkNeedsCleaning_Click(object? sender, EventArgs e)
    {
        if (_selectedRoom == null) return;
        _roomService.MarkNeedsCleaning(_selectedRoom);
        RefreshRooms();
    }

    private void BtnRoomMarkOutOfService_Click(object? sender, EventArgs e)
    {
        if (_selectedRoom == null) return;
        var reason = Microsoft.VisualBasic.Interaction.InputBox("Enter reason:", "Out of Service", "");
        if (!string.IsNullOrEmpty(reason))
        {
            _roomService.MarkOutOfService(_selectedRoom, reason);
            RefreshRooms();
        }
    }

    private void BtnAddRoom_Click(object? sender, EventArgs e)
    {
        ShowRoomDialog(null);
    }

    private void BtnEditRoom_Click(object? sender, EventArgs e)
    {
        if (_selectedRoom == null)
        {
            MessageBox.Show("Select a room first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        ShowRoomDialog(_selectedRoom);
    }

    private void BtnRemoveRoom_Click(object? sender, EventArgs e)
    {
        if (_selectedRoom == null)
        {
            MessageBox.Show("Select a room first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_selectedRoom.IsOccupied)
        {
            MessageBox.Show("Cannot remove an occupied room.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (MessageBox.Show($"Remove Room {_selectedRoom.Number}?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _roomService.RemoveRoom(_selectedRoom);
            _selectedRoom = null;
            RefreshRooms();
        }
    }

    private void ShowRoomDialog(Room? existing)
    {
        using var dlg = new Form
        {
            Text = existing == null ? "Add Room" : "Edit Room",
            Size = new Size(360, 300),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = AppColors.Surface
        };

        var lblN = new Label { Text = "Number:", Font = new Font("Segoe UI", 10), Location = new Point(20, 20), AutoSize = true };
        var nudNum = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(110, 17),
            Size = new Size(100, 28),
            Minimum = 1,
            Maximum = 9999,
            Value = existing?.Number ?? 100
        };

        var lblF = new Label { Text = "Floor:", Font = new Font("Segoe UI", 10), Location = new Point(20, 55), AutoSize = true };
        var nudFloor = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(110, 52),
            Size = new Size(100, 28),
            Minimum = 0,
            Maximum = 99,
            Value = existing?.Floor ?? 1
        };

        var lblT = new Label { Text = "Type:", Font = new Font("Segoe UI", 10), Location = new Point(20, 90), AutoSize = true };
        var cmbType = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(110, 87),
            Size = new Size(200, 28)
        };
        foreach (var rt in Enum.GetValues<RoomType>())
            cmbType.Items.Add(rt);
        cmbType.SelectedItem = existing?.Type ?? RoomType.Single;

        var lblR = new Label { Text = "Rate:", Font = new Font("Segoe UI", 10), Location = new Point(20, 125), AutoSize = true };
        var txtRate = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(110, 122),
            Size = new Size(100, 28),
            Text = existing?.Rate.ToString("F2") ?? "99.99"
        };

        var btnOk = new Button
        {
            Text = existing == null ? "Add" : "Save",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 34),
            Location = new Point(110, 175),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 34),
            Location = new Point(220, 175),
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };

        dlg.Controls.AddRange(new Control[] { lblN, nudNum, lblF, nudFloor, lblT, cmbType, lblR, txtRate, btnOk, btnCancel });
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var number = (int)nudNum.Value;
            var floor = (int)nudFloor.Value;
            var type = (RoomType)cmbType.SelectedItem!;
            if (!decimal.TryParse(txtRate.Text, out var rate) || rate < 0)
            {
                MessageBox.Show("Enter a valid rate.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (existing == null)
                    _roomService.AddRoom(number, floor, type, rate);
                else
                    _roomService.UpdateRoom(existing, number, floor, type, rate);

                RefreshRooms();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void CmbRoomFilter_Changed(object? sender, EventArgs e) => RefreshRooms();

    // ===================== RESTAURANT =====================

    private List<RestaurantOrder> _filteredOrders = new();

    private void RefreshRestaurant()
    {
        RefreshRestKPIs();
        RefreshCategoryTabs();
        RefreshMenuCards();
        RefreshStayCombos();
        RefreshOrdersGrid();
        RefreshOrderDetail();
    }

    private void RefreshRestKPIs()
    {
        var activeCount = _store.Orders.Count(o => o.Status is OrderStatus.Placed or OrderStatus.Preparing);
        var readyCount = _store.Orders.Count(o => o.Status == OrderStatus.Ready);
        var todayRev = _restaurantService.GetTodayServedRevenue();
        var menuCount = _store.MenuItems.Count;

        lblRestKpiActive.Text = activeCount.ToString();
        lblRestKpiReady.Text = readyCount.ToString();
        lblRestKpiRevenue.Text = $"${todayRev:F2}";
        lblRestKpiItems.Text = menuCount.ToString();
    }

    private void RefreshCategoryTabs()
    {
        flpCategoryTabs.Controls.Clear();
        var categories = new List<string> { "All" };
        categories.AddRange(_restaurantService.GetCategories());

        foreach (var cat in categories)
        {
            var btn = new Button
            {
                Text = cat,
                Font = new Font("Segoe UI", 9, cat == _selectedMenuCategory ? FontStyle.Bold : FontStyle.Regular),
                BackColor = cat == _selectedMenuCategory ? AppColors.Primary : AppColors.Gray200,
                ForeColor = cat == _selectedMenuCategory ? Color.White : AppColors.Gray700,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(Math.Max(70, TextRenderer.MeasureText(cat, new Font("Segoe UI", 9)).Width + 24), 28),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 0),
                Tag = cat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += CategoryTab_Click;
            flpCategoryTabs.Controls.Add(btn);
        }
    }

    private void CategoryTab_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string cat) return;
        _selectedMenuCategory = cat;
        RefreshCategoryTabs();
        RefreshMenuCards();
    }

    private void RefreshMenuCards()
    {
        flpMenuCards.SuspendLayout();
        foreach (Control c in flpMenuCards.Controls)
        {
            foreach (Control inner in c.Controls)
            {
                if (inner is PictureBox pb) { pb.Image?.Dispose(); pb.Image = null; }
            }
        }
        flpMenuCards.Controls.Clear();

        var items = _selectedMenuCategory == "All"
            ? _store.MenuItems.ToList()
            : _store.MenuItems.Where(m => m.Category == _selectedMenuCategory).ToList();

        foreach (var item in items)
            flpMenuCards.Controls.Add(CreateMenuItemCard(item));

        flpMenuCards.ResumeLayout();
    }

    private Panel CreateMenuItemCard(Models.MenuItem item)
    {
        var card = new Panel
        {
            Size = new Size(200, 280),
            BackColor = Color.White,
            Margin = new Padding(8),
            Cursor = Cursors.Hand,
            Tag = item
        };
        card.Paint += (s, e) =>
        {
            var selected = _selectedMenuItem == item;
            using var pen = new Pen(selected ? AppColors.Accent : AppColors.Gray200, selected ? 2 : 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var pic = new PictureBox
        {
            Location = new Point(8, 8),
            Size = new Size(184, 130),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = AppColors.Gray100,
            Cursor = Cursors.Hand
        };
        if (!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
        {
            try { pic.Image = Image.FromFile(item.ImagePath); } catch { }
        }

        var lblName = new Label
        {
            Text = item.Name,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(8, 144),
            Size = new Size(184, 18),
            AutoEllipsis = true
        };

        var lblPrice = new Label
        {
            Text = $"${item.Price:F2}",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            Location = new Point(8, 164),
            AutoSize = true
        };

        var lblAvail = new Label
        {
            Text = item.IsAvailable ? "Available" : "Unavailable",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = item.IsAvailable ? AppColors.Tertiary : AppColors.StatusOOS,
            Location = new Point(90, 166),
            Size = new Size(100, 18),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var nudCardQty = new NumericUpDown
        {
            Location = new Point(8, 196),
            Size = new Size(60, 26),
            Font = new Font("Segoe UI", 9),
            Minimum = 1,
            Maximum = 20,
            Value = 1
        };

        var btnAdd = new Button
        {
            Text = "Add to Order",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = item.IsAvailable ? AppColors.Tertiary : AppColors.Gray300,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(118, 30),
            Location = new Point(74, 194),
            Cursor = Cursors.Hand,
            Enabled = item.IsAvailable
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Click += (s, e) =>
        {
            AddItemToOrder(item, (int)nudCardQty.Value);
            nudCardQty.Value = 1;
        };

        card.Controls.Add(pic);
        card.Controls.Add(lblName);
        card.Controls.Add(lblPrice);
        card.Controls.Add(lblAvail);
        card.Controls.Add(nudCardQty);
        card.Controls.Add(btnAdd);

        // Card click → select for management actions (don't intercept clicks on the qty/add button)
        void SelectThis(object? s, EventArgs e) => MenuCard_Click(item);
        card.Click += SelectThis;
        pic.Click += SelectThis;
        lblName.Click += SelectThis;
        lblPrice.Click += SelectThis;
        lblAvail.Click += SelectThis;

        return card;
    }

    private void MenuCard_Click(Models.MenuItem item)
    {
        _selectedMenuItem = item;
        flpMenuCards.Invalidate(true);
        foreach (Control c in flpMenuCards.Controls) c.Invalidate();
    }

    private void AddItemToOrder(Models.MenuItem item, int quantity)
    {
        var line = new OrderLine
        {
            MenuItem = item,
            Quantity = quantity,
            Notes = string.Empty
        };
        _currentOrderLines.Add(line);
        dgvCurrentOrderLines.Rows.Add(line.MenuItem.Name, line.Quantity, $"${line.LineTotal:F2}");
        UpdateRunningTotal();
    }

    private void RefreshStayCombos()
    {
        cmbStay.Items.Clear();
        foreach (var stay in _store.Stays.Where(s => s.Status == StayStatus.Active))
            cmbStay.Items.Add(stay);
        cmbStay.DisplayMember = "DisplayLabel";
        if (cmbStay.Items.Count > 0) cmbStay.SelectedIndex = 0;
    }

    private void RefreshOrdersGrid()
    {
        var filter = cmbOrderFilter.SelectedItem?.ToString() ?? "All";

        _filteredOrders = filter == "All"
            ? _store.Orders.ToList()
            : _store.Orders.Where(o => o.Status.ToString() == filter).ToList();

        dgvOrders.Rows.Clear();
        foreach (var order in _filteredOrders)
        {
            dgvOrders.Rows.Add(
                order.Stay.Guest.Name,
                order.Stay.Room.Number,
                order.ItemCount,
                $"${order.Total:F2}",
                order.Status.ToString());
        }

        UpdateRestaurantButtons();
    }

    private void RefreshOrderDetail()
    {
        var order = GetSelectedOrder();
        if (order == null)
        {
            lblOrderDetailGuest.Text = "Select an order to view details";
            lblOrderDetailStatus.Visible = false;
            pnlStatusProgression.Visible = false;
            dgvOrderLines.Visible = false;
            lblOrderTotal.Visible = false;
            return;
        }

        lblOrderDetailGuest.Text = $"{order.Stay.Guest.Name} - Room {order.Stay.Room.Number}";

        lblOrderDetailStatus.Text = order.Status.ToString();
        lblOrderDetailStatus.BackColor = AppColors.GetOrderStatusColor(order.Status);
        lblOrderDetailStatus.Visible = true;

        pnlStatusProgression.Tag = order.Status;
        pnlStatusProgression.Visible = true;
        pnlStatusProgression.Invalidate();

        dgvOrderLines.Rows.Clear();
        foreach (var line in order.Lines)
        {
            dgvOrderLines.Rows.Add(line.MenuItem.Name, line.Quantity, line.Notes, $"${line.LineTotal:F2}");
        }
        dgvOrderLines.Visible = true;

        lblOrderTotal.Text = $"Order Total: ${order.Total:F2}";
        lblOrderTotal.Visible = true;
    }

    private void UpdateRestaurantButtons()
    {
        var order = GetSelectedOrder();
        btnAdvanceOrder.Enabled = order != null &&
            order.Status is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.Ready;
        btnAddItemsToOrder.Enabled = order != null &&
            order.Status is OrderStatus.Placed or OrderStatus.Preparing;
        btnCancelOrder.Enabled = order != null &&
            order.Status is OrderStatus.Placed or OrderStatus.Preparing;
        UpdateAdvanceButtonText();
    }

    private void UpdateAdvanceButtonText()
    {
        var order = GetSelectedOrder();
        if (order == null)
        {
            btnAdvanceOrder.Text = "Advance Status";
            return;
        }

        btnAdvanceOrder.Text = order.Status switch
        {
            OrderStatus.Placed => "Mark Preparing",
            OrderStatus.Preparing => "Mark Ready",
            OrderStatus.Ready => "Mark Served",
            _ => "Advance Status"
        };
    }

    private void CmbOrderFilter_Changed(object? sender, EventArgs e) => RefreshOrdersGrid();

    private void DgvOrders_SelectionChanged(object? sender, EventArgs e)
    {
        RefreshOrderDetail();
        UpdateRestaurantButtons();
    }

    private void DgvCurrentOrderLines_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == dgvCurrentOrderLines.Columns["Remove"]!.Index && e.RowIndex >= 0)
        {
            _currentOrderLines.RemoveAt(e.RowIndex);
            dgvCurrentOrderLines.Rows.RemoveAt(e.RowIndex);
            UpdateRunningTotal();
        }
    }

    private void BtnClearOrder_Click(object? sender, EventArgs e)
    {
        _currentOrderLines.Clear();
        dgvCurrentOrderLines.Rows.Clear();
        UpdateRunningTotal();
    }

    private void UpdateRunningTotal()
    {
        var total = _currentOrderLines.Sum(l => l.LineTotal);
        lblRunningTotal.Text = $"Total: ${total:F2}";
    }

    private void BtnPlaceOrder_Click(object? sender, EventArgs e)
    {
        if (cmbStay.SelectedItem is not Stay stay)
        {
            MessageBox.Show("Please select a stay.", "Missing Stay",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_currentOrderLines.Count == 0)
        {
            MessageBox.Show("Add at least one item to the order.", "Empty Order",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var order = _restaurantService.CreateOrder(stay, _currentOrderLines);
        stay.RestaurantCharges += order.Total;

        MessageBox.Show($"Order placed: ${order.Total:F2}", "Order Created",
            MessageBoxButtons.OK, MessageBoxIcon.Information);

        _currentOrderLines.Clear();
        dgvCurrentOrderLines.Rows.Clear();
        UpdateRunningTotal();
        RefreshRestKPIs();
        RefreshOrdersGrid();
    }

    private RestaurantOrder? GetSelectedOrder()
    {
        if (dgvOrders.CurrentRow == null) return null;
        var idx = dgvOrders.CurrentRow.Index;
        return idx >= 0 && idx < _filteredOrders.Count ? _filteredOrders[idx] : null;
    }

    private void BtnAdvanceOrder_Click(object? sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) return;

        _restaurantService.AdvanceOrderStatus(order);
        RefreshRestKPIs();
        RefreshOrdersGrid();
        RefreshOrderDetail();
    }

    private void BtnCancelOrder_Click(object? sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) return;

        if (MessageBox.Show("Cancel this order?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _restaurantService.CancelOrder(order);
            RefreshRestKPIs();
            RefreshOrdersGrid();
            RefreshOrderDetail();
        }
    }

    private void BtnAddItemsToOrder_Click(object? sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null || order.Status is not (OrderStatus.Placed or OrderStatus.Preparing)) return;

        using var dlg = new Form
        {
            Text = $"Add Items to Order - {order.Stay.Guest.Name}",
            Size = new Size(450, 400),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = AppColors.Surface
        };

        var cmbDlgItem = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(20, 20),
            Size = new Size(200, 28)
        };
        foreach (var item in _store.MenuItems.Where(m => m.IsAvailable))
            cmbDlgItem.Items.Add(item);
        if (cmbDlgItem.Items.Count > 0) cmbDlgItem.SelectedIndex = 0;

        var nudDlgQty = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(230, 20),
            Size = new Size(55, 28),
            Minimum = 1, Maximum = 20, Value = 1
        };

        var txtDlgNotes = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(295, 20),
            Size = new Size(120, 28),
            PlaceholderText = "Notes"
        };

        var dlgLines = new BindingList<OrderLine>();
        var dgvDlgLines = new DataGridView
        {
            Location = new Point(20, 90),
            Size = new Size(395, 180),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9)
        };
        dgvDlgLines.Columns.Add("Item", "Item");
        dgvDlgLines.Columns.Add("Qty", "Qty");
        dgvDlgLines.Columns["Qty"]!.Width = 40;
        dgvDlgLines.Columns.Add("Notes", "Notes");
        dgvDlgLines.Columns.Add("Total", "Total");
        dgvDlgLines.Columns["Total"]!.Width = 70;

        var btnDlgAdd = new Button
        {
            Text = "Add",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(60, 28),
            Location = new Point(20, 56),
            Cursor = Cursors.Hand
        };
        btnDlgAdd.FlatAppearance.BorderSize = 0;
        btnDlgAdd.Click += (s2, e2) =>
        {
            if (cmbDlgItem.SelectedItem is not Models.MenuItem mi) return;
            var line = new OrderLine { MenuItem = mi, Quantity = (int)nudDlgQty.Value, Notes = txtDlgNotes.Text.Trim() };
            dlgLines.Add(line);
            dgvDlgLines.Rows.Add(mi.Name, line.Quantity, line.Notes, $"${line.LineTotal:F2}");
            nudDlgQty.Value = 1;
            txtDlgNotes.Clear();
        };

        var btnDlgConfirm = new Button
        {
            Text = "Add to Order",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(140, 36),
            Location = new Point(20, 280),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        btnDlgConfirm.FlatAppearance.BorderSize = 0;

        dlg.Controls.AddRange(new Control[] { cmbDlgItem, nudDlgQty, txtDlgNotes, btnDlgAdd, dgvDlgLines, btnDlgConfirm });
        dlg.AcceptButton = btnDlgConfirm;

        if (dlg.ShowDialog(this) == DialogResult.OK && dlgLines.Count > 0)
        {
            _restaurantService.AddLinesToOrder(order, dlgLines);
            RefreshRestKPIs();
            RefreshOrdersGrid();
            RefreshOrderDetail();
        }
    }

    // --- Menu Management ---

    private Models.MenuItem? GetSelectedMenuItem() => _selectedMenuItem;

    private void ShowMenuItemDialog(Models.MenuItem? existing)
    {
        using var dlg = new Form
        {
            Text = existing == null ? "Add Menu Item" : "Edit Menu Item",
            Size = new Size(420, 440),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = AppColors.Surface
        };

        var lblN = new Label { Text = "Name:", Font = new Font("Segoe UI", 10), Location = new Point(20, 20), AutoSize = true };
        var txtName = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(110, 17), Size = new Size(260, 28), Text = existing?.Name ?? "" };

        var lblP = new Label { Text = "Price:", Font = new Font("Segoe UI", 10), Location = new Point(20, 55), AutoSize = true };
        var txtPrice = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(110, 52), Size = new Size(100, 28), Text = existing?.Price.ToString("F2") ?? "0.00" };

        var lblC = new Label { Text = "Category:", Font = new Font("Segoe UI", 10), Location = new Point(20, 90), AutoSize = true };
        var cmbCat = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(110, 87),
            Size = new Size(200, 28)
        };
        foreach (var cat in _restaurantService.GetCategories())
            cmbCat.Items.Add(cat);
        cmbCat.Text = existing?.Category ?? "";

        var chkAvail = new CheckBox
        {
            Text = "Available",
            Font = new Font("Segoe UI", 10),
            Location = new Point(110, 122),
            AutoSize = true,
            Checked = existing?.IsAvailable ?? true
        };

        var lblImg = new Label { Text = "Image:", Font = new Font("Segoe UI", 10), Location = new Point(20, 160), AutoSize = true };
        var picPreview = new PictureBox
        {
            Location = new Point(110, 160),
            Size = new Size(120, 90),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = AppColors.Gray100
        };
        if (!string.IsNullOrEmpty(existing?.ImagePath) && File.Exists(existing.ImagePath))
        {
            try { picPreview.Image = Image.FromFile(existing.ImagePath); } catch { }
        }

        string? pendingImagePath = existing?.ImagePath;
        var btnPickImage = new Button
        {
            Text = "Choose…",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 28),
            Location = new Point(240, 160),
            Cursor = Cursors.Hand
        };
        btnPickImage.FlatAppearance.BorderSize = 0;
        btnPickImage.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select menu item image",
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*"
            };
            if (ofd.ShowDialog(dlg) == DialogResult.OK)
            {
                pendingImagePath = ofd.FileName;
                try
                {
                    picPreview.Image?.Dispose();
                    picPreview.Image = Image.FromFile(ofd.FileName);
                }
                catch { /* ignore preview errors */ }
            }
        };

        var btnClearImage = new Button
        {
            Text = "Clear",
            Font = new Font("Segoe UI", 9),
            BackColor = AppColors.Gray200,
            ForeColor = AppColors.Gray800,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(70, 28),
            Location = new Point(240, 195),
            Cursor = Cursors.Hand
        };
        btnClearImage.FlatAppearance.BorderSize = 0;
        btnClearImage.Click += (s, e) =>
        {
            pendingImagePath = null;
            picPreview.Image?.Dispose();
            picPreview.Image = null;
        };

        var btnOk = new Button
        {
            Text = existing == null ? "Add" : "Save",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 34),
            Location = new Point(110, 320),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 34),
            Location = new Point(220, 320),
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };

        dlg.Controls.AddRange(new Control[] {
            lblN, txtName, lblP, txtPrice, lblC, cmbCat, chkAvail,
            lblImg, picPreview, btnPickImage, btnClearImage,
            btnOk, btnCancel
        });
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out var price) || price < 0)
            {
                MessageBox.Show("Enter a valid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var category = cmbCat.Text.Trim();
            if (string.IsNullOrEmpty(category)) category = "Uncategorized";

            // If user picked a new image path that isn't already in MenuImages, copy it there
            string? finalImagePath = pendingImagePath;
            if (!string.IsNullOrEmpty(pendingImagePath) && pendingImagePath != existing?.ImagePath)
            {
                finalImagePath = CopyMenuImage(pendingImagePath);
            }

            if (existing == null)
            {
                _restaurantService.AddMenuItem(new Models.MenuItem
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    IsAvailable = chkAvail.Checked,
                    ImagePath = finalImagePath
                });
            }
            else
            {
                _restaurantService.UpdateMenuItem(existing, name, price, category, chkAvail.Checked);
                existing.ImagePath = finalImagePath;
            }

            RefreshRestaurant();
        }
    }

    private static string CopyMenuImage(string sourcePath)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "MenuImages");
        Directory.CreateDirectory(folder);
        var ext = Path.GetExtension(sourcePath);
        var dest = Path.Combine(folder, $"{Guid.NewGuid():N}{ext}");
        File.Copy(sourcePath, dest, overwrite: false);
        return dest;
    }

    private void BtnAddMenuItem_Click(object? sender, EventArgs e) => ShowMenuItemDialog(null);

    private void BtnEditMenuItem_Click(object? sender, EventArgs e)
    {
        var item = GetSelectedMenuItem();
        if (item == null)
        {
            MessageBox.Show("Select a menu item first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        ShowMenuItemDialog(item);
    }

    private void BtnToggleAvail_Click(object? sender, EventArgs e)
    {
        var item = GetSelectedMenuItem();
        if (item == null) return;
        _restaurantService.ToggleAvailability(item);
        RefreshMenuCards();
        RefreshStayCombos();
        RefreshRestKPIs();
    }

    private void BtnRemoveMenuItem_Click(object? sender, EventArgs e)
    {
        var item = GetSelectedMenuItem();
        if (item == null) return;
        if (MessageBox.Show($"Remove '{item.Name}' from the menu?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _restaurantService.RemoveMenuItem(item);
            RefreshCategoryTabs();
            RefreshMenuCards();
            RefreshStayCombos();
            RefreshRestKPIs();
        }
    }

    // --- Status Progression Painter ---

    private void PaintStatusProgression(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel pnl) return;
        var status = pnl.Tag is OrderStatus os ? os : OrderStatus.Placed;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var statuses = new[] { OrderStatus.Placed, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Served };
        var labels = new[] { "Placed", "Preparing", "Ready", "Served" };
        var circleSize = 18;
        var spacing = (pnl.Width - 40) / (statuses.Length - 1);
        var y = pnl.Height / 2;

        var currentIdx = Array.IndexOf(statuses, status);
        if (status == OrderStatus.Cancelled) currentIdx = -1;

        for (int i = 0; i < statuses.Length; i++)
        {
            var cx = 20 + i * spacing;

            // Draw connecting line
            if (i > 0)
            {
                var prevX = 20 + (i - 1) * spacing;
                var lineColor = i <= currentIdx ? AppColors.Tertiary : AppColors.Gray300;
                using var pen = new Pen(lineColor, 2);
                g.DrawLine(pen, prevX + circleSize / 2, y, cx - circleSize / 2, y);
            }

            // Draw circle
            var filled = i <= currentIdx;
            var circleRect = new Rectangle(cx - circleSize / 2, y - circleSize / 2, circleSize, circleSize);
            if (filled)
            {
                using var brush = new SolidBrush(AppColors.Tertiary);
                g.FillEllipse(brush, circleRect);
                // Checkmark for completed stages
                if (i < currentIdx)
                {
                    using var checkPen = new Pen(Color.White, 2);
                    g.DrawLine(checkPen, cx - 4, y, cx - 1, y + 3);
                    g.DrawLine(checkPen, cx - 1, y + 3, cx + 5, y - 4);
                }
                else
                {
                    // Current stage - filled dot
                    using var dotBrush = new SolidBrush(Color.White);
                    g.FillEllipse(dotBrush, cx - 3, y - 3, 6, 6);
                }
            }
            else
            {
                using var pen = new Pen(AppColors.Gray300, 2);
                g.DrawEllipse(pen, circleRect);
            }

            // Label
            var labelColor = i <= currentIdx ? AppColors.Primary : AppColors.Gray400;
            TextRenderer.DrawText(g, labels[i], new Font("Segoe UI", 7),
                new Point(cx - 20, y + circleSize / 2 + 2), labelColor);
        }
    }

    // ===================== FINANCES =====================

    private List<Invoice> _filteredInvoices = new();

    private void RefreshFinances()
    {
        RefreshFinanceKPIs();
        RefreshInvoiceGrid();
    }

    private void RefreshFinanceKPIs()
    {
        lblFinTotalRev.Text = $"${_invoiceService.GetTotalRevenue():F2}";
        lblFinUnpaid.Text = _invoiceService.GetUnpaidInvoices().Count.ToString();
        lblFinPaidToday.Text = $"${_invoiceService.GetTodayRevenue():F2}";
        lblFinOutstanding.Text = $"${_invoiceService.GetOutstandingAmount():F2}";
    }

    private void RefreshInvoiceGrid()
    {
        var filter = cmbFinFilter.SelectedItem?.ToString() ?? "All";

        _filteredInvoices = filter == "All"
            ? _store.Invoices.ToList()
            : _store.Invoices
                .Where(i => i.PaymentStatus.ToString() == filter)
                .ToList();

        dgvInvoices.Rows.Clear();
        foreach (var inv in _filteredInvoices)
        {
            dgvInvoices.Rows.Add(
                inv.InvoiceNumber,
                inv.Guest.Name,
                inv.Room.Number,
                $"${inv.Total:F2}",
                inv.PaymentStatus.ToString());
        }

        UpdateFinanceButtons();
    }

    private void UpdateFinanceButtons()
    {
        var inv = GetSelectedInvoice();
        btnViewInvoice.Enabled = inv != null;
        btnMarkPaid.Enabled = inv != null && inv.PaymentStatus == PaymentStatus.Pending;
    }

    private Invoice? GetSelectedInvoice()
    {
        if (dgvInvoices.CurrentRow == null) return null;
        var idx = dgvInvoices.CurrentRow.Index;
        return idx >= 0 && idx < _filteredInvoices.Count ? _filteredInvoices[idx] : null;
    }

    private void CmbFinFilter_Changed(object? sender, EventArgs e) => RefreshInvoiceGrid();

    private void DgvInvoices_SelectionChanged(object? sender, EventArgs e) => UpdateFinanceButtons();

    private void BtnViewInvoice_Click(object? sender, EventArgs e)
    {
        var inv = GetSelectedInvoice();
        if (inv == null) return;

        using var form = new CheckoutForm(inv, isReadOnly: true);
        form.ShowDialog(this);
    }

    private void BtnMarkPaid_Click(object? sender, EventArgs e)
    {
        var inv = GetSelectedInvoice();
        if (inv == null || inv.PaymentStatus != PaymentStatus.Pending) return;

        using var picker = new Form
        {
            Text = "Select Payment Method",
            Size = new Size(320, 180),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var cmb = new ComboBox
        {
            Font = new Font("Segoe UI", 11),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(20, 30),
            Size = new Size(260, 30)
        };
        foreach (var pm in Enum.GetValues<PaymentMethod>())
            cmb.Items.Add(pm.ToString());
        cmb.SelectedIndex = 0;

        var btnOk = new Button
        {
            Text = "Mark as Paid",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(140, 36),
            Location = new Point(20, 80),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        btnOk.FlatAppearance.BorderSize = 0;

        picker.Controls.Add(cmb);
        picker.Controls.Add(btnOk);
        picker.AcceptButton = btnOk;

        if (picker.ShowDialog(this) == DialogResult.OK)
        {
            var method = Enum.Parse<PaymentMethod>(cmb.SelectedItem!.ToString()!);
            _invoiceService.MarkPaid(inv, method);
            RefreshFinances();
        }
    }

    // ===================== REPORTS =====================

    private void RefreshReports()
    {
        lblOccReportValue.Text = $"{_reportService.GetOccupancyRate():F1}%";
        lblAvgStayValue.Text = $"{_reportService.GetAverageStayDuration():F1}d";
        lblRepeatGuestValue.Text = $"{_reportService.GetRepeatGuestPercentage():F1}%";

        // Total revenue
        var totalRoom = _store.Stays.Sum(s => s.RoomCharges);
        var totalRest = _store.Stays.Sum(s => s.RestaurantCharges);
        lblTotalRevenueValue.Text = $"${totalRoom + totalRest:F2}";

        // Revenue by room type
        var revRoom = _reportService.GetRevenueByRoomType();
        lblRevRoom.Text = string.Join("\n", revRoom.Select(kv => $"{kv.Key}: ${kv.Value:F2}"));

        // Restaurant revenue by category
        var revCat = _reportService.GetRestaurantRevenueByCategory();
        lblRevRestaurant.Text = string.Join("\n", revCat.Select(kv => $"{kv.Key}: ${kv.Value:F2}"));

        // Top menu items
        var topItems = _reportService.GetTopMenuItems();
        lblTopItems.Text = string.Join("\n", topItems.Select(t => $"{t.Name}: {t.Count} orders"));
    }

    private void BtnRefreshReports_Click(object? sender, EventArgs e) => RefreshReports();

    // ===================== USERS & ROLES =====================

    private void RefreshUsers()
    {
        if (!_authService.Can(PermissionResource.Users, PermissionAction.Read)) return;
        RefreshUsersGrid();
        RefreshRolesGrid();
    }

    private void RefreshUsersGrid()
    {
        dgvUsers.Rows.Clear();
        foreach (var u in _userService.GetUsers())
        {
            dgvUsers.Rows.Add(u.Username, u.Role?.Name ?? "(none)", u.Role?.Permissions.Count ?? 0);
        }
    }

    private void RefreshRolesGrid()
    {
        dgvRoles.Rows.Clear();
        foreach (var r in _userService.GetRoles())
        {
            dgvRoles.Rows.Add(r.Name, r.IsSystem ? "System" : "—", $"{r.Permissions.Count} / {Permission.All().Count()}");
        }
    }

    private User? GetSelectedUser()
    {
        if (dgvUsers.CurrentRow == null) return null;
        var idx = dgvUsers.CurrentRow.Index;
        var list = _userService.GetUsers().ToList();
        return idx >= 0 && idx < list.Count ? list[idx] : null;
    }

    private Role? GetSelectedRole()
    {
        if (dgvRoles.CurrentRow == null) return null;
        var idx = dgvRoles.CurrentRow.Index;
        var list = _userService.GetRoles().ToList();
        return idx >= 0 && idx < list.Count ? list[idx] : null;
    }

    private void BtnAddUser_Click(object? sender, EventArgs e) => ShowUserDialog(null);

    private void BtnEditUser_Click(object? sender, EventArgs e)
    {
        var u = GetSelectedUser();
        if (u == null) { Warn("Select a user first."); return; }
        ShowUserDialog(u);
    }

    private void BtnRemoveUser_Click(object? sender, EventArgs e)
    {
        var u = GetSelectedUser();
        if (u == null) { Warn("Select a user first."); return; }
        if (MessageBox.Show($"Remove user '{u.Username}'?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            _userService.RemoveUser(u);
            RefreshUsers();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void BtnAddRole_Click(object? sender, EventArgs e) => ShowRoleDialog(null);

    private void BtnEditRole_Click(object? sender, EventArgs e)
    {
        var r = GetSelectedRole();
        if (r == null) { Warn("Select a role first."); return; }
        if (r.IsSystem) { Warn($"The system role '{r.Name}' cannot be modified."); return; }
        ShowRoleDialog(r);
    }

    private void BtnRemoveRole_Click(object? sender, EventArgs e)
    {
        var r = GetSelectedRole();
        if (r == null) { Warn("Select a role first."); return; }
        if (MessageBox.Show($"Remove role '{r.Name}'?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            _userService.RemoveRole(r);
            RefreshUsers();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ShowUserDialog(User? existing)
    {
        using var dlg = new Form
        {
            Text = existing == null ? "Add User" : "Edit User",
            Size = new Size(420, 320),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = AppColors.Surface
        };

        var lblU = new Label { Text = "Username:", Font = new Font("Segoe UI", 10), Location = new Point(20, 20), AutoSize = true };
        var txtU = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(130, 17), Size = new Size(250, 28), Text = existing?.Username ?? "" };

        var lblP = new Label
        {
            Text = existing == null ? "Password:" : "New password:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(20, 55),
            AutoSize = true
        };
        var txtP = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(130, 52), Size = new Size(250, 28), UseSystemPasswordChar = true };
        var lblPHint = new Label
        {
            Text = existing == null ? "" : "(leave blank to keep current)",
            Font = new Font("Segoe UI", 8, FontStyle.Italic),
            ForeColor = AppColors.Gray500,
            Location = new Point(130, 82),
            AutoSize = true
        };

        var lblR = new Label { Text = "Role:", Font = new Font("Segoe UI", 10), Location = new Point(20, 110), AutoSize = true };
        var cmbR = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(130, 107),
            Size = new Size(250, 28),
            DisplayMember = nameof(Role.Name)
        };
        foreach (var role in _userService.GetRoles()) cmbR.Items.Add(role);
        if (existing != null) cmbR.SelectedItem = existing.Role;
        else if (cmbR.Items.Count > 0) cmbR.SelectedIndex = 0;

        var btnOk = new Button
        {
            Text = existing == null ? "Add" : "Save",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 36),
            Location = new Point(180, 170),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(90, 36),
            Location = new Point(290, 170),
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };

        dlg.Controls.AddRange(new Control[] { lblU, txtU, lblP, txtP, lblPHint, lblR, cmbR, btnOk, btnCancel });
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var role = cmbR.SelectedItem as Role;
            if (existing == null)
                _userService.AddUser(txtU.Text.Trim(), txtP.Text, role!);
            else
                _userService.UpdateUser(existing, txtU.Text.Trim(), txtP.Text.Length > 0 ? txtP.Text : null, role!);
            RefreshUsers();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ShowRoleDialog(Role? existing)
    {
        using var dlg = new Form
        {
            Text = existing == null ? "Add Role" : $"Edit Role — {existing.Name}",
            Size = new Size(620, 480),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = AppColors.Surface
        };

        var lblN = new Label { Text = "Role name:", Font = new Font("Segoe UI", 10), Location = new Point(20, 20), AutoSize = true };
        var txtN = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(130, 17), Size = new Size(300, 28), Text = existing?.Name ?? "" };

        var lblM = new Label
        {
            Text = "Permissions (check the actions this role can perform):",
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = AppColors.Gray600,
            Location = new Point(20, 56),
            AutoSize = true
        };

        // Permission matrix: rows = resources, columns = actions
        var grid = new TableLayoutPanel
        {
            Location = new Point(20, 80),
            Size = new Size(560, 290),
            ColumnCount = 5,
            RowCount = Enum.GetValues<PermissionResource>().Length + 1,
            BackColor = Color.White,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        for (int i = 0; i < 4; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));

        // Header row
        grid.Controls.Add(new Label { Text = "Resource", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, BackColor = AppColors.Primary, ForeColor = Color.White }, 0, 0);
        int col = 1;
        foreach (var a in Enum.GetValues<PermissionAction>())
        {
            grid.Controls.Add(new Label { Text = a.ToString(), Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, BackColor = AppColors.Primary, ForeColor = Color.White }, col++, 0);
        }

        var checkboxes = new Dictionary<Permission, CheckBox>();
        int row = 1;
        foreach (var r in Enum.GetValues<PermissionResource>())
        {
            grid.Controls.Add(new Label
            {
                Text = r.ToString(),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Dock = DockStyle.Fill
            }, 0, row);

            col = 1;
            foreach (var a in Enum.GetValues<PermissionAction>())
            {
                var perm = new Permission(r, a);
                var cb = new CheckBox
                {
                    Dock = DockStyle.Fill,
                    CheckAlign = ContentAlignment.MiddleCenter,
                    Checked = existing?.Has(r, a) ?? false,
                    Enabled = !(existing?.IsSystem ?? false)
                };
                checkboxes[perm] = cb;
                grid.Controls.Add(cb, col++, row);
            }
            row++;
        }

        var btnOk = new Button
        {
            Text = existing == null ? "Add" : "Save",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 36),
            Location = new Point(380, 390),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand,
            Enabled = !(existing?.IsSystem ?? false)
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(90, 36),
            Location = new Point(500, 390),
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };

        dlg.Controls.AddRange(new Control[] { lblN, txtN, lblM, grid, btnOk, btnCancel });
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var selected = checkboxes.Where(kv => kv.Value.Checked).Select(kv => kv.Key);
            if (existing == null)
                _userService.AddRole(txtN.Text.Trim(), selected);
            else
                _userService.UpdateRole(existing, txtN.Text.Trim(), selected);
            RefreshUsers();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void Warn(string msg) =>
        MessageBox.Show(this, msg, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void ShowError(Exception ex) =>
        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
