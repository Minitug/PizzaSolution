using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Controllers;

[Route("api/recipes")]
public class RecipeController(IRecipeRepository recipeRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddRecipes([FromBody] ComparableList<PizzaRecipeDto> newRecipes)
    {
        foreach (PizzaRecipeDto recipe in newRecipes)
        {
            await recipeRepository.AddRecipe(recipe);
        }

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRecipe(
    [FromBody] PizzaRecipeDto recipe)
    {
        await recipeRepository.UpdateRecipe(recipe);

        return Ok();
    }
}