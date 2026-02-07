using Game.Api.Opponents;
using Game.Api.Services;
using Game.Shared.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.Configure<RandomOptions>(builder.Configuration.GetSection("Random"));
builder.Services.AddSingleton<IGameRandom, SeededGameRandom>();
builder.Services.AddSingleton<IRoundResolver, RoundResolver>();
builder.Services.AddSingleton<IBetValidator, BetValidator>();
builder.Services.AddSingleton<IPlayerStateService, PlayerStateService>();
builder.Services.AddSingleton<IOpponentService, OpponentService>();
builder.Services.AddSingleton<IOpponentStrategy, MartyStrategy>();
builder.Services.AddSingleton<IOpponentStrategy, WeightedStrategy>();
builder.Services.AddSingleton<IOpponentStrategy, PatternStrategy>();
builder.Services.AddSingleton<IOpponentStrategy, CheaterStrategy>();
builder.Services.AddSingleton<IGameService, GameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/opponents", (IOpponentService opponentService) =>
{
    var opponents = opponentService.GetProfiles();
    return Results.Ok(opponents);
});

app.MapGet("/opponents/{id}", (string id, IOpponentService opponentService) =>
{
    try
    {
        var strategy = opponentService.GetStrategy(id);
        return Results.Ok(strategy.Profile);
    }
    catch (BadHttpRequestException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/play", (PlayRequest request, IGameService gameService) =>
{
    var response = gameService.PlayRound(request);
    return Results.Ok(response);
});

app.MapGet("/stats", (IGameService gameService) =>
{
    var stats = gameService.GetStats();
    return Results.Ok(stats);
});

app.MapPost("/reset", (IGameService gameService) =>
{
    var state = gameService.Reset();
    return Results.Ok(state);
});

app.Run();
