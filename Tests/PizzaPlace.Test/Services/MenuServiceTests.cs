using PizzaPlace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaPlace.Test.Services
{
    [TestClass]
    public class MenuServiceTests
    {
        [TestMethod]
        [DataRow(10, "Standard Menu")]
        [DataRow(11, "Lunch Menu")]
        [DataRow(13, "Lunch Menu")]
        [DataRow(14, "Standard Menu")]
        public void GetMenu_AtSpecificHour_ReturnsExpectedMenu(
            int hour, string expectedTitle)
        {
            // Arrange
            var service = new MenuService();
            var time = new DateTimeOffset(
                2030, 10, 12, hour, 0, 0, TimeSpan.Zero);

            // Act
            var actual = service.GetMenu(time);

            // Assert
            Assert.AreEqual(expectedTitle, actual.Title);
        }
    }
}
