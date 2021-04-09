using System;
using AndNetwork.Server.Discord.Enums;
using AndNetwork.Shared.Enums;
using Discord;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server
{
    public static class Extensions
    {
        public static LogLevel ToLogLevel(this LogSeverity severity) => severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
        };

        public static Color? GetDiscordColor(this ClanDepartmentEnum department) => department switch
        {
            ClanDepartmentEnum.Reserve => Color.DarkerGrey,
            ClanDepartmentEnum.BeginnersPool => Color.Gold,
            ClanDepartmentEnum.None => null,
            ClanDepartmentEnum.Infrastructure => Color.Orange,
            ClanDepartmentEnum.Research => Color.Green,
            ClanDepartmentEnum.Military => Color.Blue,
            ClanDepartmentEnum.Agitation => Color.Purple,
            _ => throw new ArgumentOutOfRangeException(nameof(department), department, null),
        };

        public static OverwritePermissions ToOverwritePermissions(this DiscordPermissionsFlags permissionsFlags) => new((ulong)permissionsFlags, ~(ulong)permissionsFlags);
    }
}
