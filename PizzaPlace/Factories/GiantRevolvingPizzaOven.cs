using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Produces pizza on one giant revolving surface. Downside is, that all pizzas must have the same cooking time, when prepared at the same time.
/// </summary>
public class GiantRevolvingPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int GiantRevolvingPizzaOvenCapacity = 120;

    protected override int Capacity => GiantRevolvingPizzaOvenCapacity;

    protected override void PlanPizzaMaking(
    IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var batches = recipeOrders
            .GroupBy(x => x.Recipe.CookingTimeMinutes)
            .SelectMany(group => group.Chunk(Capacity));

        Task previousBatch = Task.CompletedTask;

        foreach (var batch in batches)
        {
            int cookingTime = batch[0].Recipe.CookingTimeMinutes;
            Task waitForPreviousBatch = previousBatch;

            var batchCompletion = new TaskCompletionSource();

            Task? cookingTask = null;

            foreach (var (recipe, orderGuid) in batch)
            {
                _pizzaQueue.Enqueue((
                    async () =>
                    {
                        cookingTask ??= CookBatch();

                        await cookingTask;
                        return GetPizza(recipe.RecipeType);
                    },
                    orderGuid
                ));
            }

            previousBatch = batchCompletion.Task;

            async Task CookBatch()
            {
                await waitForPreviousBatch;
                await CookPizza(cookingTime);
                batchCompletion.TrySetResult();
            }
        }
    }
}
