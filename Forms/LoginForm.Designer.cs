using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnTogglePassword = null!;
    private Button btnSignIn = null!;
    private Label lblError = null!;

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

        // Form settings
        Text = "The Grand Hotel - Login";
        Size = new Size(800, 500);
        MinimumSize = new Size(650, 420);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor = AppColors.Surface;

        // Left decorative panel
        var panelLeft = new Panel
        {
            Dock = DockStyle.Left,
            Width = 320,
            BackColor = AppColors.Primary
        };

        var lblHotelName = new Label
        {
            Text = "THE GRAND\nHOTEL",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = AppColors.Accent,
            AutoSize = false,
            Size = new Size(280, 140),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblTagline = new Label
        {
            Text = "Management System",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = false,
            Size = new Size(280, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        panelLeft.Resize += (s, e) =>
        {
            lblHotelName.Location = new Point(
                (panelLeft.Width - lblHotelName.Width) / 2,
                (panelLeft.Height - 170) / 2);
            lblTagline.Location = new Point(
                (panelLeft.Width - lblTagline.Width) / 2,
                lblHotelName.Bottom + 5);
        };

        panelLeft.Controls.Add(lblHotelName);
        panelLeft.Controls.Add(lblTagline);

        // Right panel (fills remaining space)
        var panelRight = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Surface
        };

        // Fixed-size login panel, centered inside panelRight
        var panelLogin = new Panel
        {
            Size = new Size(340, 350),
            BackColor = AppColors.Surface
        };

        var lblTitle = new Label
        {
            Text = "Sign In",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(0, 0),
            AutoSize = true
        };

        var lblSubtitle = new Label
        {
            Text = "Enter your credentials to continue",
            Font = new Font("Segoe UI", 10),
            ForeColor = AppColors.Gray500,
            Location = new Point(0, 48),
            AutoSize = true
        };

        var lblUser = new Label
        {
            Text = "Username",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Gray700,
            Location = new Point(0, 95),
            AutoSize = true
        };

        txtUsername = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Location = new Point(0, 120),
            Size = new Size(340, 30),
            PlaceholderText = "Enter username"
        };

        var lblPass = new Label
        {
            Text = "Password",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Gray700,
            Location = new Point(0, 170),
            AutoSize = true
        };

        txtPassword = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Location = new Point(0, 195),
            Size = new Size(284, 30),
            UseSystemPasswordChar = true,
            PlaceholderText = "Enter password"
        };
        txtPassword.KeyDown += TxtPassword_KeyDown;

        btnTogglePassword = new Button
        {
            Text = "Show",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = AppColors.Gray200,
            ForeColor = AppColors.Gray800,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(56, 30),
            Location = new Point(284, 195),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btnTogglePassword.FlatAppearance.BorderSize = 0;
        btnTogglePassword.Click += BtnTogglePassword_Click;

        lblError = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(200, 50, 50),
            Location = new Point(0, 235),
            AutoSize = true,
            Visible = false
        };

        btnSignIn = new Button
        {
            Text = "Sign In",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = AppColors.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(340, 45),
            Location = new Point(0, 265),
            Cursor = Cursors.Hand
        };
        btnSignIn.FlatAppearance.BorderSize = 0;
        btnSignIn.Click += BtnSignIn_Click;

        var lblHint = new Label
        {
            Text = "Hint: superadmin / superadmin123  ·  staff / staff123",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray400,
            Location = new Point(0, 320),
            AutoSize = true
        };

        panelLogin.Controls.AddRange(new Control[] {
            lblTitle, lblSubtitle, lblUser, txtUsername,
            lblPass, txtPassword, btnTogglePassword, lblError, btnSignIn, lblHint
        });

        // Center the login panel inside the right panel on resize
        panelRight.Resize += (s, e) =>
        {
            panelLogin.Location = new Point(
                Math.Max(20, (panelRight.Width - panelLogin.Width) / 2),
                Math.Max(20, (panelRight.Height - panelLogin.Height) / 2));
        };

        panelRight.Controls.Add(panelLogin);

        Controls.Add(panelRight);
        Controls.Add(panelLeft);

        AcceptButton = btnSignIn;

        ResumeLayout(false);
        PerformLayout();
    }
}
