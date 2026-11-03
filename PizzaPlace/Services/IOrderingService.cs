using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public interface IOrderingService
{
    Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order);
    Task<OrderDto> SubmitPizzaOrder(PizzaOrder order);
    Task<OrderDto?> GetOrderStatus(Guid orderId);
}
