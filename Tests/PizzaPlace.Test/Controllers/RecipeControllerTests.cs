using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaPlace.Test.Controllers
{
    [TestClass]
    public class RecipeControllerTests
    {
        [TestMethod]
        public async Task AddRecipes_AddsAllRecipes()
        {
            //Arrange
            Mock<IRecipeRepository> recipeRepositoryMock = new();
            var recipe1 = new PizzaRecipeDto(
                PizzaRecipeType.StandardPizza,
                [
                    new StockDto(StockType.Dough, 5)
                    ],
                5);
            var recipe2 = new PizzaRecipeDto(
                PizzaRecipeType.OddPizza,
                [
                    new StockDto(StockType.Dough, 3)
                    ],
                2);


            recipeRepositoryMock
            .Setup(x => x.AddRecipe(recipe1))
            .ReturnsAsync(1L);

            recipeRepositoryMock
            .Setup(x => x.AddRecipe(recipe2))
            .ReturnsAsync(1L);

            ComparableList<PizzaRecipeDto> recipes = [recipe1, recipe2];

            RecipeController controller = new(recipeRepositoryMock.Object);

            //Act
            IActionResult result = await controller.AddRecipes(recipes);

            //Assert
            recipeRepositoryMock.VerifyAll();
            Assert.IsInstanceOfType<OkResult>(result);
        }

        [TestMethod]
        public async Task UpdateRecipe_UpdatesExistingRecipe()
        {
            //Arrange
            var recipe = new PizzaRecipeDto(
                PizzaRecipeType.StandardPizza,
                [
                    new StockDto(StockType.Dough, 3)
                ],
                8,
                Id: 1
            );

            Mock<IRecipeRepository> recipeRepositoryMock = new();

            recipeRepositoryMock
                .Setup(x => x.UpdateRecipe(recipe))
                .Returns(Task.CompletedTask);

            RecipeController controller = new(recipeRepositoryMock.Object);

            //Act
            IActionResult result = await controller.UpdateRecipe(recipe);

            //Assert
            recipeRepositoryMock.VerifyAll();
            Assert.IsInstanceOfType<OkResult>(result);
        }

        [TestMethod]
        public async Task UpdateRecipe_ChangesStoredRecipe()
        {
            //Arrange
            FakeRecipeRepository repository = new();

            var originalRecipe = new PizzaRecipeDto(
                PizzaRecipeType.OddPizza,
                [
                    new StockDto(StockType.Dough, 2)
                ],
                10
            );

            long id = await repository.AddRecipe(originalRecipe);

            //Act
            var updatedRecipe = originalRecipe with
            {
                Id = id,
                CookingTimeMinutes = 6
            };

            await repository.UpdateRecipe(updatedRecipe);

            //Assert
            PizzaRecipeDto storedRecipe =
                await repository.GetRecipe(PizzaRecipeType.OddPizza);

            Assert.AreEqual(6, storedRecipe.CookingTimeMinutes);
            Assert.AreEqual(id, storedRecipe.Id);

        }
    }
}
