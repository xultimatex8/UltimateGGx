using DotNetEnv;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Services;
using System.Text.Json.Serialization;
using backend.Interfaces;
using backend.Middleware;
using backend.Http;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsProduction())
{
    Env.Load("../.env");
}

var riotApiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY");

builder.Services.AddHttpClient("RiotPlatform", client =>
{
    client.BaseAddress = new Uri("https://euw1.api.riotgames.com/");
    client.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);
}).AddHttpMessageHandler<RiotRateLimitHandler>();

builder.Services.AddHttpClient("RiotRegional", client =>
{
    client.BaseAddress = new Uri("https://europe.api.riotgames.com/");
    client.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);
}).AddHttpMessageHandler<RiotRateLimitHandler>();

builder.Services.AddHttpClient<IDataDragonService, DataDragonService>(client =>
{
    client.BaseAddress = new Uri("https://ddragon.leagueoflegends.com/");
});

builder.Services.AddTransient<RiotRateLimitHandler>();

builder.Services.AddScoped<IDataDragonSyncCheckerService, DataDragonSyncCheckerService>();
builder.Services.AddHostedService<DataDragonSyncBackgroundService>();

builder.Services.AddScoped<IRiotApiService, RiotApiService>();
builder.Services.AddScoped<ChampionSyncService>();
builder.Services.AddScoped<ISummonerService, SummonerService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ITimelineService, TimelineService>();

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost"};" +
    $"Port=5432;" +
    $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
    $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("Angular");

app.UseAuthorization();
app.MapControllers();
app.Run();