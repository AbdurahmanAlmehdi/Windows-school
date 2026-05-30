using System.ComponentModel;
using Microsoft.Data.SqlClient;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Persistence.Repositories;

// Owns `restaurant_orders` and the child `order_lines` table.
public sealed class RestaurantOrderRepository
{
    private readonly SqlDb _db;
    public RestaurantOrderRepository(SqlDb db) { _db = db; }

    private const string SelectOrders = @"
        SELECT order_id, stay_id, status, created_at
        FROM   dbo.restaurant_orders
        ORDER  BY created_at;";

    private const string SelectLines = @"
        SELECT line_id, order_id, menu_item_id, quantity, notes
        FROM   dbo.order_lines;";

    private const string DeleteLines  = @"DELETE FROM dbo.order_lines;";
    private const string DeleteOrders = @"DELETE FROM dbo.restaurant_orders;";

    private const string InsertOrder = @"
        INSERT INTO dbo.restaurant_orders (order_id, stay_id, status, created_at)
        VALUES (@id, @stay, @status, @created);";

    private const string InsertLine = @"
        INSERT INTO dbo.order_lines (line_id, order_id, menu_item_id, quantity, notes)
        VALUES (@id, @order, @item, @qty, @notes);";

    public List<RestaurantOrder> GetAll(
        IReadOnlyDictionary<Guid, Stay>     staysById,
        IReadOnlyDictionary<Guid, MenuItem> itemsById)
    {
        using var c = _db.Open();

        var orders = new List<RestaurantOrder>();
        var byId = new Dictionary<Guid, RestaurantOrder>();

        using (var cmd = new SqlCommand(SelectOrders, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var stayId = r.GetGuid(1);
                if (!staysById.TryGetValue(stayId, out var stay))
                    throw new InvalidDataException($"Order references unknown stay {stayId}.");

                var order = new RestaurantOrder
                {
                    Id        = r.GetGuid(0),
                    Stay      = stay,
                    Status    = Enum.Parse<OrderStatus>(r.GetString(2)),
                    CreatedAt = r.GetDateTime(3),
                    Lines     = new BindingList<OrderLine>()
                };
                orders.Add(order);
                byId[order.Id] = order;
            }
        }

        using (var cmd = new SqlCommand(SelectLines, c))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var orderId = r.GetGuid(1);
                var itemId  = r.GetGuid(2);

                if (!byId.TryGetValue(orderId, out var order)) continue;
                if (!itemsById.TryGetValue(itemId, out var item))
                    throw new InvalidDataException($"Order line references unknown menu item {itemId}.");

                order.Lines.Add(new OrderLine
                {
                    Id       = r.GetGuid(0),
                    MenuItem = item,
                    Quantity = r.GetInt32(3),
                    Notes    = r.IsDBNull(4) ? "" : r.GetString(4)
                });
            }
        }

        return orders;
    }

    public void DeleteAll(SqlConnection c, SqlTransaction tx)
    {
        using (var cmd = new SqlCommand(DeleteLines, c, tx))  cmd.ExecuteNonQuery();
        using (var cmd = new SqlCommand(DeleteOrders, c, tx)) cmd.ExecuteNonQuery();
    }

    public void Insert(RestaurantOrder order, SqlConnection c, SqlTransaction tx)
    {
        using (var cmd = new SqlCommand(InsertOrder, c, tx))
        {
            cmd.Parameters.AddWithValue("@id",      order.Id);
            cmd.Parameters.AddWithValue("@stay",    order.Stay.Id);
            cmd.Parameters.AddWithValue("@status",  order.Status.ToString());
            cmd.Parameters.AddWithValue("@created", order.CreatedAt);
            cmd.ExecuteNonQuery();
        }

        foreach (var line in order.Lines)
        {
            using var cmd = new SqlCommand(InsertLine, c, tx);
            cmd.Parameters.AddWithValue("@id",    line.Id);
            cmd.Parameters.AddWithValue("@order", order.Id);
            cmd.Parameters.AddWithValue("@item",  line.MenuItem.Id);
            cmd.Parameters.AddWithValue("@qty",   line.Quantity);
            cmd.Parameters.AddWithValue("@notes",
                string.IsNullOrEmpty(line.Notes) ? DBNull.Value : (object)line.Notes);
            cmd.ExecuteNonQuery();
        }
    }
}
