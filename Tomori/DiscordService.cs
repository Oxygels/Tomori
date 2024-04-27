// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class DiscordService(IConfiguration Config, ILogger<DiscordService> Logger) : BackgroundService
{
    DiscordSocketClient client;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConfigureDiscordClient();
    }

    private async Task ConfigureDiscordClient()
    {
        var config = new DiscordSocketConfig();
        config.GatewayIntents = Discord.GatewayIntents.AllUnprivileged | Discord.GatewayIntents.MessageContent;

        client = new DiscordSocketClient(config);
        client.MessageReceived += OnMessageReceived;
        client.Log += OnLogReceived;

        var token = Config.GetValue<string>("token");
        await client.LoginAsync(Discord.TokenType.Bot, token, validateToken: true);
        await client.StartAsync();

        Logger.LogInformation("Discord bot started");

    }

    private Task OnLogReceived(LogMessage message)
    {
        switch(message.Severity)
        {
            case LogSeverity.Error:
                Logger.LogError("Discord error: {Message}",message.Message); 
                break;

            case LogSeverity.Warning:
                Logger.LogWarning("Discord warning: {Message}", message.Message);
                break;

            case LogSeverity.Critical:
                Logger.LogCritical("Discord critical: {Message}",message.Message);
                break;
        }
        return Task.CompletedTask;
    }

    private Task OnMessageReceived(SocketMessage message)
    {
        Console.WriteLine($"Message {message.Content}");
        return Task.CompletedTask;
    }
}