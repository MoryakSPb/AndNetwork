using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Commands
{
    public class DiscordCommandsBase : ModuleBase<SocketCommandContext>
    {
        private protected static readonly CultureInfo RussianCulture;

        private protected readonly DiscordBot Bot;

        static DiscordCommandsBase()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru");
            culture.NumberFormat.CurrencySymbol = "SC";
            RussianCulture = culture;
        }

        public DiscordCommandsBase(DiscordBot bot) => Bot = bot;

        protected override async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null) =>
            await Context.Message.ReplyAsync(message, isTTS, embed, allowedMentions, options).ConfigureAwait(false);
    }
}
