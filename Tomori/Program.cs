using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Tomori.Services;
using Tomori.Utils;

var host = Host.CreateApplicationBuilder(args);
host.Configuration.AddEnvironmentVariables(prefix: "Tomori_");

host.Services.AddSingleton<HttpClient>();
host.Services.AddSingleton<CardImageModule>();

host.Services.AddHostedService<DiscordService>();

await host.Build().RunAsync();

