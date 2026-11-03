namespace PizzaPlace.Models;

public record OrderDto(
    Guid OrderId,
    string Status,
    decimal TotalCost,
    int EstimatedMinutes
);