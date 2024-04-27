using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Tomori.Utils;

namespace Tomori.Services;

internal class DiscordService : BackgroundService
{
    private DiscordSocketClient _client;
    private IConfiguration _config;
    private ILogger<DiscordService> _logger;
    private HttpClient _httpClient;

    private CardImageModule _cardModule;

    public DiscordService(IConfiguration configuration, ILogger<DiscordService> logger, HttpClient httpClient, CardImageModule cardImageModule)
    {
        _config = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _cardModule = cardImageModule;

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

    private async Task OnMessageReceived(SocketMessage message)
    {
        Console.WriteLine(message.Content);
        await ParseKarutaKluEmbed(message);
    }

    private async Task ParseKarutaKluEmbed(SocketMessage message)
    {
        if (message.Author.Id != Constants.KarutaID)
            return;

        var embeds = message.Embeds;

        var cardDropMatch = KarutaRegex.DropMessageRegex().Match(message.Content);
        if (cardDropMatch.Success)
        {
            var cardCountStr = cardDropMatch.Groups[1].Value;
            var cardCount = int.Parse(cardCountStr);
            await HandleDropMessage(message, cardCount);
            return;
        }


        if (embeds.Count != 1)
            return;

        var embed = embeds.First();
        if (embed.Description.Contains("Wishlisted"))
        {
            ParseKarutaKluEmbedSingle(embed.Description);
            return;
        }

        if (embed.Title == "Character Results")
        {
            ParseKarutaKluEmbedMultiple(embed.Fields.First().Value);
            return;
        }

    }

    private async Task HandleDropMessage(SocketMessage message, int cardCount)
    {
        if (message.Attachments.Count == 0)
        {
            _logger.LogError("No picture drop for {Message}", message.Content);
            return;
        }

        var attachment = message.Attachments.First();
        var picture = await _httpClient.GetByteArrayAsync(attachment.Url);

        var listChars = _cardModule.ReadTextFromCard(picture, cardCount);

        foreach(var character in listChars)
        {
            Console.WriteLine(character);
        }
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