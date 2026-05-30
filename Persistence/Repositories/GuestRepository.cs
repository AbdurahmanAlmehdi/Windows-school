using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

public sealed class GuestRepository
{
    private readonly SqlDb _db;
    public GuestRepository(SqlDb db) { _db = db; }

    private const string SelectAll = @"
        SELECT guest_id, name, contact, passport, gender, is_vip, stay_count
        FROM   dbo.guests
        ORDER  BY name;";

    private const string DeleteAllSql = @"DELETE FROM dbo.guests;";

    private const string InsertSql = @"
        INSERT INTO dbo.guests
            (guest_id, name, contact, passport, gender, is_vip, stay_count)
        VALUES
            (@id, @name, @contact, @passport, @gender, @vip, @stays);";

    public List<Guest> GetAll()
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand(SelectAll, c);
        using var r = cmd.ExecuteReader();

        var guests = new List<Guest>();
        while (r.Read())
        {
            guests.Add(new Guest
            {
                Id        = r.GetGuid(0),
                Name      = r.GetString(1),
                Contact   = r.IsDBNull(2) ? "" : r.GetString(2),
                Passport  = r.GetString(3),
                Gender    = Enum.Parse<Gender>(r.GetString(4)),
                IsVip     = r.GetBoolean(5),
                StayCount = r.GetInt32(6)
            });
        }
        return guests;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteAllSql, c, tx);
        cmd.ExecuteNonQuery();
    }

    public void Insert(Guest guest, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(InsertSql, c, tx);
        cmd.Parameters.AddWithValue("@id",       guest.Id);
        cmd.Parameters.AddWithValue("@name",     guest.Name);
        cmd.Parameters.AddWithValue("@contact",
            string.IsNullOrEmpty(guest.Contact) ? DBNull.Value : (object)guest.Contact);
        cmd.Parameters.AddWithValue("@passport", guest.Passport);
        cmd.Parameters.AddWithValue("@gender",   guest.Gender.ToString());
        cmd.Parameters.AddWithValue("@vip",      guest.IsVip);
        cmd.Parameters.AddWithValue("@stays",    guest.StayCount);
        cmd.ExecuteNonQuery();
    }
}
