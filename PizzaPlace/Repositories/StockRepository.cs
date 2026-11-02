
using Microsoft.Data.SqlClient;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using System.Data;

namespace PizzaPlace.Repositories;

public class StockRepository(IConfiguration configuration) : IStockRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("PizzaPlace")
        ?? throw new InvalidOperationException("Missing PizzaPlace connection string.");

    public async Task<StockDto> AddToStock(StockDto stock)
    {
        if (stock.Amount < 0)
            throw new PizzaException("Stock cannot have negative amount.");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // A transaction prevents concurrent requests from creating
        // duplicate stock entries or losing quantity updates.
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            const string sql = """
                DECLARE @Result TABLE (Id BIGINT, Amount INT);

                UPDATE Stock
                SET Amount = Amount + @Amount
                OUTPUT INSERTED.Id, INSERTED.Amount
                    INTO @Result
                WHERE StockType = @StockType;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO Stock (StockType, Amount)
                    OUTPUT INSERTED.Id, INSERTED.Amount
                        INTO @Result
                    VALUES (@StockType, @Amount);
                END

                SELECT Id, Amount FROM @Result;
                """;

            await using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.Add("@StockType", SqlDbType.Int).Value = (int)stock.StockType;
            command.Parameters.Add("@Amount", SqlDbType.Int).Value = stock.Amount;

            long id;
            int updatedAmount;

            await using (var reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("Failed to update stock.");

                id = reader.GetInt64(0);
                updatedAmount = reader.GetInt32(1);
            }

            await transaction.CommitAsync();

            return new StockDto(stock.StockType, updatedAmount, id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<StockDto> GetStock(StockType stockType)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT Id, Amount
            FROM Stock
            WHERE StockType = @StockType;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@StockType", SqlDbType.Int).Value = (int)stockType;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return new StockDto(stockType, 0);

        return new StockDto(
            stockType,
            reader.GetInt32(1),
            reader.GetInt64(0));
    }

    public async Task<StockDto> TakeStock(StockType stockType, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Unable to take zero or negative amount.");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE Stock
            SET Amount = Amount - @Amount
            WHERE StockType = @StockType
              AND Amount >= @Amount;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@StockType", SqlDbType.Int).Value = (int)stockType;
        command.Parameters.Add("@Amount", SqlDbType.Int).Value = amount;

        int affectedRows = await command.ExecuteNonQueryAsync();

        if (affectedRows == 0)
            throw new PizzaException("Not enough stock to take the given amount.");

        return new StockDto(stockType, amount);
    }
}
