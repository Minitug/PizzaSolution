using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Factories;

/// <summary>
/// Producing one line of pizza. 
/// Taking 7 minutes to setup - and then 5 minutes less for every subsequent pizza of the same recipe type to a minimum of 4 minutes.
/// </summary>
public class AssemblyLinePizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int AssemblyLineCapacity = 1;
    public const int SetupTimeMinutes = 7;
    public const int SubsequentPizzaTimeSavingsInMinutes = 5;
    public const int MinimumCookingTimeMinutes = 4;

    protected override int Capacity => AssemblyLineCapacity;

    protected override void PlanPizzaMaking(
    IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        PizzaRecipeType? previousType = null;
        int previousCookingTime = 0;

        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            int cookingTime;

            if (recipe.RecipeType != previousType)
            {
                cookingTime = recipe.CookingTimeMinutes + SetupTimeMinutes;
            }
            else
            {
                cookingTime = Math.Max(
                    MinimumCookingTimeMinutes,
                    previousCookingTime - SubsequentPizzaTimeSavingsInMinutes
                );
            }

            _pizzaQueue.Enqueue((
                async () =>
                {
                    await CookPizza(cookingTime);
                    return GetPizza(recipe.RecipeType);
                },
                orderGuid
            ));

            previousType = recipe.RecipeType;
            previousCookingTime = cookingTime;
        }
    }
}
