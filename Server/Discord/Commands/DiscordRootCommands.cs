using System.IO;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands.Permissions;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Commands
{
    public class DiscordRootCommands : DiscordCommandsBase
    {
        private static readonly string HelpText = File.ReadAllText("help.md");

        public DiscordRootCommands(DiscordBot bot) : base(bot) { }

        [Command("?")]
        [Alias("help")]
        [Summary("Возвращает справку по всем командам")]
        [MinRankPermission]
        public async Task Help()
        {
            await ReplyAsync(HelpText).ConfigureAwait(false);
        }
    }
}
