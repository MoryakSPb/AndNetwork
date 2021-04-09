using System;

namespace AndNetwork.Server.Import.AndNet7.Objects
{
    public class Player
    {
        public string Name { get; set; }
        public string RealName { get; set; }
        public int Department { get; set; }
        public int Post { get; set; }
        public ulong SteamId { get; set; }
        public ulong DiscordId { get; set; }
        public uint? VkId { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
