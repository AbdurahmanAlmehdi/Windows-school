using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class AuthService
{
    private readonly DataStore _store;

    public AuthService(DataStore store)
    {
        _store = store;
    }

    public User? CurrentUser { get; private set; }

    public bool Login(string username, string password)
    {
        var user = _store.Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user != null)
        {
            CurrentUser = user;
            return true;
        }
        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
