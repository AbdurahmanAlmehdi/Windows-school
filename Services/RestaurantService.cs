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
}
