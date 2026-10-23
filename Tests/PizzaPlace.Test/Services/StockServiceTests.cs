using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Services;

[TestClass]
public class StockServiceTests
{
    [TestMethod]
    public async Task GetStock_OnePizza_ReturnsAvailableIngredients()
    {
        // Arrange
        Mock<IStockRepository> stockRepositoryMock = new();

        ComparableList<PizzaAmount> order = 
            [
             new PizzaAmount(PizzaRecipeType.StandardPizza, 1)
            ];

        PizzaOrder pizzaOrder = new(order);



        // Act

        // Assert
    }
}
