using System;

namespace AndNetwork.Server.Discord.Enums
{
    [Flags]
    public enum DiscordPermissionsFlags : ulong
    {
        None = 0,

        View = 1024,
        Read = 1115136,
        Write = 38691524160UL,
        Priority = 107427909440UL,
        Moderator = 107440500672UL,

        All = ulong.MaxValue,
    }
}
