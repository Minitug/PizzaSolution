using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    public async Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        ComparableList<StockDto> stock = await GetStock(order, recipeDtos);
        ComparableList<StockDto> requiredIngredients = new();
        foreach (PizzaAmount pizza in order.RequestedOrder)
        {
            PizzaRecipeDto? recipe = recipeDtos.FirstOrDefault(
                r => r.RecipeType == pizza.PizzaType);
            if (recipe != null)
            {
                foreach (StockDto ingredient in recipe.Ingredients)
                {
                    int requiredAmount = ingredient.Amount * pizza.Amount;
                    bool ingredientIsAdded = (requiredIngredients.Any(item => item.StockType == ingredient.StockType));
                    if (!ingredientIsAdded)
                    {
                        requiredIngredients.Add(new StockDto(ingredient.StockType, requiredAmount));
                    }
                    else
                    {
                        StockDto? existingIngredient = requiredIngredients.FirstOrDefault(
                            item => item.StockType == ingredient.StockType);

                        if (existingIngredient != null)
                        {

                            StockDto updatedIngredient = existingIngredient with
                            {
                                Amount = existingIngredient.Amount + requiredAmount
                            };


                            for (int i = 0; i < requiredIngredients.Count; i++)
                            {
                                if (requiredIngredients[i].StockType == ingredient.StockType)
                                {
                                    requiredIngredients[i] = updatedIngredient;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (StockDto ingredient in requiredIngredients)
        {
            StockDto? stockIngredientCheck = stock.FirstOrDefault(
                item => item.StockType == ingredient.StockType);

            if (stockIngredientCheck == null)
            {
                return true;
            }

            if (ingredient.Amount > stockIngredientCheck.Amount)
            {
                return true;
            }
        }

        return false;
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
