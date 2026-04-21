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
    private readonly DataStore _store;

    private Guest? _lookedUpGuest;
    private readonly BindingList<OrderLine> _currentOrderLines = new();

    public MainForm(
        AuthService authService,
        RoomService roomService,
        BookingService bookingService,
        RestaurantService restaurantService,
        ReportService reportService,
        DataStore dataStore)
    {
        _authService = authService;
        _roomService = roomService;
        _bookingService = bookingService;
        _restaurantService = restaurantService;
        _reportService = reportService;
        _store = dataStore;

        InitializeComponent();

        // Position logout button
        btnLogout.Location = new Point(panelHeader.Width - btnLogout.Width - 12, 8);

        // Hide Reports tab for non-managers
        if (_authService.CurrentUser != null && !_authService.CurrentUser.IsManager)
        {
            tabMain.TabPages.Remove(tabReports);
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
        lblOccupancyValue.Text = $"{_reportService.GetOccupancyRate():F1}%";

        var today = DateTime.Today;
        var arrivals = _store.Reservations.Count(r =>
            r.CheckInDate.Date == today &&
            r.Status == ReservationStatus.Confirmed);
        lblArrivalsValue.Text = arrivals.ToString();

        var departures = _store.Stays.Count(s =>
            s.ExpectedCheckOut.Date == today &&
            s.Status == StayStatus.Active);
        lblDeparturesValue.Text = departures.ToString();

        var cleaning = _store.Rooms.Count(r => r.Status == RoomStatus.NeedsCleaning);
        lblHousekeepingValue.Text = cleaning.ToString();

        var activeOrders = _store.Orders.Count(o =>
            o.Status is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.Ready);
        lblActiveOrdersValue.Text = activeOrders.ToString();
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

        var total = _bookingService.CheckOut(stay);
        MessageBox.Show($"Guest {stay.Guest.Name} checked out.\nTotal charges: ${total:F2}",
            "Check Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    private void RefreshRestaurant()
    {
        // Menu
        dgvMenu.Rows.Clear();
        foreach (var item in _store.MenuItems)
            dgvMenu.Rows.Add(item.Name, item.Category, $"${item.Price:F2}");

        // Stays combo
        cmbStay.Items.Clear();
        foreach (var stay in _store.Stays.Where(s => s.Status == StayStatus.Active))
            cmbStay.Items.Add(stay);
        cmbStay.DisplayMember = "DisplayLabel";
        if (cmbStay.Items.Count > 0) cmbStay.SelectedIndex = 0;

        // MenuItem combo
        cmbMenuItem.Items.Clear();
        foreach (var item in _store.MenuItems.Where(m => m.IsAvailable))
            cmbMenuItem.Items.Add(item);
        if (cmbMenuItem.Items.Count > 0) cmbMenuItem.SelectedIndex = 0;

        // Orders
        RefreshOrdersGrid();
    }

    private void RefreshOrdersGrid()
    {
        dgvOrders.Rows.Clear();
        foreach (var order in _store.Orders.Where(o =>
            o.Status is not OrderStatus.Cancelled))
        {
            dgvOrders.Rows.Add(
                order.Stay.Guest.Name,
                order.Stay.Room.Number,
                $"${order.Total:F2}",
                order.Status.ToString());
        }
    }

    private void BtnAddLine_Click(object? sender, EventArgs e)
    {
        if (cmbMenuItem.SelectedItem is not Models.MenuItem menuItem) return;

        var line = new OrderLine
        {
            MenuItem = menuItem,
            Quantity = (int)nudQty.Value
        };
        _currentOrderLines.Add(line);
        lstOrderLines.Items.Add(line.ToString());
        nudQty.Value = 1;
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
        lstOrderLines.Items.Clear();
        RefreshOrdersGrid();
    }

    private RestaurantOrder? GetSelectedOrder()
    {
        if (dgvOrders.CurrentRow == null) return null;
        var idx = dgvOrders.CurrentRow.Index;
        var visibleOrders = _store.Orders
            .Where(o => o.Status is not OrderStatus.Cancelled)
            .ToList();
        return idx >= 0 && idx < visibleOrders.Count ? visibleOrders[idx] : null;
    }

    private void BtnAdvanceOrder_Click(object? sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) return;

        _restaurantService.AdvanceOrderStatus(order);
        RefreshOrdersGrid();
    }

    private void BtnCancelOrder_Click(object? sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) return;

        if (MessageBox.Show("Cancel this order?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _restaurantService.CancelOrder(order);
            RefreshOrdersGrid();
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
