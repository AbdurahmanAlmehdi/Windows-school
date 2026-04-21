using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
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
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
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
            Location = new Point(20, 120),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblTagline = new Label
        {
            Text = "Management System",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize = false,
            Size = new Size(280, 30),
            Location = new Point(20, 265),
            TextAlign = ContentAlignment.MiddleCenter
        };

        panelLeft.Controls.Add(lblHotelName);
        panelLeft.Controls.Add(lblTagline);

        // Right login panel
        var lblTitle = new Label
        {
            Text = "Sign In",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            Location = new Point(380, 80),
            AutoSize = true
        };

        var lblSubtitle = new Label
        {
            Text = "Enter your credentials to continue",
            Font = new Font("Segoe UI", 10),
            ForeColor = AppColors.Gray500,
            Location = new Point(380, 120),
            AutoSize = true
        };

        var lblUser = new Label
        {
            Text = "Username",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Gray700,
            Location = new Point(380, 175),
            AutoSize = true
        };

        txtUsername = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Location = new Point(380, 200),
            Size = new Size(340, 30),
            PlaceholderText = "Enter username"
        };

        var lblPass = new Label
        {
            Text = "Password",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = AppColors.Gray700,
            Location = new Point(380, 250),
            AutoSize = true
        };

        txtPassword = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Location = new Point(380, 275),
            Size = new Size(340, 30),
            UseSystemPasswordChar = true,
            PlaceholderText = "Enter password"
        };
        txtPassword.KeyDown += TxtPassword_KeyDown;

        lblError = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(200, 50, 50),
            Location = new Point(380, 315),
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
            Location = new Point(380, 345),
            Cursor = Cursors.Hand
        };
        btnSignIn.FlatAppearance.BorderSize = 0;
        btnSignIn.Click += BtnSignIn_Click;

        var lblHint = new Label
        {
            Text = "Hint: admin / admin123  or  staff / staff123",
            Font = new Font("Segoe UI", 9),
            ForeColor = AppColors.Gray400,
            Location = new Point(380, 405),
            AutoSize = true
        };

        Controls.Add(panelLeft);
        Controls.Add(lblTitle);
        Controls.Add(lblSubtitle);
        Controls.Add(lblUser);
        Controls.Add(txtUsername);
        Controls.Add(lblPass);
        Controls.Add(txtPassword);
        Controls.Add(lblError);
        Controls.Add(btnSignIn);
        Controls.Add(lblHint);

        AcceptButton = btnSignIn;

        ResumeLayout(false);
        PerformLayout();
    }
}
