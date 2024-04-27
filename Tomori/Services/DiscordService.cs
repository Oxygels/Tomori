using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Tomori.Services;

internal class DiscordService : BackgroundService
{
    private DiscordSocketClient _client;
    private IConfiguration _config;
    private ILogger<DiscordService> _logger;

    public DiscordService(IConfiguration configuration, ILogger<DiscordService> logger)
    {
        _config = configuration;
        _logger = logger;


        ConfigureDiscordClient();
    }

    private void ConfigureDiscordClient()
    {
        var config = new DiscordSocketConfig();
        config.GatewayIntents = Discord.GatewayIntents.MessageContent | Discord.GatewayIntents.GuildMessages | Discord.GatewayIntents.Guilds;

        _client = new DiscordSocketClient(config);
        _client.MessageReceived += OnMessageReceived;
        _client.MessageUpdated += OnMessageUpdated;
        _client.Log += OnLogReceived;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = _config.GetValue<string>("token");
        await _client.LoginAsync(Discord.TokenType.Bot, token, validateToken: true);
        await _client.StartAsync();

        _logger.LogInformation("Discord bot started");
    }

    private Task OnMessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
    {
        OnMessageReceived(message);
        return Task.CompletedTask;
    }

    private Task OnLogReceived(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Error:
                _logger.LogError("Discord error: {Message}", message.Message);
                break;

            case LogSeverity.Warning:
                _logger.LogWarning("Discord warning: {Message}", message.Message);
                break;

            case LogSeverity.Critical:
                _logger.LogCritical("Discord critical: {Message}", message.Message);
                break;
        }
        return Task.CompletedTask;
    }

    private Task OnMessageReceived(SocketMessage message)
    {
        Console.WriteLine(message.Content);
        ParseKarutaKluEmbed(message);

        return Task.CompletedTask;
    }

    private void ParseKarutaKluEmbed(SocketMessage message)
    {
        if (message.Author.Id != Constants.KarutaID)
            return;

        var embeds = message.Embeds;

        if (embeds.Count != 1)
            return;

        var embed = embeds.First();
        if (embed.Description.Contains("Wishlisted"))
        {
            ParseKarutaKluEmbedSingle(embed.Description);
        }

        else if (embed.Title == "Character Results")
        {
            ParseKarutaKluEmbedMultiple(embed.Fields.First().Value);
        }

        return;

    }

    private void ParseKarutaKluEmbedMultiple(string description)
    {
        foreach (var i in description.Split('\n'))
        {
            var groups = KarutaRegex.MultiCharRegex().Match(i).Groups;

            var wl = groups[1].Value;
            var series = groups[2].Value;
            var character = groups[3].Value;
        }
    }

    private void ParseKarutaKluEmbedSingle(string description)
    {
        var groups = KarutaRegex.SingleCharRegex().Match(description).Groups;

        var wl = groups[3].Value;
        var series = groups[2].Value;
        var character = groups[1].Value;
    }
}