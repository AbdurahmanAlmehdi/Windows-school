using System.Drawing.Drawing2D;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

public class CheckoutForm : Form
{
    private readonly Invoice _invoice;
    private readonly bool _isReadOnly;
    private ComboBox _cmbPayment = null!;

    public PaymentMethod SelectedPaymentMethod =>
        Enum.TryParse<PaymentMethod>(_cmbPayment.SelectedItem?.ToString(), out var pm) ? pm : PaymentMethod.Cash;

    public CheckoutForm(Invoice invoice, bool isReadOnly = false)
    {
        _invoice = invoice;
        _isReadOnly = isReadOnly;
        BuildLayout();
    }

    private void BuildLayout()
    {
        Text = "Guest Folio";
        Size = new Size(600, 750);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = AppColors.Surface;

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0)
        };

        // Navy header
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = AppColors.Primary
        };

        var lblTitle = new Label
        {
            Text = "GUEST FOLIO",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true,
            Location = new Point(20, 15)
        };

        var lblInvNum = new Label
        {
            Text = _invoice.InvoiceNumber,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = true
        };
        lblInvNum.Location = new Point(560 - lblInvNum.PreferredWidth - 20, 18);

        header.Controls.Add(lblTitle);
        header.Controls.Add(lblInvNum);

        // Content panel (below header, inside scroll)
        var content = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 16, 24, 16)
        };

        int y = 8;

        // Guest info card (painted)
        var guestCard = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(528, 80),
            BackColor = Color.Transparent
        };
        guestCard.Paint += DrawingUtilities.PaintCardBackground;

        var vipText = _invoice.Guest.IsVip ? "  [VIP]" : "";
        var lblGuestName = new Label
        {
            Text = $"{_invoice.Guest.Name}{vipText}",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(16, 14)
        };

        var checkIn = _invoice.Stay.CheckInDate;
        var checkOut = _invoice.Stay.ActualCheckOut ?? _invoice.Stay.ExpectedCheckOut;
        var lblRoomInfo = new Label
        {
            Text = $"Room {_invoice.Room.Number} {_invoice.Room.Type}   |   {checkIn:MMM dd} -> {checkOut:MMM dd, yyyy}",
            Font = new Font("Segoe UI", 10),
            ForeColor = AppColors.Gray600,
            AutoSize = true,
            Location = new Point(16, 44)
        };

        guestCard.Controls.Add(lblGuestName);
        guestCard.Controls.Add(lblRoomInfo);

        // PAID badge if read-only
        if (_isReadOnly && _invoice.PaymentStatus == PaymentStatus.Paid)
        {
            var lblPaid = new Label
            {
                Text = "PAID",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppColors.Tertiary,
                AutoSize = false,
                Size = new Size(60, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(450, 14)
            };
            guestCard.Controls.Add(lblPaid);
        }

        content.Controls.Add(guestCard);
        y += 90;

        // Section: ROOM CHARGES
        var roomLines = _invoice.Lines.Where(l => l.Category == InvoiceLineCategory.RoomCharge).ToList();
        if (roomLines.Count > 0)
        {
            y = AddSection(content, "ROOM CHARGES", roomLines, y);
        }

        // Section: RESTAURANT CHARGES
        var restLines = _invoice.Lines.Where(l => l.Category == InvoiceLineCategory.RestaurantCharge).ToList();
        if (restLines.Count > 0)
        {
            y = AddSection(content, "RESTAURANT CHARGES", restLines, y);
        }

        // Divider
        var divider = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(528, 2),
            BackColor = AppColors.Gray300
        };
        content.Controls.Add(divider);
        y += 12;

        // Totals
        y = AddTotalRow(content, "Subtotal", _invoice.Subtotal, y, false);
        y = AddTotalRow(content, "Tax (10%)", _invoice.Tax, y, false);

        // Bold separator
        var sepBold = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(528, 3),
            BackColor = AppColors.Primary
        };
        content.Controls.Add(sepBold);
        y += 10;

        // TOTAL
        var lblTotalLabel = new Label
        {
            Text = "TOTAL",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, y)
        };
        var lblTotalValue = new Label
        {
            Text = $"${_invoice.Total:F2}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = true
        };
        lblTotalValue.Location = new Point(528 - TextRenderer.MeasureText($"${_invoice.Total:F2}", lblTotalValue.Font).Width, y);
        content.Controls.Add(lblTotalLabel);
        content.Controls.Add(lblTotalValue);
        y += 50;

        // Payment method / buttons
        if (!_isReadOnly)
        {
            var lblPayMethod = new Label
            {
                Text = "Payment Method:",
                Font = new Font("Segoe UI", 11),
                ForeColor = AppColors.Gray700,
                AutoSize = true,
                Location = new Point(0, y + 4)
            };
            content.Controls.Add(lblPayMethod);

            _cmbPayment = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(150, y),
                Size = new Size(200, 30)
            };
            foreach (var pm in Enum.GetValues<PaymentMethod>())
                _cmbPayment.Items.Add(pm.ToString());
            _cmbPayment.SelectedIndex = 1; // CreditCard default
            content.Controls.Add(_cmbPayment);
            y += 50;

            var btnConfirm = new Button
            {
                Text = "Confirm Checkout && Pay",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = AppColors.Accent,
                ForeColor = AppColors.Primary,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(250, 44),
                Location = new Point(0, y),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            content.Controls.Add(btnConfirm);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 11),
                BackColor = AppColors.Gray200,
                ForeColor = AppColors.Gray700,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 44),
                Location = new Point(260, y),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            content.Controls.Add(btnCancel);

            AcceptButton = btnConfirm;
            CancelButton = btnCancel;
        }
        else
        {
            // Read-only: show payment info
            if (_invoice.PaymentMethod.HasValue)
            {
                var lblPaidInfo = new Label
                {
                    Text = $"Paid via {_invoice.PaymentMethod} on {_invoice.PaymentDate:MMM dd, yyyy}",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = AppColors.Tertiary,
                    AutoSize = true,
                    Location = new Point(0, y)
                };
                content.Controls.Add(lblPaidInfo);
                y += 35;
            }

            _cmbPayment = new ComboBox(); // dummy to avoid null
            _cmbPayment.Items.Add("Cash");
            _cmbPayment.SelectedIndex = 0;

            var btnClose = new Button
            {
                Text = "Close",
                Font = new Font("Segoe UI", 11),
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Location = new Point(0, y),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnClose.FlatAppearance.BorderSize = 0;
            content.Controls.Add(btnClose);

            CancelButton = btnClose;
        }

        mainPanel.Controls.Add(content);
        mainPanel.Controls.Add(header);
        Controls.Add(mainPanel);
    }

    private int AddSection(Panel parent, string sectionTitle, List<InvoiceLine> lines, int startY)
    {
        int y = startY;

        // Section header
        var lblSection = new Label
        {
            Text = sectionTitle,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true,
            Location = new Point(0, y)
        };
        parent.Controls.Add(lblSection);
        y += 26;

        decimal sectionSub = 0;
        foreach (var line in lines)
        {
            var desc = line.Quantity > 1
                ? $"{line.Description} x{line.Quantity}"
                : line.Description;

            var lblDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.Gray700,
                AutoSize = true,
                Location = new Point(16, y)
            };

            var amountText = $"${line.LineTotal:F2}";
            var lblAmount = new Label
            {
                Text = amountText,
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.Gray700,
                AutoSize = true
            };
            lblAmount.Location = new Point(528 - TextRenderer.MeasureText(amountText, lblAmount.Font).Width, y);

            parent.Controls.Add(lblDesc);
            parent.Controls.Add(lblAmount);
            sectionSub += line.LineTotal;
            y += 24;
        }

        // Section subtotal
        var subText = $"Sub: ${sectionSub:F2}";
        var lblSub = new Label
        {
            Text = subText,
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            ForeColor = AppColors.Gray500,
            AutoSize = true
        };
        lblSub.Location = new Point(528 - TextRenderer.MeasureText(subText, lblSub.Font).Width, y);
        parent.Controls.Add(lblSub);
        y += 30;

        // Separator
        var sep = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(528, 1),
            BackColor = AppColors.Gray200
        };
        parent.Controls.Add(sep);
        y += 10;

        return y;
    }

    private int AddTotalRow(Panel parent, string label, decimal amount, int y, bool bold)
    {
        var font = bold ? new Font("Segoe UI", 12, FontStyle.Bold) : new Font("Segoe UI", 11);
        var color = bold ? AppColors.Primary : AppColors.Gray700;

        var lblLabel = new Label
        {
            Text = label,
            Font = font,
            ForeColor = color,
            AutoSize = true,
            Location = new Point(300, y)
        };

        var amtText = $"${amount:F2}";
        var lblAmt = new Label
        {
            Text = amtText,
            Font = font,
            ForeColor = color,
            AutoSize = true
        };
        lblAmt.Location = new Point(528 - TextRenderer.MeasureText(amtText, lblAmt.Font).Width, y);

        parent.Controls.Add(lblLabel);
        parent.Controls.Add(lblAmt);
        return y + 28;
    }
}
