using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    public Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        throw new NotImplementedException("Sufficient stock must be checked.");
    }

    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        ComparableList<StockDto> stock = new();
        bool ingredientIsAdded = false;

        foreach (PizzaAmount pizza in order.RequestedOrder) {
            PizzaRecipeDto? recipe = recipeDtos.FirstOrDefault(
                r => r.RecipeType == pizza.PizzaType);
            if (recipe != null) {
                foreach (StockDto ingredient in recipe.Ingredients)
                {
                    ingredientIsAdded = (stock.Any(item => item.StockType == ingredient.StockType));
                    if (!ingredientIsAdded)
                    {
                        StockDto availableStock = await stockRepository.GetStock(ingredient.StockType);
                        stock.Add(availableStock);
                    }
                }
            }
        }
        return stock;
    }
}
