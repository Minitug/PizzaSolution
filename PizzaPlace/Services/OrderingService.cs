using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven,
    IOrderRepository orderRepository,
    OrderQueue orderQueue) : IOrderingService
{
    public async Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order)
    {
        var recipes = await recipeService.GetPizzaRecipes(order);
        if (await stockService.HasInsufficientStock(order, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        var stock = await stockService.GetStock(order, recipes);

        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.PizzaType)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        return await pizzaOven.PreparePizzas(prepareOrder, stock);

        PizzaRecipeDto GetPizzaRecipe(PizzaRecipeType pizzaType, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.RecipeType == pizzaType) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for {pizzaType} which was expected.");
    }

    public async Task<OrderDto> SubmitPizzaOrder(PizzaOrder order)
    {
        if (order.RequestedOrder.Count == 0 ||
            order.RequestedOrder.Any(x => x.Amount <= 0))
            throw new PizzaException("Order must contain positive pizza quantities.");

        var recipes = await recipeService.GetPizzaRecipes(order);

        if (await stockService.HasInsufficientStock(order, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        // Temporary pricing: 75 DKK per pizza.
        const decimal pricePerPizza = 75m;

        int pizzaCount = order.RequestedOrder.Sum(x => x.Amount);
        decimal totalCost = pizzaCount * pricePerPizza;

        // Conservative estimate: sequential cooking.
        int estimatedMinutes = order.RequestedOrder.Sum(request =>
        {
            var recipe = recipes.First(x => x.RecipeType == request.PizzaType);
            return request.Amount * recipe.CookingTimeMinutes;
        });

        var result = new OrderDto(
            Guid.NewGuid(),
            "Queued",
            totalCost,
            estimatedMinutes);

        await orderRepository.AddOrder(result);

        orderQueue.Enqueue(new QueuedPizzaOrder(result.OrderId, order));

        return result;
    }

    public Task<OrderDto?> GetOrderStatus(Guid orderId) =>
    orderRepository.GetOrder(orderId);
}
