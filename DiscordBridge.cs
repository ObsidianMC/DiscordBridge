using DiscordBridge;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Channels;

namespace DiscordBridge;

public sealed class DiscordBridge : PluginBase
{
    // Dependencies will be injected automatically, if dependency class and field/property names match
    // Plugins won't load until all their required dependencies are added
    // Optional dependencies may be injected at any time, if at all
    [Inject]
    public ILogger<DiscordBridge> Logger { get; set; }

    // Gets injected after ConfigureServices is called
    private DiscordClient DiscordClient { get; set; }
    private Config Config { get; set; }
    private IServer Server { get; set; }
    private DiscordWebhook? Webhook { get; set; }

    //You can register services, commands and events here if you'd like
    public override void ConfigureServices(IServiceCollection services)
    {
        // never gets called?
    }

    public override void ConfigureRegistry(IPluginRegistry registry)
    {
        // Register message received event
        registry.MapEvent(async (IncomingChatMessageEventArgs chat) => await handleGameChat(chat));
    }

    private async ValueTask handleGameChat(IncomingChatMessageEventArgs args)
    {
        if (DiscordClient is null) return; // no client injected
        if (Webhook is null) return;

        await Webhook.ExecuteAsync(new DiscordWebhookBuilder()
            .WithAvatarUrl("https://mc-heads.net/head/" + args.Player.Username)
            .WithContent(args.Message)
            .WithUsername(args.Player.Username));
    }

    private async ValueTask handleDiscordMessage(MessageCreatedEventArgs args)
    {
        await Task.Yield();

        if (args.Channel.Id != Config.ChannelId) return;
        if (Server is null) return;
        if(args.Author.IsBot) return;

        Server.BroadcastMessage(ChatMessage.Simple("[Discord] ").AppendText($"{args.Message.Author.Username}: {args.Message.Content}"));

    }

    //Called when the plugin has fully loaded
    public override async ValueTask OnLoadedAsync(IServer server)
    {
        Logger.LogInformation("§a{pluginName} §floaded!", Info.Name);

        Config = Config.Load();
        //services.AddDiscordClient(Config.Token, DiscordIntents.MessageContents);
        //services.ConfigureEventHandlers(x =>
        //{
        //    x.HandleMessageCreated(async (client, eventArgs) => await handleDiscordMessage(eventArgs));
        //});
        DiscordClient = DiscordClientBuilder
            .CreateDefault(Config.Token, DiscordIntents.MessageContents | DiscordIntents.GuildMessages)
            .ConfigureEventHandlers(events =>
            {
                events.HandleMessageCreated(async (client, args) => await handleDiscordMessage(args));
            })
            .Build();

        this.Server = server;
        _ = Task.Run(async () => await this.DiscordClient.ConnectAsync(new DiscordActivity("Bridging Chat", DiscordActivityType.Custom)));

        var channel = await DiscordClient.GetChannelAsync(Config.ChannelId);
        var webhooks = await channel.GetWebhooksAsync();
        if (webhooks.Any(x => x.Name == "ObsidianBridge"))
        {
            Webhook = webhooks.First(x => x.Name == "ObsidianBridge");
        }
        else
        {
            Webhook = await channel.CreateWebhookAsync("ObsidianBridge");
        }
    }

    //This is self explanatory (called when the plugin is being unloaded)
    public override async ValueTask OnUnloadingAsync()
    {
        await DiscordClient.DisconnectAsync();
    }
}
