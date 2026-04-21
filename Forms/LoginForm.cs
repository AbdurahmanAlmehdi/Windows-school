using HotelManagement.WinForms.Services;
using HotelManagement.WinForms.Theme;

namespace HotelManagement.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly AuthService _authService;

    public LoginForm(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();
    }

    private void BtnSignIn_Click(object? sender, EventArgs e)
    {
        lblError.Visible = false;

        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            lblError.Text = "Please enter both username and password.";
            lblError.Visible = true;
            return;
        }

        if (_authService.Login(username, password))
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            lblError.Text = "Invalid username or password.";
            lblError.Visible = true;
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    private void TxtPassword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnSignIn_Click(sender, e);
        }
    }
}
