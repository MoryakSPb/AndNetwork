using System;

namespace AndNetwork.Server.Discord.Enums
{
    [Flags]
    public enum DiscordPermissionsFlags : ulong
    {
        None = 0,

        View = 2098176UL,
        Read = 3212288UL,
        Write = 313569431104UL,
        Priority = 382305816384UL,
        Moderator = 399498276800UL,

        All = ulong.MaxValue,
    }
}
