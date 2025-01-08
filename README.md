# DiscordBridge
A simple Obsidian plugin that bridges in-game chat with Discord

## Download
DiscordBridge gets built using GitHub actions by default. At this moment, the plugin is unsigned. This will change later.

[Download the latest build here](https://github.com/ObsidianMC/DiscordBridge/releases)

## Config format
On first run, DiscordBridge will generate a new config file named `discordbridge.json`. It looks like this:
```json
{
    "token": "your Discord token",
	"channel_id" : 0
}
```
Make sure the bot whose token you use is in the server that the channel is in!
