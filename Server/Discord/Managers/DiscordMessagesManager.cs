using System;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using Discord;
using Discord.Net;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server.Discord.Managers
{
    public class DiscordMessagesManager
    {
        private readonly DiscordBot _bot;

        internal DiscordMessagesManager(DiscordBot bot) => _bot = bot;

        public async Task SendAll(string message, ClanContext data)
        {
            foreach (ClanMember member in data.Members.AsQueryable().Where(x => x.Rank > ClanMemberRankEnum.None).ToArray()) await Send(member, message);
        }

        public async Task SendDepartment(string message, ClanDepartmentEnum department, ClanContext data)
        {
            foreach (ClanMember member in data.Members.AsQueryable().Where(x => x.Department == department).ToArray()) await Send(member, message);
        }

        public async Task<IUserMessage> Send(ClanMember member, string message) => await Send(_bot?.GetUser(member.DiscordId), member, message);

        public async Task<IUserMessage> Send(IUser user, ClanMember member, string message)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (member is null) throw new ArgumentNullException(nameof(member));
            string result = string.Format(message, member.Nickname, member.Rank.GetRankName());
            try
            {
                _bot.Logger.LogInformation($"Send message to {user}");
                return await user.SendMessageAsync(result);
            }
            catch (HttpException e)
            {
                _bot.Logger.Log(LogLevel.Information, e, $"Direct message is not delivered to user {member}");
            }

            return null;
        }
    }
}
