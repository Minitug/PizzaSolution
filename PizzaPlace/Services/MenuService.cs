using PizzaPlace.Models.Types;

namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    public Menu GetMenu(DateTimeOffset menuDate)
    {
        TimeSpan time = menuDate.ToUniversalTime().TimeOfDay;

        if (time >= TimeSpan.FromHours(11) &&
            time < TimeSpan.FromHours(14))
        {
            return CreateLunchMenu();
        }

        return CreateStandardMenu();
    }

    private Menu CreateStandardMenu()
    {

        return new Menu("Standard Menu",
            [new MenuItem("Margherita", PizzaRecipeType.StandardPizza, 75.00),
            new MenuItem("Pepperoni", PizzaRecipeType.StandardPizza, 85.00),
            new MenuItem("Capricciosa", PizzaRecipeType.StandardPizza, 90.00),
            new MenuItem("Amerikano", PizzaRecipeType.OddPizza, 100.00),
            new MenuItem("Robertino", PizzaRecipeType.ExtremelyTastyPizza, 105.00),
            new MenuItem("Bolognese", PizzaRecipeType.RarePizza, 105.00),
            new MenuItem("Hawaii", PizzaRecipeType.OddPizza, 90.00),
            new MenuItem("Milan", PizzaRecipeType.StandardPizza, 95.00),
            new MenuItem("Venezia", PizzaRecipeType.StandardPizza, 85.00),
            new MenuItem("Barzan", PizzaRecipeType.RarePizza, 115.00),
            new MenuItem("Rimini", PizzaRecipeType.StandardPizza, 95.00),
            new MenuItem("Rodi", PizzaRecipeType.StandardPizza, 95.00),
            ]);
    }

    private Menu CreateLunchMenu()
    {
        const double lunchDiscount = 0.90;
        return new Menu("Lunch Menu",
            [new MenuItem("Margherita", PizzaRecipeType.StandardPizza, 75.00 * lunchDiscount),
            new MenuItem("Pepperoni", PizzaRecipeType.StandardPizza, 85.00 * lunchDiscount),
            new MenuItem("Capricciosa", PizzaRecipeType.StandardPizza, 90.00 * lunchDiscount),
            new MenuItem("Amerikano", PizzaRecipeType.OddPizza, 100.00 * lunchDiscount),
            new MenuItem("Robertino", PizzaRecipeType.ExtremelyTastyPizza, 105.00 * lunchDiscount),
            new MenuItem("Bolognese", PizzaRecipeType.RarePizza, 105.00 * lunchDiscount),
            new MenuItem("Hawaii", PizzaRecipeType.OddPizza, 90.00 * lunchDiscount),
            new MenuItem("Milan", PizzaRecipeType.StandardPizza, 95.00 * lunchDiscount),
            new MenuItem("Venezia", PizzaRecipeType.StandardPizza, 85.00 * lunchDiscount),
            new MenuItem("Barzan", PizzaRecipeType.RarePizza, 115.00 * lunchDiscount),
            new MenuItem("Rimini", PizzaRecipeType.StandardPizza, 95.00 * lunchDiscount),
            new MenuItem("Rodi", PizzaRecipeType.StandardPizza, 95.00 * lunchDiscount),
            ]);
    }
}
 