
using Microsoft.Data.SqlClient;
using PizzaPlace.Models;
using System.Data;

namespace PizzaPlace.Repositories;

public class OrderRepository(IConfiguration configuration) : IOrderRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("PizzaPlace")
        ?? throw new InvalidOperationException("Missing PizzaPlace connection string.");

    public async Task AddOrder(OrderDto order)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO Orders (Id, Status, TotalCost, EstimatedMinutes)
            VALUES (@Id, @Status, @TotalCost, @EstimatedMinutes);
            """;

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = order.OrderId;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = order.Status;

        var costParameter = command.Parameters.Add("@TotalCost", SqlDbType.Decimal);
        costParameter.Precision = 10;
        costParameter.Scale = 2;
        costParameter.Value = order.TotalCost;

        command.Parameters.Add("@EstimatedMinutes", SqlDbType.Int).Value = order.EstimatedMinutes;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<OrderDto?> GetOrder(Guid orderId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT Id, Status, TotalCost, EstimatedMinutes
            FROM Orders
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = orderId;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new OrderDto(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetDecimal(2),
            reader.GetInt32(3)
        );
    }

    public async Task UpdateOrderStatus(Guid orderId, string status)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE Orders
            SET Status = @Status,
                CompletedAt = CASE
                    WHEN @Status = 'Completed' THEN SYSUTCDATETIME()
                    ELSE CompletedAt
                END
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = orderId;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;

        int affectedRows = await command.ExecuteNonQueryAsync();

        if (affectedRows == 0)
            throw new InvalidOperationException($"Order {orderId} does not exist.");
    }

    public async Task<int> FailUnfinishedOrders()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
        UPDATE Orders
        SET Status = 'Failed'
        WHERE Status IN ('Queued', 'Preparing');
        """;

        await using var command = new SqlCommand(sql, connection);
        return await command.ExecuteNonQueryAsync();
    }
}
