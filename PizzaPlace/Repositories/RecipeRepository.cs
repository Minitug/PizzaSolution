using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PizzaPlace.Repositories;

public class RecipeRepository(IConfiguration configuration) : IRecipeRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("PizzaPlace")
        ?? throw new InvalidOperationException("Missing PizzaPlace connection string.");

    public async Task<long> AddRecipe(PizzaRecipeDto recipe)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string sql = """
                INSERT INTO Recipes (RecipeType, CookingTimeMinutes)
                OUTPUT INSERTED.Id
                VALUES (@RecipeType, @CookingTimeMinutes);
                """;

            await using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.Add("@RecipeType", SqlDbType.Int).Value = (int)recipe.RecipeType;
            command.Parameters.Add("@CookingTimeMinutes", SqlDbType.Int).Value = recipe.CookingTimeMinutes;

            long id = (long)(await command.ExecuteScalarAsync())!;

            await InsertIngredients(connection, transaction, id, recipe.Ingredients);
            await transaction.CommitAsync();

            return id;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            await transaction.RollbackAsync();
            throw new PizzaException($"Recipe already added for {recipe.RecipeType}.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PizzaRecipeDto> GetRecipe(PizzaRecipeType recipeType)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string recipeSql = """
            SELECT Id, RecipeType, CookingTimeMinutes
            FROM Recipes
            WHERE RecipeType = @RecipeType;
            """;

        long id;
        int cookingTime;

        await using (var command = new SqlCommand(recipeSql, connection))
        {
            command.Parameters.Add("@RecipeType", SqlDbType.Int).Value = (int)recipeType;

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new PizzaException($"Recipe does not exist of type {recipeType}.");

            id = reader.GetInt64(0);
            cookingTime = reader.GetInt32(2);
        }

        const string ingredientSql = """
            SELECT StockType, Amount
            FROM RecipeIngredients
            WHERE RecipeId = @RecipeId;
            """;

        var ingredients = new ComparableList<StockDto>();

        await using (var command = new SqlCommand(ingredientSql, connection))
        {
            command.Parameters.Add("@RecipeId", SqlDbType.BigInt).Value = id;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ingredients.Add(new StockDto(
                    (StockType)reader.GetInt32(0),
                    reader.GetInt32(1)));
            }
        }

        return new PizzaRecipeDto(recipeType, ingredients, cookingTime, id);
    }

    public async Task UpdateRecipe(PizzaRecipeDto recipe)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string updateSql = """
                UPDATE Recipes
                SET RecipeType = @RecipeType,
                    CookingTimeMinutes = @CookingTimeMinutes
                WHERE Id = @Id;
                """;

            await using (var command = new SqlCommand(updateSql, connection, transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.BigInt).Value = recipe.Id;
                command.Parameters.Add("@RecipeType", SqlDbType.Int).Value = (int)recipe.RecipeType;
                command.Parameters.Add("@CookingTimeMinutes", SqlDbType.Int).Value = recipe.CookingTimeMinutes;

                int affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                    throw new PizzaException($"Recipe with ID {recipe.Id} does not exist.");
            }

            const string deleteSql = """
                DELETE FROM RecipeIngredients
                WHERE RecipeId = @RecipeId;
                """;

            await using (var command = new SqlCommand(deleteSql, connection, transaction))
            {
                command.Parameters.Add("@RecipeId", SqlDbType.BigInt).Value = recipe.Id;
                await command.ExecuteNonQueryAsync();
            }

            await InsertIngredients(connection, transaction, recipe.Id, recipe.Ingredients);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task InsertIngredients(
        SqlConnection connection,
        SqlTransaction transaction,
        long recipeId,
        IEnumerable<StockDto> ingredients)
    {
        const string sql = """
            INSERT INTO RecipeIngredients (RecipeId, StockType, Amount)
            VALUES (@RecipeId, @StockType, @Amount);
            """;

        foreach (var ingredient in ingredients)
        {
            await using var command = new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@RecipeId", SqlDbType.BigInt).Value = recipeId;
            command.Parameters.Add("@StockType", SqlDbType.Int).Value = (int)ingredient.StockType;
            command.Parameters.Add("@Amount", SqlDbType.Int).Value = ingredient.Amount;

            await command.ExecuteNonQueryAsync();
        }
    }
}
