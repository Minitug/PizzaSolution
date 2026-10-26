using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

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

        PizzaRecipeType pizzaType = PizzaRecipeType.StandardPizza;

        ComparableList<StockDto> ingredients =
            [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2)
            ];

        PizzaRecipeDto recipe = new PizzaRecipeDto(pizzaType, ingredients, 5);

        ComparableList<PizzaRecipeDto> recipes =
            [
                recipe
            ];

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 20));

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 10));

        StockService stockService = new(stockRepositoryMock.Object);

        // Act
        ComparableList<StockDto> stock = await stockService.GetStock(pizzaOrder, recipes);

        // Assert
        Assert.AreEqual(20, stock[0].Amount);
        Assert.AreEqual(10, stock[1].Amount);
        Assert.AreEqual(2, stock.Count);
    }

    [TestMethod]
    public async Task GetStock_TwoPizzasShareIngredients()
    {
        // Arrange
        Mock<IStockRepository> stockRepositoryMock = new();

        ComparableList<PizzaAmount> order =
            [
             new PizzaAmount(PizzaRecipeType.StandardPizza, 2),
             new PizzaAmount(PizzaRecipeType.OddPizza, 1)
            ];

        PizzaOrder pizzaOrder = new(order);

        PizzaRecipeType pizzaTypeStandard = PizzaRecipeType.StandardPizza;
        PizzaRecipeType pizzaTypeOdd = PizzaRecipeType.OddPizza;

        ComparableList<StockDto> ingredientsStandard =
            [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2)
            ];

        PizzaRecipeDto recipeStandard = new PizzaRecipeDto(pizzaTypeStandard, ingredientsStandard, 5);

        ComparableList<StockDto> ingredientsOdd =
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Bacon, 5)
            ];

        PizzaRecipeDto recipeOdd = new PizzaRecipeDto(pizzaTypeOdd, ingredientsOdd, 5);

        ComparableList<PizzaRecipeDto> recipes =
            [
                recipeStandard,
                recipeOdd
            ];

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 20));

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 10));


        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Bacon))
        .ReturnsAsync(new StockDto(StockType.Bacon, 15));

        StockService stockService = new(stockRepositoryMock.Object);

        // Act
        ComparableList<StockDto> stock = await stockService.GetStock(pizzaOrder, recipes);

        // Assert
        Assert.AreEqual(3, stock.Count);
        Assert.AreEqual(20, stock[0].Amount);
        Assert.AreEqual(10, stock[1].Amount);
        Assert.AreEqual(15, stock[2].Amount);
        stockRepositoryMock.Verify(
            x => x.GetStock(StockType.Dough),
            Times.Once());
    }

    [TestMethod]
    public async Task HasInsufficientStock_EnoughStock_ReturnsFalse()
    {
        // Arrange
        Mock<IStockRepository> stockRepositoryMock = new();

        ComparableList<PizzaAmount> order =
            [
             new PizzaAmount(PizzaRecipeType.StandardPizza, 2),
             new PizzaAmount(PizzaRecipeType.OddPizza, 1)
            ];

        PizzaOrder pizzaOrder = new(order);

        PizzaRecipeType pizzaTypeStandard = PizzaRecipeType.StandardPizza;
        PizzaRecipeType pizzaTypeOdd = PizzaRecipeType.OddPizza;

        ComparableList<StockDto> ingredientsStandard =
            [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2)
            ];

        PizzaRecipeDto recipeStandard = new PizzaRecipeDto(pizzaTypeStandard, ingredientsStandard, 5);

        ComparableList<StockDto> ingredientsOdd =
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Bacon, 5)
            ];

        PizzaRecipeDto recipeOdd = new PizzaRecipeDto(pizzaTypeOdd, ingredientsOdd, 5);

        ComparableList<PizzaRecipeDto> recipes =
            [
                recipeStandard,
                recipeOdd
            ];

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 20));

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 10));


        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Bacon))
        .ReturnsAsync(new StockDto(StockType.Bacon, 15));

        StockService stockService = new(stockRepositoryMock.Object);

        // Act
        bool hasInsufficientStock = await stockService.HasInsufficientStock(pizzaOrder, recipes);

        // Assert
        Assert.IsFalse(hasInsufficientStock);
    }


    [TestMethod]
    public async Task HasInsufficientStock_RecipesNotFound()
    {
        // Arrange
        Mock<IStockRepository> stockRepositoryMock = new();

        ComparableList<PizzaAmount> order =
            [
             new PizzaAmount(PizzaRecipeType.StandardPizza, 2),
             new PizzaAmount(PizzaRecipeType.OddPizza, 1)
            ];

        PizzaOrder pizzaOrder = new(order);

        PizzaRecipeType pizzaTypeStandard = PizzaRecipeType.EmptyPizza;
        PizzaRecipeType pizzaTypeOdd = PizzaRecipeType.RarePizza;

        ComparableList<StockDto> ingredientsStandard =
            [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2)
            ];

        PizzaRecipeDto recipeStandard = new PizzaRecipeDto(pizzaTypeStandard, ingredientsStandard, 5);

        ComparableList<StockDto> ingredientsOdd =
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Bacon, 5)
            ];

        PizzaRecipeDto recipeOdd = new PizzaRecipeDto(pizzaTypeOdd, ingredientsOdd, 5);

        ComparableList<PizzaRecipeDto> recipes =
            [
                recipeStandard,
                recipeOdd
            ];

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 20));

        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 10));


        stockRepositoryMock
        .Setup(x => x.GetStock(StockType.Bacon))
        .ReturnsAsync(new StockDto(StockType.Bacon, 15));

        StockService stockService = new(stockRepositoryMock.Object);

        // Act
        bool hasInsufficientStock = await stockService.HasInsufficientStock(pizzaOrder, recipes);

        // Assert
        Assert.IsTrue(hasInsufficientStock);
    }
}
