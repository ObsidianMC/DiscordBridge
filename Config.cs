using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBridge
{
    public class Config
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = "";

        [JsonPropertyName("channel_id")]
        public ulong ChannelId { get; set; } = 0;

        public static Config Load()
        {
            if (!File.Exists("discordbridge.json"))
            {
                File.WriteAllText("discordbridge.json", JsonSerializer.Serialize(new Config()));
            }
            var json = System.IO.File.ReadAllText("discordbridge.json");
            var cfg = JsonSerializer.Deserialize<Config>(json);
            File.WriteAllText("discordbridge.json", JsonSerializer.Serialize(cfg));
            return cfg;
        }
    }
}
