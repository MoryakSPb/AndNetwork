using System;

namespace AndNetwork.Server.Discord.Enums
{
    [Flags]
    public enum DiscordPermissionsFlags : ulong
    {
        None = 0,

        View = 1024,
        Read = 1115136,
        Write = 4331785792UL,
        Priority = 4348694336UL,
        Moderator = 4361285568UL,

        All = ulong.MaxValue,
    }
}
