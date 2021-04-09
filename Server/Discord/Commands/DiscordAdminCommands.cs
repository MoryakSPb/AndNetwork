using System;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands.Permissions;
using AndNetwork.Shared.Enums;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Commands
{
    [Group("admin")]
    internal class DiscordAdminCommands : DiscordCommandsBase
    {
        public DiscordAdminCommands(DiscordBot bot) : base(bot) { }

        [Command(nameof(Sync))]
        [Alias("update")]
        [Summary("Update channels, roles, nicknames and elections")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task Sync()
        {
            using (IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data))
            {
                await Bot.SyncGuild(data).ConfigureAwait(true);
            }

            await ReplyAsync("Синхронизация завершена").ConfigureAwait(false);
        }

        [Command(nameof(Time))]
        public async Task Time()
        {
            await ReplyAsync(DateTime.UtcNow.ToString("F", RussianCulture));
        }
    }
}
