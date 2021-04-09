using System;

namespace AndNetwork.Server.Import.AndNet7.Objects
{
    public class Award
    {
        public int Idr { get; set; }
        public ulong PlayerSteamId { get; set; }
        public DateTime Time { get; set; }
        public string Description { get; set; }
    }
}
