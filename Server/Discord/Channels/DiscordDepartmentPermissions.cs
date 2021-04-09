using System.Text.Json.Serialization;
using AndNetwork.Server.Discord.Enums;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Server.Discord.Channels
{
    public class DiscordDepartmentPermissions
    {
        [JsonIgnore]
        public ulong ChannelId { get; set; }
        public ClanDepartmentEnum Department { get; set; }
        [JsonIgnore]
        public virtual DiscordChannelMetadata Metadata { get; set; }
        public DiscordPermissionsFlags Permissions { get; set; }
    }
}
