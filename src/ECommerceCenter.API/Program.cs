using ECommerceCenter.Application;
using ECommerceCenter.API.Middleware;
using ECommerceCenter.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = builder.Configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:5174";
        policy
            .WithOrigins(
                frontendUrl,
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // required for HttpOnly cookie to be sent
    });
});

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseHttpsRedirection();

app.UseStaticFiles(); // serves wwwroot/images/products/* for uploaded product images

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
