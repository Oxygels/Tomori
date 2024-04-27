// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateApplicationBuilder(args);
host.Configuration.AddEnvironmentVariables(prefix: "Tomori_");
host.Services.AddHostedService<DiscordService>();

await host.Build().RunAsync();

