using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Tomori.Services;

var host = Host.CreateApplicationBuilder(args);
host.Configuration.AddEnvironmentVariables(prefix: "Tomori_");
host.Services.AddHostedService<DiscordService>();

await host.Build().RunAsync();

