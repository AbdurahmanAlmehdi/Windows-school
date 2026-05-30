using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

public sealed class MenuItemRepository
{
    private readonly SqlDb _db;
    public MenuItemRepository(SqlDb db) { _db = db; }

    private const string SelectAll = @"
        SELECT menu_item_id, name, price, category, is_available, description, image_path
        FROM   dbo.menu_items
        ORDER  BY category, name;";

    private const string DeleteAllSql = @"DELETE FROM dbo.menu_items;";

    private const string InsertSql = @"
        INSERT INTO dbo.menu_items
            (menu_item_id, name, price, category, is_available, description, image_path)
        VALUES
            (@id, @name, @price, @category, @avail, @description, @image);";

    private const string UpsertSql = @"
        IF EXISTS (SELECT 1 FROM dbo.menu_items WHERE menu_item_id = @id)
            UPDATE dbo.menu_items
               SET name = @name, price = @price, category = @category,
                   is_available = @avail, description = @description, image_path = @image
             WHERE menu_item_id = @id;
        ELSE
            INSERT INTO dbo.menu_items
                (menu_item_id, name, price, category, is_available, description, image_path)
            VALUES
                (@id, @name, @price, @category, @avail, @description, @image);";

    private const string DeleteSql = @"DELETE FROM dbo.menu_items WHERE menu_item_id = @id;";

    public List<MenuItem> GetAll()
    {
        using var c = _db.Open();
        using var cmd = new SqlCommand(SelectAll, c);
        using var r = cmd.ExecuteReader();

        var items = new List<MenuItem>();
        while (r.Read())
        {
            items.Add(new MenuItem
            {
                Id          = r.GetGuid(0),
                Name        = r.GetString(1),
                Price       = r.GetDecimal(2),
                Category    = r.GetString(3),
                IsAvailable = r.GetBoolean(4),
                Description = r.IsDBNull(5) ? "" : r.GetString(5),
                ImagePath   = r.IsDBNull(6) ? null : r.GetString(6)
            });
        }
        return items;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteAllSql, c, tx);
        cmd.ExecuteNonQuery();
    }

    public void Insert(MenuItem item, SqlConnection c, SqlTransaction tx) =>
        ExecuteWrite(InsertSql, item, c, tx);

    public void Upsert(MenuItem item, SqlConnection c, SqlTransaction tx) =>
        ExecuteWrite(UpsertSql, item, c, tx);

    public void Delete(MenuItem item, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(DeleteSql, c, tx);
        cmd.Parameters.AddWithValue("@id", item.Id);
        cmd.ExecuteNonQuery();
    }

    private static void ExecuteWrite(string sql, MenuItem item, SqlConnection c, SqlTransaction tx)
    {
        using var cmd = new SqlCommand(sql, c, tx);
        cmd.Parameters.AddWithValue("@id",       item.Id);
        cmd.Parameters.AddWithValue("@name",     item.Name);
        cmd.Parameters.AddWithValue("@price",    item.Price);
        cmd.Parameters.AddWithValue("@category", item.Category);
        cmd.Parameters.AddWithValue("@avail",    item.IsAvailable);
        cmd.Parameters.AddWithValue("@description",
            string.IsNullOrEmpty(item.Description) ? DBNull.Value : (object)item.Description);
        cmd.Parameters.AddWithValue("@image",
            string.IsNullOrEmpty(item.ImagePath) ? DBNull.Value : (object)item.ImagePath);
        cmd.ExecuteNonQuery();
    }
}
