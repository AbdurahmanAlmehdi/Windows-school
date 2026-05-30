using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

public sealed class RoomRepository
{
    private readonly SqlDb _db;
    public RoomRepository(SqlDb db) { _db = db; }

    private const string SelectAll = @"
        SELECT room_id, number, floor, type, rate, is_occupied, [condition], maintenance_log
        FROM   dbo.rooms
        ORDER  BY number;";

    private const string DeleteAllSql = @"DELETE FROM dbo.rooms;";

    private const string InsertSql = @"
        INSERT INTO dbo.rooms
            (room_id, number, floor, type, rate, is_occupied, [condition], maintenance_log)
        VALUES
            (@id, @number, @floor, @type, @rate, @occupied, @condition, @log);";

    private const string UpsertSql = @"
        IF EXISTS (SELECT 1 FROM dbo.rooms WHERE room_id = @id)
            UPDATE dbo.rooms
               SET number = @number, floor = @floor, type = @type, rate = @rate,
                   is_occupied = @occupied, [condition] = @condition, maintenance_log = @log
             WHERE room_id = @id;
        ELSE
            INSERT INTO dbo.rooms
                (room_id, number, floor, type, rate, is_occupied, [condition], maintenance_log)
            VALUES
                (@id, @number, @floor, @type, @rate, @occupied, @condition, @log);";

    private const string DeleteSql = @"DELETE FROM dbo.rooms WHERE room_id = @id;";

    public List<Room> GetAll()
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand(SelectAll, c);
        using var r = cmd.ExecuteReader();

        var rooms = new List<Room>();
        while (r.Read())
        {
            rooms.Add(new Room
            {
                Id              = r.GetGuid(0),
                Number          = r.GetInt32(1),
                Floor           = r.GetInt32(2),
                Type            = Enum.Parse<RoomType>(r.GetString(3)),
                Rate            = r.GetDecimal(4),
                IsOccupied      = r.GetBoolean(5),
                Condition       = Enum.Parse<RoomCondition>(r.GetString(6)),
                MaintenanceLog  = r.IsDBNull(7) ? "" : r.GetString(7)
            });
        }
        return rooms;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteAllSql, c, tx);
        cmd.ExecuteNonQuery();
    }

    public void Insert(Room room, SqlConnection c, SqlTransaction tx) =>
        ExecuteWrite(InsertSql, room, c, tx);

    public void Upsert(Room room, SqlConnection c, SqlTransaction tx) =>
        ExecuteWrite(UpsertSql, room, c, tx);

    public void Delete(Room room, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteSql, c, tx);
        cmd.Parameters.AddWithValue("@id", room.Id);
        cmd.ExecuteNonQuery();
    }

    private static void ExecuteWrite(string sql, Room room, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(sql, c, tx);
        cmd.Parameters.AddWithValue("@id",        room.Id);
        cmd.Parameters.AddWithValue("@number",    room.Number);
        cmd.Parameters.AddWithValue("@floor",     room.Floor);
        cmd.Parameters.AddWithValue("@type",      room.Type.ToString());
        cmd.Parameters.AddWithValue("@rate",      room.Rate);
        cmd.Parameters.AddWithValue("@occupied",  room.IsOccupied);
        cmd.Parameters.AddWithValue("@condition", room.Condition.ToString());
        cmd.Parameters.AddWithValue("@log",
            string.IsNullOrEmpty(room.MaintenanceLog) ? DBNull.Value : (object)room.MaintenanceLog);
        cmd.ExecuteNonQuery();
    }
}
