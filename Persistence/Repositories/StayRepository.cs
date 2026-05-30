using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

public sealed class StayRepository
{
    private readonly SqlDb _db;
    public StayRepository(SqlDb db) { _db = db; }

    private const string SelectAll = @"
        SELECT stay_id, guest_id, room_id, reservation_id,
               check_in_date, expected_check_out, actual_check_out,
               room_charges, restaurant_charges, status
        FROM   dbo.stays
        ORDER  BY check_in_date;";

    private const string DeleteAllSql = @"DELETE FROM dbo.stays;";

    private const string InsertSql = @"
        INSERT INTO dbo.stays
            (stay_id, guest_id, room_id, reservation_id,
             check_in_date, expected_check_out, actual_check_out,
             room_charges, restaurant_charges, status)
        VALUES
            (@id, @guest, @room, NULL,
             @in, @expected, @actual,
             @roomChg, @restChg, @status);";

    public List<Stay> GetAll(
        IReadOnlyDictionary<Guid, Guest> guestsById,
        IReadOnlyDictionary<Guid, Room>  roomsById)
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand(SelectAll, c);
        using var r = cmd.ExecuteReader();

        var stays = new List<Stay>();
        while (r.Read())
        {
            var guestId = r.GetGuid(1);
            var roomId  = r.GetGuid(2);

            if (!guestsById.TryGetValue(guestId, out var guest))
                throw new InvalidDataException($"Stay references unknown guest {guestId}.");
            if (!roomsById.TryGetValue(roomId, out var room))
                throw new InvalidDataException($"Stay references unknown room {roomId}.");

            stays.Add(new Stay
            {
                Id                 = r.GetGuid(0),
                Guest              = guest,
                Room               = room,
                CheckInDate        = r.GetDateTime(4),
                ExpectedCheckOut   = r.GetDateTime(5),
                ActualCheckOut     = r.IsDBNull(6) ? null : r.GetDateTime(6),
                RoomCharges        = r.GetDecimal(7),
                RestaurantCharges  = r.GetDecimal(8),
                Status             = Enum.Parse<StayStatus>(r.GetString(9))
            });
        }
        return stays;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteAllSql, c, tx);
        cmd.ExecuteNonQuery();
    }

    public void Insert(Stay stay, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(InsertSql, c, tx);
        cmd.Parameters.AddWithValue("@id",       stay.Id);
        cmd.Parameters.AddWithValue("@guest",    stay.Guest.Id);
        cmd.Parameters.AddWithValue("@room",     stay.Room.Id);
        cmd.Parameters.AddWithValue("@in",       stay.CheckInDate);
        cmd.Parameters.AddWithValue("@expected", stay.ExpectedCheckOut);
        cmd.Parameters.AddWithValue("@actual",
            stay.ActualCheckOut.HasValue ? (object)stay.ActualCheckOut.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@roomChg",  stay.RoomCharges);
        cmd.Parameters.AddWithValue("@restChg",  stay.RestaurantCharges);
        cmd.Parameters.AddWithValue("@status",   stay.Status.ToString());
        cmd.ExecuteNonQuery();
    }
}
