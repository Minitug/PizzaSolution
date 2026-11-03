
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class OrderBackgroundWorker(
    OrderQueue orderQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var queuedOrder in
                orderQueue.ReadAllAsync(stoppingToken))
            {
                await ProcessOrder(queuedOrder);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
    }

    private async Task ProcessOrder(QueuedPizzaOrder queuedOrder)
    {
        // Create a new scope for this order.
        // The HTTP request that submitted it has already finished.
        using var scope = scopeFactory.CreateScope();

        var orderingService =
            scope.ServiceProvider.GetRequiredService<IOrderingService>();

        var orderRepository =
            scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        try
        {
            await orderRepository.UpdateOrderStatus(
                queuedOrder.OrderId, "Preparing");

            var pizzas = await orderingService.HandlePizzaOrder(
                queuedOrder.Order);

            await orderRepository.UpdateOrderStatus(
                queuedOrder.OrderId, "Completed");

            logger.LogInformation(
                "Order {OrderId} completed. Prepared {Count} pizzas.",
                queuedOrder.OrderId,
                pizzas.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Order {OrderId} failed.",
                queuedOrder.OrderId);

            try
            {
                await orderRepository.UpdateOrderStatus(
                    queuedOrder.OrderId, "Failed");
            }
            catch (Exception updateException)
            {
                logger.LogError(
                    updateException,
                    "Unable to mark order {OrderId} as failed.",
                    queuedOrder.OrderId);
            }
        }
    }
}
