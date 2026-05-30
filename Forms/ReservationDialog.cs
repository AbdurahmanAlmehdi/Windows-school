using System.ComponentModel;
using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Services;
using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

public class ReservationDialog : Form
{
    private readonly DataStore _store;
    private readonly RoomService _roomService;
    private readonly BookingService _bookingService;
    private readonly Room? _preselectedRoom;

    private Guest? _lookedUpGuest;

    private TextBox _txtPassport = null!;
    private Button _btnLookup = null!;
    private Label _lblGuestStatus = null!;
    private TextBox _txtGuestName = null!;
    private ComboBox _cmbGuestGender = null!;
    private TextBox _txtPhone = null!;

    private ComboBox _cmbRoomNumber = null!;
    private TextBox _txtRoomType = null!;
    private TextBox _txtRoomRate = null!;
    private TextBox _txtRoomCapacity = null!;

    private DateTimePicker _dtpCheckIn = null!;
    private DateTimePicker _dtpCheckOut = null!;

    private DataGridView _dgvAccompanying = null!;
    private BindingList<AccompanyingGuest> _accompanying = new();
    private Button _btnAddPerson = null!;
    private Button _btnRemovePerson = null!;
    private Label _lblCapacityStatus = null!;

    private Panel _pnlCertificate = null!;
    private TextBox _txtCertificateId = null!;

    private Button _btnSave = null!;
    private Button _btnCancel = null!;

    public ReservationDialog(
        DataStore store,
        RoomService roomService,
        BookingService bookingService,
        Room? preselectedRoom = null)
    {
        _store = store;
        _roomService = roomService;
        _bookingService = bookingService;
        _preselectedRoom = preselectedRoom;

        BuildLayout();
        PopulateRooms();
        UpdateCertificateSection();
        UpdateCapacityStatus();
    }

    private void BuildLayout()
    {
        Text = _preselectedRoom != null
            ? $"Reserve Room {_preselectedRoom.Number}"
            : "New Reservation";
        Size = new Size(780, 880);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = AppColors.Primary
        };
        header.Controls.Add(new Label
        {
            Text = _preselectedRoom != null
                ? $"RESERVE ROOM {_preselectedRoom.Number}"
                : "NEW RESERVATION",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 15)
        });

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(36, 16, 36, 16),
            BackColor = AppColors.Surface
        };

        int y = 0;
        const int colW = 320;
        const int rightX = 340;

        // --- Guest ---
        body.Controls.Add(SectionHeader("GUEST", 0, y)); y += 28;

        // Passport is the primary identifier (mid-term UI feedback).
        body.Controls.Add(FieldLabel("Passport #", 0, y));
        _txtPassport = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 20), Size = new Size(170, 28) };
        _btnLookup = PrimaryButton("Lookup", 180, y + 20, 80);
        _btnLookup.Click += BtnLookup_Click;
        body.Controls.Add(_txtPassport);
        body.Controls.Add(_btnLookup);

        _lblGuestStatus = new Label
        {
            Text = "Enter a passport number to look up an existing guest.",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray500,
            AutoSize = true,
            Location = new Point(0, y + 54)
        };
        body.Controls.Add(_lblGuestStatus);
        y += 80;

        body.Controls.Add(FieldLabel("Guest Name", 0, y));
        _txtGuestName = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 20), Size = new Size(colW, 28) };
        body.Controls.Add(_txtGuestName);

        body.Controls.Add(FieldLabel("Gender", rightX, y));
        _cmbGuestGender = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(rightX, y + 20),
            Size = new Size(colW, 28)
        };
        _cmbGuestGender.Items.AddRange(new object[] { Gender.Male, Gender.Female });
        _cmbGuestGender.SelectedIndex = 0;
        _cmbGuestGender.SelectedIndexChanged += (s, e) => UpdateCertificateSection();
        body.Controls.Add(_cmbGuestGender);
        y += 56;

        body.Controls.Add(FieldLabel("Phone (optional)", 0, y));
        _txtPhone = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 20), Size = new Size(colW, 28) };
        body.Controls.Add(_txtPhone);
        y += 56;

        // --- Room ---
        body.Controls.Add(SectionHeader("ROOM", 0, y)); y += 28;

        body.Controls.Add(FieldLabel("Room #", 0, y));
        _cmbRoomNumber = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(0, y + 20),
            Size = new Size(colW, 28),
            Enabled = _preselectedRoom == null
        };
        _cmbRoomNumber.SelectedIndexChanged += (s, e) => { UpdateRoomDetails(); UpdateCapacityStatus(); };
        body.Controls.Add(_cmbRoomNumber);

        body.Controls.Add(FieldLabel("Type", rightX, y));
        _txtRoomType = ReadOnlyBox(rightX, y + 20, colW);
        body.Controls.Add(_txtRoomType);
        y += 56;

        body.Controls.Add(FieldLabel("Rate (per night)", 0, y));
        _txtRoomRate = ReadOnlyBox(0, y + 20, colW);
        body.Controls.Add(_txtRoomRate);

        body.Controls.Add(FieldLabel("Max capacity", rightX, y));
        _txtRoomCapacity = ReadOnlyBox(rightX, y + 20, colW);
        body.Controls.Add(_txtRoomCapacity);
        y += 56;

        // --- Dates ---
        body.Controls.Add(SectionHeader("DATES", 0, y)); y += 28;

        body.Controls.Add(FieldLabel("Check In", 0, y));
        _dtpCheckIn = new DateTimePicker { Font = new Font("Segoe UI", 10), Location = new Point(0, y + 20), Size = new Size(colW, 28), Format = DateTimePickerFormat.Short };
        body.Controls.Add(_dtpCheckIn);

        body.Controls.Add(FieldLabel("Check Out", rightX, y));
        _dtpCheckOut = new DateTimePicker { Font = new Font("Segoe UI", 10), Location = new Point(rightX, y + 20), Size = new Size(colW, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(1) };
        body.Controls.Add(_dtpCheckOut);
        y += 56;

        // --- Party ---
        body.Controls.Add(SectionHeader("ACCOMPANYING GUESTS", 0, y)); y += 28;

        var lblPartyHint = new Label
        {
            Text = "Children (under 18) count as half an adult for capacity.",
            Font = new Font("Segoe UI", 8, FontStyle.Italic),
            ForeColor = AppColors.Gray500,
            AutoSize = true,
            Location = new Point(0, y)
        };
        body.Controls.Add(lblPartyHint);
        y += 18;

        _dgvAccompanying = new DataGridView
        {
            Location = new Point(0, y),
            Size = new Size(680, 160),
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 9),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2
        };
        _dgvAccompanying.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
        _dgvAccompanying.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _dgvAccompanying.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvAccompanying.EnableHeadersVisualStyles = false;

        _dgvAccompanying.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Name",
            DataPropertyName = nameof(AccompanyingGuest.Name),
            FillWeight = 26
        });
        _dgvAccompanying.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Age",
            DataPropertyName = nameof(AccompanyingGuest.Age),
            FillWeight = 14
        });
        var genderCol = new DataGridViewComboBoxColumn
        {
            HeaderText = "Gender",
            DataPropertyName = nameof(AccompanyingGuest.Gender),
            FillWeight = 22,
            FlatStyle = FlatStyle.Flat
        };
        genderCol.Items.AddRange(Gender.Male, Gender.Female);
        _dgvAccompanying.Columns.Add(genderCol);
        _dgvAccompanying.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Passport (optional)",
            DataPropertyName = nameof(AccompanyingGuest.Passport),
            FillWeight = 38
        });

        _dgvAccompanying.DataSource = _accompanying;
        _accompanying.ListChanged += (s, e) => { UpdateCertificateSection(); UpdateCapacityStatus(); };
        _dgvAccompanying.CellValueChanged += (s, e) => { UpdateCertificateSection(); UpdateCapacityStatus(); };
        _dgvAccompanying.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_dgvAccompanying.IsCurrentCellDirty)
                _dgvAccompanying.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _dgvAccompanying.DataError += (s, e) => { e.ThrowException = false; };

        body.Controls.Add(_dgvAccompanying);
        y += 170;

        _btnAddPerson = new Button
        {
            Text = "+ Add Person",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Tertiary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 30),
            Location = new Point(0, y),
            Cursor = Cursors.Hand
        };
        _btnAddPerson.FlatAppearance.BorderSize = 0;
        _btnAddPerson.Click += (s, e) =>
        {
            _accompanying.Add(new AccompanyingGuest { Age = 18, Gender = Gender.Male });
        };

        _btnRemovePerson = new Button
        {
            Text = "Remove Selected",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.StatusOOS,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(150, 30),
            Location = new Point(140, y),
            Cursor = Cursors.Hand
        };
        _btnRemovePerson.FlatAppearance.BorderSize = 0;
        _btnRemovePerson.Click += (s, e) =>
        {
            if (_dgvAccompanying.CurrentRow == null) return;
            var idx = _dgvAccompanying.CurrentRow.Index;
            if (idx >= 0 && idx < _accompanying.Count)
                _accompanying.RemoveAt(idx);
        };

        body.Controls.Add(_btnAddPerson);
        body.Controls.Add(_btnRemovePerson);

        _lblCapacityStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(300, y + 8)
        };
        body.Controls.Add(_lblCapacityStatus);
        y += 44;


        _pnlCertificate = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(680, 90),
            BackColor = Color.FromArgb(255, 248, 230),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12),
            Visible = false
        };
        var lblCertTitle = new Label
        {
            Text = "Marriage certificate required (mixed-gender adult couple)",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(12, 8)
        };
        var lblCertHint = new Label
        {
            Text = "Certificate ID:",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray600,
            AutoSize = true,
            Location = new Point(12, 40)
        };
        _txtCertificateId = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(110, 36),
            Size = new Size(280, 28),
            PlaceholderText = "e.g. MC-2024-00123"
        };
        _pnlCertificate.Controls.AddRange(new Control[] { lblCertTitle, lblCertHint, _txtCertificateId });
        body.Controls.Add(_pnlCertificate);
        y += 100;

        // --- Footer Buttons ---
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = AppColors.Surface,
            Padding = new Padding(24, 12, 24, 12)
        };
        _btnSave = new Button
        {
            Text = "Create Reservation",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = AppColors.Accent,
            ForeColor = AppColors.Primary,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(180, 36),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            BackColor = AppColors.Gray200,
            ForeColor = AppColors.Gray800,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 36),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        footer.Controls.Add(_btnSave);
        footer.Controls.Add(_btnCancel);
        footer.Resize += (s, e) =>
        {
            _btnSave.Location = new Point(footer.Width - _btnSave.Width - 24, 12);
            _btnCancel.Location = new Point(footer.Width - _btnSave.Width - _btnCancel.Width - 32, 12);
        };

        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private static Label SectionHeader(string text, int x, int y) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        ForeColor = AppColors.Primary,
        AutoSize = true,
        Location = new Point(x, y)
    };

    private static Label FieldLabel(string text, int x, int y) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 9),
        ForeColor = AppColors.Gray600,
        AutoSize = true,
        Location = new Point(x, y)
    };

    private static TextBox ReadOnlyBox(int x, int y, int w) => new()
    {
        Font = new Font("Segoe UI", 10),
        Location = new Point(x, y),
        Size = new Size(w, 28),
        ReadOnly = true,
        BackColor = AppColors.Gray100,
        TabStop = false
    };

    private static Button PrimaryButton(string text, int x, int y, int w)
    {
        var b = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(w, 28),
            Location = new Point(x, y),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private void PopulateRooms()
    {
        _cmbRoomNumber.Items.Clear();

        if (_preselectedRoom != null)
        {
            _cmbRoomNumber.Items.Add(_preselectedRoom);
            _cmbRoomNumber.DisplayMember = nameof(Room.Number);
            _cmbRoomNumber.SelectedIndex = 0;
            return;
        }

        foreach (var room in _roomService.GetAvailableRooms())
            _cmbRoomNumber.Items.Add(room);
        _cmbRoomNumber.DisplayMember = nameof(Room.Number);
        if (_cmbRoomNumber.Items.Count > 0) _cmbRoomNumber.SelectedIndex = 0;
        else UpdateRoomDetails();
    }

    private void UpdateRoomDetails()
    {
        if (_cmbRoomNumber.SelectedItem is Room room)
        {
            _txtRoomType.Text = room.Type.ToString();
            _txtRoomRate.Text = room.Rate.ToString("C");
            _txtRoomCapacity.Text = $"{room.Capacity} guest(s)";
        }
        else
        {
            _txtRoomType.Text = "";
            _txtRoomRate.Text = "";
            _txtRoomCapacity.Text = "";
        }
    }

    private int CurrentCapacityUnits()
    {
        var adults = 1 + _accompanying.Count(a => !a.IsChild);
        var children = _accompanying.Count(a => a.IsChild);
        return 2 * adults + children;
    }

    private void UpdateCapacityStatus()
    {
        var adults = 1 + _accompanying.Count(a => !a.IsChild);
        var children = _accompanying.Count(a => a.IsChild);
        var used = CurrentCapacityUnits();
        var partyText = $"Party: {adults} adult(s)" + (children > 0 ? $" + {children} child(ren)" : "");

        if (_cmbRoomNumber.SelectedItem is Room room)
        {
            var cap = 2 * room.Capacity;
            var fits = used <= cap;
            _lblCapacityStatus.Text = partyText + (fits ? "  ✓ fits" : "  ✗ exceeds room capacity");
            _lblCapacityStatus.ForeColor = fits ? AppColors.Tertiary : AppColors.StatusOOS;
        }
        else
        {
            _lblCapacityStatus.Text = partyText;
            _lblCapacityStatus.ForeColor = AppColors.Gray600;
        }
    }

    private void BtnLookup_Click(object? sender, EventArgs e)
    {
        var passport = _txtPassport.Text.Trim();
        if (string.IsNullOrEmpty(passport))
        {
            _lblGuestStatus.Text = "Enter a passport number.";
            _lblGuestStatus.ForeColor = AppColors.Gray500;
            return;
        }

        _lookedUpGuest = _store.Guests.FirstOrDefault(g =>
            g.Passport.Equals(passport, StringComparison.OrdinalIgnoreCase));

        if (_lookedUpGuest != null)
        {
            _lblGuestStatus.Text = $"Found returning guest: {_lookedUpGuest.Name}";
            _lblGuestStatus.ForeColor = AppColors.Tertiary;
            _txtGuestName.Text = _lookedUpGuest.Name;
            _txtPhone.Text = _lookedUpGuest.Contact;
            _cmbGuestGender.SelectedItem = _lookedUpGuest.Gender;
        }
        else
        {
            _lblGuestStatus.Text = "New guest — please fill in details below.";
            _lblGuestStatus.ForeColor = AppColors.StatusClean;
            _txtGuestName.Focus();
        }

        UpdateCertificateSection();
    }

    private bool RequiresMarriageCertificate()
    {
        if (_cmbGuestGender.SelectedItem is not Gender g) return false;
        var opposite = g == Gender.Male ? Gender.Female : Gender.Male;
        return _accompanying.Any(a => !a.IsChild && a.Gender == opposite);
    }

    private void UpdateCertificateSection()
    {
        _pnlCertificate.Visible = RequiresMarriageCertificate();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // Commit any pending grid edit
        _dgvAccompanying.EndEdit();

        var name = _txtGuestName.Text.Trim();
        var phone = _txtPhone.Text.Trim();
        var passport = _txtPassport.Text.Trim();

        if (string.IsNullOrEmpty(passport)) { Warn("Passport number is required."); return; }
        if (string.IsNullOrEmpty(name)) { Warn("Guest name is required."); return; }

        if (_cmbRoomNumber.SelectedItem is not Room room)
        {
            Warn("Please select a room."); return;
        }

        if (_dtpCheckOut.Value.Date <= _dtpCheckIn.Value.Date)
        {
            Warn("Check-out must be after check-in."); return;
        }

        // Validate accompanying rows
        foreach (var a in _accompanying)
        {
            if (string.IsNullOrWhiteSpace(a.Name))
            {
                Warn("Every accompanying person needs a name."); return;
            }
            if (a.Age <= 0 || a.Age > 120)
            {
                Warn($"Invalid age for {a.Name}. Enter a value between 1 and 120."); return;
            }
        }

        // Capacity check
        var used = CurrentCapacityUnits();
        var cap = 2 * room.Capacity;
        if (used > cap)
        {
            Warn($"Room {room.Number} ({room.Type}) has capacity {room.Capacity}. " +
                 $"Your party exceeds capacity (children count as half).");
            return;
        }

        var guestGender = (Gender)(_cmbGuestGender.SelectedItem ?? Gender.Male);

        string? certificateId = null;
        if (RequiresMarriageCertificate())
        {
            certificateId = _txtCertificateId.Text.Trim();
            if (string.IsNullOrEmpty(certificateId))
            {
                Warn("A marriage certificate ID is required for a mixed-gender couple."); return;
            }
        }

        var guest = _lookedUpGuest;
        if (guest == null)
        {
            guest = _store.Guests.FirstOrDefault(g =>
                g.Passport.Equals(passport, StringComparison.OrdinalIgnoreCase));
        }

        if (guest == null)
        {
            guest = new Guest
            {
                Name = name,
                Contact = phone,
                Passport = passport,
                Gender = guestGender
            };
            _store.Guests.Add(guest);
        }
        else
        {
            guest.Name = name;
            guest.Contact = phone;
            guest.Passport = passport;
            guest.Gender = guestGender;
        }

        try
        {
            _bookingService.CreateReservation(
                guest, room,
                _dtpCheckIn.Value.Date, _dtpCheckOut.Value.Date,
                _accompanying.ToList(),
                certificateId);
        }
        catch (ArgumentException ex) { Warn(ex.Message); return; }
        catch (InvalidOperationException ex) { Warn(ex.Message); return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void Warn(string msg) =>
        MessageBox.Show(this, msg, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
