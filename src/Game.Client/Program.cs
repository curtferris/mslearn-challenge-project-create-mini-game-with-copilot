using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Game.Client;
using Game.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
	var configuration = sp.GetRequiredService<IConfiguration>();
	var baseUrl = configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
	return new HttpClient { BaseAddress = new Uri(baseUrl) };
});

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<SessionStateCache>();
builder.Services.AddScoped<GameApiClient>();

await builder.Build().RunAsync();
