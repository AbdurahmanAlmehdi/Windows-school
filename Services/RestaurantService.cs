using HotelManagement.WinForms.Data;
using HotelManagement.WinForms.Models;
using HotelManagement.WinForms.Persistence;

namespace HotelManagement.WinForms.Services;

public class RestaurantService
{
    private readonly DataStore _store;
    private readonly AuthService _auth;
    private readonly IPersistenceContext _persistence;

    public RestaurantService(
        DataStore store,
        AuthService auth,
        IPersistenceContext? persistence = null)
    {
        _store = store;
        _auth = auth;
        _persistence = persistence ?? NullPersistenceContext.Instance;
    }

    public RestaurantOrder CreateOrder(Stay stay, IEnumerable<OrderLine> lines)
    {
        // FR-RST-4 / DEF-12: orders can only be created against an Active stay.
        if (stay.Status != StayStatus.Active)
            throw new InvalidOperationException(
                $"Cannot create an order against a {stay.Status} stay; only Active stays accept orders.");

        var linesList = lines.ToList();

        // DC-4 / DEF-14: every OrderLine.Quantity must be >= 1.
        foreach (var line in linesList)
            if (line.Quantity < 1)
                throw new ArgumentException(
                    "OrderLine.Quantity must be >= 1.", nameof(lines));

        var order = new RestaurantOrder
        {
            Stay = stay,
            Status = OrderStatus.Placed
        };
        foreach (var line in linesList)
            order.Lines.Add(line);

        _store.Orders.Add(order);
        _persistence.SaveOrder(order);
        return order;
    }

    public void AdvanceOrderStatus(RestaurantOrder order)
    {
        // FR-RST-6 / DEF-13: a cancelled order cannot be advanced.
        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException(
                "Cannot advance a cancelled order.");

        var next = order.Status switch
        {
            OrderStatus.Placed    => OrderStatus.Preparing,
            OrderStatus.Preparing => OrderStatus.Ready,
            OrderStatus.Ready     => OrderStatus.Served,
            _                     => order.Status // Served: silent no-op (legal terminal state)
        };

        if (next == order.Status) return;

        order.Status = next;
        _persistence.SaveOrder(order);
    }

    public void CancelOrder(RestaurantOrder order)
    {
        // FR-RST-7: cancellation is only legal from Placed or Preparing.
        // From Ready/Served/Cancelled we silently no-op (preserves existing API surface).
        if (order.Status is not (OrderStatus.Placed or OrderStatus.Preparing))
            return;

        order.Status = OrderStatus.Cancelled;
        _persistence.SaveOrder(order);

        // Cancelled orders must not contribute to the stay's restaurant charges.
        RecomputeStayCharges(order.Stay);
    }

    public IEnumerable<RestaurantOrder> GetOrdersForStay(Stay stay) =>
        _store.Orders.Where(o => o.Stay == stay);

    public void AddMenuItem(MenuItem item)
    {
        _auth.Require(PermissionResource.MenuItems, PermissionAction.Create);
        _store.MenuItems.Add(item);
        _persistence.SaveMenuItem(item);
    }

    public void RemoveMenuItem(MenuItem item)
    {
        _auth.Require(PermissionResource.MenuItems, PermissionAction.Delete);
        _store.MenuItems.Remove(item);
        _persistence.DeleteMenuItem(item);
    }

    public void UpdateMenuItem(MenuItem item, string name, decimal price, string category, bool isAvailable, string? imagePath = null)
    {
        _auth.Require(PermissionResource.MenuItems, PermissionAction.Update);
        item.Name = name;
        item.Price = price;
        item.Category = category;
        item.IsAvailable = isAvailable;
        if (imagePath != null) item.ImagePath = imagePath;
        _persistence.SaveMenuItem(item);
    }

    public void ToggleAvailability(MenuItem item)
    {
        _auth.Require(PermissionResource.MenuItems, PermissionAction.Update);
        item.IsAvailable = !item.IsAvailable;
        _persistence.SaveMenuItem(item);
    }

    public List<string> GetCategories() =>
        _store.MenuItems.Select(m => m.Category).Distinct().OrderBy(c => c).ToList();

    public void AddLinesToOrder(RestaurantOrder order, IEnumerable<OrderLine> newLines)
    {
        if (order.Status is not (OrderStatus.Placed or OrderStatus.Preparing)) return;

        var added = false;
        foreach (var line in newLines)
        {
            if (line.Quantity < 1)
                throw new ArgumentException(
                    "OrderLine.Quantity must be >= 1.", nameof(newLines));
            order.Lines.Add(line);
            added = true;
        }

        // Even if no lines were added we still recompute below in case the
        // caller wants to trigger a fresh aggregation. Persist the order
        // when we actually changed it.
        if (added) _persistence.SaveOrder(order);

        RecomputeStayCharges(order.Stay);
    }

    public decimal GetTodayServedRevenue() =>
        _store.Orders
            .Where(o => o.Status == OrderStatus.Served && o.CreatedAt.Date == DateTime.Today)
            .Sum(o => o.Total);

    private void RecomputeStayCharges(Stay stay)
    {
        stay.RestaurantCharges = _store.Orders
            .Where(o => o.Stay == stay && o.Status != OrderStatus.Cancelled)
            .Sum(o => o.Total);
        _persistence.SaveStay(stay);
    }
}
