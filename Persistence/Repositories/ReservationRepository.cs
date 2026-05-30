using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

// Owns `reservations` and the child `reservation_accompanying` table.
public sealed class ReservationRepository
{
    private readonly SqlDb _db;
    public ReservationRepository(SqlDb db) { _db = db; }

    private const string SelectReservations = @"
        SELECT reservation_id, guest_id, room_id,
               check_in_date, check_out_date, status, marriage_certificate_id
        FROM   dbo.reservations
        ORDER  BY check_in_date;";

    private const string SelectAccompanying = @"
        SELECT accompanying_id, reservation_id, name, gender, age, passport
        FROM   dbo.reservation_accompanying;";

    private const string DeleteAccompanying = @"DELETE FROM dbo.reservation_accompanying;";
    private const string DeleteReservations = @"DELETE FROM dbo.reservations;";

    private const string InsertReservation = @"
        INSERT INTO dbo.reservations
            (reservation_id, guest_id, room_id, check_in_date, check_out_date,
             status, marriage_certificate_id)
        VALUES
            (@id, @guest, @room, @in, @out, @status, @cert);";

    private const string InsertAccompanying = @"
        INSERT INTO dbo.reservation_accompanying
            (accompanying_id, reservation_id, name, gender, age, passport)
        VALUES
            (@id, @res, @name, @gender, @age, @passport);";

    public List<Reservation> GetAll(
        IReadOnlyDictionary<Guid, Guest> guestsById,
        IReadOnlyDictionary<Guid, Room>  roomsById)
    {
        using var c = _db.Open();

        var reservations = new List<Reservation>();
        var byId = new Dictionary<Guid, Reservation>();

        using (var cmd = new SqlCommand(SelectReservations, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var guestId = r.GetGuid(1);
                var roomId  = r.GetGuid(2);

                if (!guestsById.TryGetValue(guestId, out var guest))
                    throw new InvalidDataException($"Reservation references unknown guest {guestId}.");
                if (!roomsById.TryGetValue(roomId, out var room))
                    throw new InvalidDataException($"Reservation references unknown room {roomId}.");

                var res = new Reservation
                {
                    Id                    = r.GetGuid(0),
                    Guest                 = guest,
                    Room                  = room,
                    CheckInDate           = r.GetDateTime(3),
                    CheckOutDate          = r.GetDateTime(4),
                    Status                = Enum.Parse<ReservationStatus>(r.GetString(5)),
                    MarriageCertificateId = r.IsDBNull(6) ? null : r.GetString(6),
                    Accompanying          = new List<AccompanyingGuest>()
                };
                reservations.Add(res);
                byId[res.Id] = res;
            }
        }

        using (var cmd = new SqlCommand(SelectAccompanying, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var resId = r.GetGuid(1);
                if (!byId.TryGetValue(resId, out var res)) continue;

                res.Accompanying.Add(new AccompanyingGuest
                {
                    Id       = r.GetGuid(0),
                    Name     = r.GetString(2),
                    Gender   = Enum.Parse<Gender>(r.GetString(3)),
                    Age      = r.GetInt32(4),
                    Passport = r.GetString(5)
                });
            }
        }

        return reservations;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        // Child rows first (CASCADE would also handle this, but being explicit
        // keeps the snapshot semantics symmetric with the insert side).
        using (var cmd = new SqlCommand(DeleteAccompanying, c, tx)) cmd.ExecuteNonQuery();
        using (var cmd = new SqlCommand(DeleteReservations, c, tx)) cmd.ExecuteNonQuery();
    }

    public void Insert(Reservation res, SqlConnection c, SqlTransaction tx)
    {
        using (var cmd = new SqlCommand(InsertReservation, c, tx))
        {
            cmd.Parameters.AddWithValue("@id",     res.Id);
            cmd.Parameters.AddWithValue("@guest",  res.Guest.Id);
            cmd.Parameters.AddWithValue("@room",   res.Room.Id);
            cmd.Parameters.AddWithValue("@in",     res.CheckInDate.Date);
            cmd.Parameters.AddWithValue("@out",    res.CheckOutDate.Date);
            cmd.Parameters.AddWithValue("@status", res.Status.ToString());
            cmd.Parameters.AddWithValue("@cert",
                string.IsNullOrEmpty(res.MarriageCertificateId)
                    ? DBNull.Value : (object)res.MarriageCertificateId);
            cmd.ExecuteNonQuery();
        }

        foreach (var person in res.Accompanying)
        {
            using var cmd = new SqlCommand(InsertAccompanying, c, tx);
            cmd.Parameters.AddWithValue("@id",       person.Id);
            cmd.Parameters.AddWithValue("@res",      res.Id);
            cmd.Parameters.AddWithValue("@name",     person.Name);
            cmd.Parameters.AddWithValue("@gender",   person.Gender.ToString());
            cmd.Parameters.AddWithValue("@age",      person.Age);
            cmd.Parameters.AddWithValue("@passport", person.Passport ?? "");
            cmd.ExecuteNonQuery();
        }
    }
}
