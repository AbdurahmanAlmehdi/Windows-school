namespace HotelManagement.WinForms.Services.Security;

// BCrypt-based password hashing (NFR-SEC-1).
//
// Work factor 4 is intentionally low for the academic build per SRS CON-7
// so the test suite (which runs hundreds of Verify operations) stays fast.
// Production deployments should bump this to >= 12.
public static class PasswordHasher
{
    private const int WorkFactor = 4;

    public static string Hash(string plaintext) =>
        BCrypt.Net.BCrypt.HashPassword(plaintext ?? string.Empty, WorkFactor);

    public static bool Verify(string? plaintext, string? hash)
    {
        if (plaintext is null || hash is null) return false;
        if (!IsHash(hash)) return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(plaintext, hash);
        }
        catch
        {
            // Malformed hash, salt revision mismatch, etc.
            return false;
        }
    }

    // BCrypt hashes start with $2a$, $2b$, or $2y$. Any other value in the
    // password column is treated as legacy plaintext that needs migrating.
    public static bool IsHash(string value) =>
        value.Length >= 4
        && value[0] == '$'
        && value[1] == '2'
        && (value[2] == 'a' || value[2] == 'b' || value[2] == 'y')
        && value[3] == '$';
}
