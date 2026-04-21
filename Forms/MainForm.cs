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
    private readonly DataStore _store;

    private Guest? _lookedUpGuest;
    private readonly BindingList<OrderLine> _currentOrderLines = new();

    public MainForm(
        AuthService authService,
        RoomService roomService,
        BookingService bookingService,
        RestaurantService restaurantService,
        ReportService reportService,
        InvoiceService invoiceService,
        DataStore dataStore)
    {
        _authService = authService;
        _roomService = roomService;
        _bookingService = bookingService;
        _restaurantService = restaurantService;
        _reportService = reportService;
        _invoiceService = invoiceService;
        _store = dataStore;

        InitializeComponent();

        // Position logout button
        btnLogout.Location = new Point(panelHeader.Width - btnLogout.Width - 12, 8);

        // Hide Reports tab and menu management for non-managers
        if (_authService.CurrentUser != null && !_authService.CurrentUser.IsManager)
        {
            tabMain.TabPages.Remove(tabReports);
            btnAddMenuItem.Visible = false;
            btnEditMenuItem.Visible = false;
            btnToggleAvail.Visible = false;
            btnRemoveMenuItem.Visible = false;
        }

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

        var available = _store.Rooms.Count(r => r.Status == RoomStatus.Available);
        lblAvailableValue.Text = available.ToString();

        var occupied = _store.Rooms.Count(r => r.Status == RoomStatus.Occupied);
        lblOccupiedValue.Text = occupied.ToString();

        var oos = _store.Rooms.Count(r => r.Status == RoomStatus.OutOfService);
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
            .Where(r => r.Status == RoomStatus.NeedsCleaning)
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
            dgvReservations.Rows.Add(
                r.Room.Number,
                r.Guest.Name,
                r.Guest.Contact,
                r.CheckInDate.ToShortDateString(),
                r.CheckOutDate.ToShortDateString(),
                r.Status.ToString());
        }

        lblResStatus.Text = $"{_filteredReservations.Count} reservation(s)";
        UpdateResButtons();
        RefreshRoomCombo();
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

    private void RefreshRoomCombo()
    {
        cmbRoom.Items.Clear();
        foreach (var room in _roomService.GetAvailableRooms())
            cmbRoom.Items.Add(room);
        if (cmbRoom.Items.Count > 0) cmbRoom.SelectedIndex = 0;
    }

    private void CmbResFilter_Changed(object? sender, EventArgs e) => RefreshReservations();

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

    private void BtnLookup_Click(object? sender, EventArgs e)
    {
        var phone = txtPhone.Text.Trim();
        if (string.IsNullOrEmpty(phone))
        {
            lblGuestStatus.Text = "Enter a phone number.";
            return;
        }

        _lookedUpGuest = _store.Guests.FirstOrDefault(g =>
            g.Contact.Equals(phone, StringComparison.OrdinalIgnoreCase));

        if (_lookedUpGuest != null)
        {
            lblGuestStatus.Text = $"Found: {_lookedUpGuest.Name}";
            lblGuestStatus.ForeColor = AppColors.Tertiary;
            txtGuestName.Visible = false;
        }
        else
        {
            lblGuestStatus.Text = "New guest — enter name:";
            lblGuestStatus.ForeColor = AppColors.StatusClean;
            txtGuestName.Visible = true;
            txtGuestName.Focus();
        }
    }

    private void BtnCreateRes_Click(object? sender, EventArgs e)
    {
        if (_lookedUpGuest == null && string.IsNullOrWhiteSpace(txtGuestName.Text))
        {
            MessageBox.Show("Please lookup a guest by phone first.", "Missing Guest",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cmbRoom.SelectedItem is not Room room)
        {
            MessageBox.Show("Please select a room.", "Missing Room",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (dtpCheckOut.Value.Date <= dtpCheckIn.Value.Date)
        {
            MessageBox.Show("Check-out must be after check-in.", "Invalid Dates",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var guest = _lookedUpGuest;
        if (guest == null)
        {
            guest = new Guest
            {
                Name = txtGuestName.Text.Trim(),
                Contact = txtPhone.Text.Trim()
            };
            _store.Guests.Add(guest);
        }

        _bookingService.CreateReservation(guest, room, dtpCheckIn.Value.Date, dtpCheckOut.Value.Date);
        MessageBox.Show($"Reservation created for {guest.Name} in Room {room.Number}.",
            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Reset
        txtPhone.Clear();
        txtGuestName.Clear();
        txtGuestName.Visible = false;
        lblGuestStatus.Text = "";
        _lookedUpGuest = null;
        RefreshReservations();
    }

    // ===================== ROOMS =====================

    private void RefreshRooms()
    {
        flpRooms.Controls.Clear();
        var filter = cmbRoomFilter.SelectedItem?.ToString() ?? "All";

        var rooms = filter == "All"
            ? _store.Rooms.ToList()
            : _store.Rooms.Where(r => r.Status.ToString() == filter).ToList();

        foreach (var room in rooms)
        {
            var card = CreateRoomCard(room);
            flpRooms.Controls.Add(card);
        }
    }

    private Panel CreateRoomCard(Room room)
    {
        var card = new Panel
        {
            Size = new Size(180, 130),
            BackColor = Color.White,
            Margin = new Padding(8),
            Cursor = Cursors.Hand
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(AppColors.Gray200, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var statusColor = AppColors.GetRoomStatusColor(room.Status);

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
            Text = room.Type.ToString(),
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

        var lblStatus = new Label
        {
            Text = room.Status.ToString(),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = statusColor,
            AutoSize = false,
            Size = new Size(120, 22),
            Location = new Point(10, 90),
            TextAlign = ContentAlignment.MiddleCenter
        };

        card.Controls.Add(statusBar);
        card.Controls.Add(lblNum);
        card.Controls.Add(lblType);
        card.Controls.Add(lblRate);
        card.Controls.Add(lblStatus);

        // Context menu
        var ctx = new ContextMenuStrip();
        ctx.Items.Add("Mark Available / Clean", null, (s, e) => { _roomService.MarkClean(room); RefreshRooms(); });
        ctx.Items.Add("Mark Needs Cleaning", null, (s, e) => { _roomService.MarkNeedsCleaning(room); RefreshRooms(); });
        ctx.Items.Add("Mark Out of Service", null, (s, e) =>
        {
            var reason = Microsoft.VisualBasic.Interaction.InputBox("Enter reason:", "Out of Service", "");
            if (!string.IsNullOrEmpty(reason))
            {
                _roomService.MarkOutOfService(room, reason);
                RefreshRooms();
            }
        });
        card.ContextMenuStrip = ctx;

        return card;
    }

    private void CmbRoomFilter_Changed(object? sender, EventArgs e) => RefreshRooms();

    // ===================== RESTAURANT =====================

    private List<RestaurantOrder> _filteredOrders = new();

    private void RefreshRestaurant()
    {
        RefreshRestKPIs();
        RefreshCategoryTabs();
        RefreshMenuGrid();
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
        RefreshMenuGrid();
    }

    private void RefreshMenuGrid()
    {
        dgvMenu.Rows.Clear();
        var items = _selectedMenuCategory == "All"
            ? _store.MenuItems.ToList()
            : _store.MenuItems.Where(m => m.Category == _selectedMenuCategory).ToList();

        foreach (var item in items)
            dgvMenu.Rows.Add(item.Name, item.Category, $"${item.Price:F2}", item.IsAvailable ? "Yes" : "No");
    }

    private void RefreshStayCombos()
    {
        cmbStay.Items.Clear();
        foreach (var stay in _store.Stays.Where(s => s.Status == StayStatus.Active))
            cmbStay.Items.Add(stay);
        cmbStay.DisplayMember = "DisplayLabel";
        if (cmbStay.Items.Count > 0) cmbStay.SelectedIndex = 0;

        cmbMenuItem.Items.Clear();
        foreach (var item in _store.MenuItems.Where(m => m.IsAvailable))
            cmbMenuItem.Items.Add(item);
        if (cmbMenuItem.Items.Count > 0) cmbMenuItem.SelectedIndex = 0;
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

    private void BtnAddLine_Click(object? sender, EventArgs e)
    {
        if (cmbMenuItem.SelectedItem is not Models.MenuItem menuItem) return;

        var line = new OrderLine
        {
            MenuItem = menuItem,
            Quantity = (int)nudQty.Value,
            Notes = txtLineNotes.Text.Trim()
        };
        _currentOrderLines.Add(line);
        dgvCurrentOrderLines.Rows.Add(line.MenuItem.Name, line.Quantity, line.Notes, $"${line.LineTotal:F2}");
        nudQty.Value = 1;
        txtLineNotes.Clear();
        UpdateRunningTotal();
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

    private Models.MenuItem? GetSelectedMenuItem()
    {
        if (dgvMenu.CurrentRow == null) return null;
        var idx = dgvMenu.CurrentRow.Index;
        var items = _selectedMenuCategory == "All"
            ? _store.MenuItems.ToList()
            : _store.MenuItems.Where(m => m.Category == _selectedMenuCategory).ToList();
        return idx >= 0 && idx < items.Count ? items[idx] : null;
    }

    private void ShowMenuItemDialog(Models.MenuItem? existing)
    {
        using var dlg = new Form
        {
            Text = existing == null ? "Add Menu Item" : "Edit Menu Item",
            Size = new Size(340, 280),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = AppColors.Surface
        };

        var lblN = new Label { Text = "Name:", Font = new Font("Segoe UI", 10), Location = new Point(20, 20), AutoSize = true };
        var txtName = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(100, 17), Size = new Size(200, 28), Text = existing?.Name ?? "" };

        var lblP = new Label { Text = "Price:", Font = new Font("Segoe UI", 10), Location = new Point(20, 55), AutoSize = true };
        var txtPrice = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(100, 52), Size = new Size(100, 28), Text = existing?.Price.ToString("F2") ?? "0.00" };

        var lblC = new Label { Text = "Category:", Font = new Font("Segoe UI", 10), Location = new Point(20, 90), AutoSize = true };
        var cmbCat = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(100, 87),
            Size = new Size(200, 28)
        };
        foreach (var cat in _restaurantService.GetCategories())
            cmbCat.Items.Add(cat);
        cmbCat.Text = existing?.Category ?? "";

        var chkAvail = new CheckBox
        {
            Text = "Available",
            Font = new Font("Segoe UI", 10),
            Location = new Point(100, 122),
            AutoSize = true,
            Checked = existing?.IsAvailable ?? true
        };

        var btnOk = new Button
        {
            Text = existing == null ? "Add" : "Save",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 34),
            Location = new Point(100, 160),
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
            Location = new Point(210, 160),
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };

        dlg.Controls.AddRange(new Control[] { lblN, txtName, lblP, txtPrice, lblC, cmbCat, chkAvail, btnOk, btnCancel });
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

            if (existing == null)
            {
                _restaurantService.AddMenuItem(new Models.MenuItem
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    IsAvailable = chkAvail.Checked
                });
            }
            else
            {
                _restaurantService.UpdateMenuItem(existing, name, price, category, chkAvail.Checked);
            }

            RefreshCategoryTabs();
            RefreshMenuGrid();
            RefreshStayCombos();
            RefreshRestKPIs();
        }
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
        RefreshMenuGrid();
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
            RefreshMenuGrid();
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
}
