using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/order")]
public class OrderingController(
    IOrderingService orderingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlacePizzaOrder(
    [FromBody] PizzaOrder pizzaOrder)
    {
        var result = await orderingService.SubmitPizzaOrder(pizzaOrder);

        return AcceptedAtAction(
            nameof(GetOrderStatus),
            new { orderId = result.OrderId },
            result);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        var order = await orderingService.GetOrderStatus(orderId);

        return order is null ? NotFound() : Ok(order);
    }
}
