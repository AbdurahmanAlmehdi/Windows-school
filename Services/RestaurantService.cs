using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;

namespace HotelManagement.WinForms.Services;

public class RestaurantService
{
    private readonly DataStore _store;

    public RestaurantService(DataStore store)
    {
        _store = store;
    }

    public RestaurantOrder CreateOrder(Stay stay, IEnumerable<OrderLine> lines)
    {
        var order = new RestaurantOrder
        {
            Stay = stay,
            Status = OrderStatus.Placed
        };
        foreach (var line in lines)
            order.Lines.Add(line);

        _store.Orders.Add(order);
        return order;
    }

    public void AdvanceOrderStatus(RestaurantOrder order)
    {
        order.Status = order.Status switch
        {
            OrderStatus.Placed => OrderStatus.Preparing,
            OrderStatus.Preparing => OrderStatus.Ready,
            OrderStatus.Ready => OrderStatus.Served,
            _ => order.Status
        };
    }

    public void CancelOrder(RestaurantOrder order)
    {
        if (order.Status is OrderStatus.Placed or OrderStatus.Preparing)
        {
            order.Status = OrderStatus.Cancelled;
        }
    }

    public IEnumerable<RestaurantOrder> GetOrdersForStay(Stay stay) =>
        _store.Orders.Where(o => o.Stay == stay);

    public void AddMenuItem(MenuItem item) => _store.MenuItems.Add(item);

    public void RemoveMenuItem(MenuItem item) => _store.MenuItems.Remove(item);

    public void UpdateMenuItem(MenuItem item, string name, decimal price, string category, bool isAvailable)
    {
        item.Name = name;
        item.Price = price;
        item.Category = category;
        item.IsAvailable = isAvailable;
    }

    public void ToggleAvailability(MenuItem item) => item.IsAvailable = !item.IsAvailable;

    public List<string> GetCategories() =>
        _store.MenuItems.Select(m => m.Category).Distinct().OrderBy(c => c).ToList();

    public void AddLinesToOrder(RestaurantOrder order, IEnumerable<OrderLine> newLines)
    {
        if (order.Status is not (OrderStatus.Placed or OrderStatus.Preparing)) return;

        foreach (var line in newLines)
            order.Lines.Add(line);

        order.Stay.RestaurantCharges = _store.Orders
            .Where(o => o.Stay == order.Stay && o.Status != OrderStatus.Cancelled)
            .Sum(o => o.Total);
    }

    public decimal GetTodayServedRevenue() =>
        _store.Orders
            .Where(o => o.Status == OrderStatus.Served && o.CreatedAt.Date == DateTime.Today)
            .Sum(o => o.Total);
}
