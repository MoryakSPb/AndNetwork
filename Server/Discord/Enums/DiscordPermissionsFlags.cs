using System;

namespace AndNetwork.Server.Discord.Enums
{
    [Flags]
    public enum DiscordPermissionsFlags : ulong
    {
        None = 0,

        View = 2098176,
        Read = 3212288,
        Write = 176130477632UL,
        Priority = 244866862912UL,
        Moderator = 262059323328UL,

        All = ulong.MaxValue,
    }
}
