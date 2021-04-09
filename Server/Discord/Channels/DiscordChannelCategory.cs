using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AndNetwork.Server.Discord.Channels
{
    public class DiscordChannelCategory
    {
        public ulong DiscordId { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public virtual IList<DiscordChannelMetadata> Channels { get; set; }
    }
}
