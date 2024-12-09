using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath
        | HttpLoggingFields.Duration
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode;

    logging.MediaTypeOptions.AddText("application/json");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});
builder.Services.AddHttpLoggingInterceptor<LoggingApi.MyHttpLoggingInterceptor>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpLogging();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/products", (Product product) =>
{
    if (string.IsNullOrEmpty(product.Name) || product.Price <= 0)
    {
        return Results.BadRequest("Invalid product details.");
    }

    // Simulate saving the product (e.g., to a database)
    // Return 201 Created with the product
    return Results.Created($"/products/{product.Name}", product);
})
.WithName("CreateProduct")
.WithOpenApi();



app.MapGet("/querystring", (int EntityId) =>
{
    // Simulate fetching data using the EntityId
    var entityDetails = new
    {
        EntityId = EntityId,
        Name = $"Sample {EntityId}",
        Description = "This is a sample description."
    };

    return Results.Ok(entityDetails);
})
.WithName("GetEntity")
.WithOpenApi();


app.MapGet("/route/{entityId}", (int entityId) =>
{
    // Simulate fetching data using the EntityId
    var entityDetails = new
    {
        EntityId = entityId,
        Name = $"Sample {entityId}",
        Description = "This is a sample description."
    };

    return Results.Ok(entityDetails);
})
.WithName("GetEntityWithRoute")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record Product(string EntityId, string Name, decimal Price);
