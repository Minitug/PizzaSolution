using System.Threading.Channels;
using PizzaPlace.Models;

namespace PizzaPlace.Services;

public record QueuedPizzaOrder(Guid OrderId, PizzaOrder Order);

public class OrderQueue
{
    private readonly Channel<QueuedPizzaOrder> _channel =
        Channel.CreateUnbounded<QueuedPizzaOrder>();

    public void Enqueue(QueuedPizzaOrder order)
    {
        if (!_channel.Writer.TryWrite(order))
            throw new InvalidOperationException("Unable to queue pizza order.");
    }

    public IAsyncEnumerable<QueuedPizzaOrder> ReadAllAsync(
        CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}