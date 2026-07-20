using DotNetEnv;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Services;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<RiotApiService>(client =>
{
    client.BaseAddress = new Uri("https://europe.api.riotgames.com/");
    client.DefaultRequestHeaders.Add("X-Riot-Token", Environment.GetEnvironmentVariable("RIOT_API_KEY"));
});

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost"};" +
    $"Port=5432;" +
    $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
    $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}";

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Angular");

app.UseAuthorization();
app.MapControllers();
app.Run();