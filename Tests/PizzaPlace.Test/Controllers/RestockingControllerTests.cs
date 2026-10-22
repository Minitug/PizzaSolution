using Microsoft.AspNetCore.Mvc;
using Moq;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaPlace.Test.Controllers
{
    [TestClass]
    public class RestockingControllerTests
    {
        [TestMethod]
        public async Task RestockAddAllIngredients()
        {
            //Arrange
            ComparableList<StockDto> tmpStock =
                [
                  new StockDto(StockType.Dough, 5),
                  new StockDto(StockType.Tomatoes, 10),
                  new StockDto(StockType.GratedCheese, 10),
                  new StockDto(StockType.GenericSpices, 10),
                  new StockDto(StockType.FermentedDough, 5),
                  new StockDto(StockType.RottenTomatoes, 10),
                  new StockDto(StockType.UngenericSpices, 10)
                  ];

            Mock<IStockRepository> stockRepositoryMock = new();

            var restockingController = new RestockingController(stockRepositoryMock.Object);

            foreach (var item in tmpStock) {
                stockRepositoryMock.Setup(x => x.AddToStock(item))
               .ReturnsAsync(item);
            }

            //Act
            IActionResult result = await restockingController.Restock(tmpStock);

            //Assert
            stockRepositoryMock.VerifyAll();
            Assert.IsInstanceOfType<OkResult>(result);
        }
    }
}
