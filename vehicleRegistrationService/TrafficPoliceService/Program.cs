var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

// Configure CORS to allow requests from other services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowServices", policy =>
    {
        policy.WithOrigins("http://localhost:5001", "http://localhost:5000", "http://localhost:5002",
                          "http://localhost:4200", "https://localhost:4200",
                          "http://vehicleservice:8080", "http://authservice:8080", "http://notificationservice:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowServices");

app.MapControllers();

app.Run();
