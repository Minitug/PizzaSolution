using PizzaPlace.Models;
namespace PizzaPlace.Repositories;

public interface IOrderRepository
{
    Task AddOrder(OrderDto order);
    Task<OrderDto?> GetOrder(Guid orderId);
    Task UpdateOrderStatus(Guid orderId, string status);
    Task<int> FailUnfinishedOrders();
}
