using PizzaPlace;
using PizzaPlace.Factories;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddPolicy("allowAll",
        policy =>
        {
            policy.WithOrigins(
            // "whatEver.homepage.com"
            );
            policy.AllowAnyHeader();
            policy.AllowCredentials();
            policy.AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(d =>
{
    d.Title = "Pizza Place";
    d.Version = "v1";
});

// Register services:
var services = builder.Services;
services.AddSingleton(TimeProvider.System);

services.AddScoped<IStockRepository, StockRepository>();
services.AddScoped<IRecipeRepository, RecipeRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();

services.AddSingleton<OrderQueue>();

services.AddHostedService<OrderBackgroundWorker>();

services.AddTransient<IPizzaOven, NormalPizzaOven>();
services.AddTransient<IPizzaOven, AssemblyLinePizzaOven>();
services.AddTransient<IPizzaOven, GiantRevolvingPizzaOven>();

services.AddTransient<IStockService, StockService>();
services.AddTransient<IRecipeService, RecipeService>();
services.AddTransient<IOrderingService, OrderingService>();
services.AddTransient<IMenuService, MenuService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var orderRepository =
        scope.ServiceProvider.GetRequiredService<IOrderRepository>();

    await orderRepository.FailUnfinishedOrders();
}

app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors();

app.MapControllers();

app.Run();
