using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Channels;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace AndNetwork.Server.Discord.Managers
{
    public class DiscordChannelManager
    {
        private readonly DiscordBot _bot;
        private readonly DiscordRoleManager _roleManager;

        internal DiscordChannelManager(DiscordBot bot, DiscordRoleManager roleManager)
        {
            _bot = bot;
            _roleManager = roleManager;
        }
        
        public async Task SyncChannels(ClanContext data)
        {
            Dictionary<ClanDepartmentEnum, ClanMember> advisors = await data.Members.AsQueryable().Where(x => x.Rank == ClanMemberRankEnum.Advisor).ToDictionaryAsync(x => x.Department, x => x);
            foreach (DiscordChannelMetadata metadata in data.Channels.ToArray())
            {
                SocketGuildChannel channel = _bot.Guild.GetChannel(metadata.DiscordId);
                await channel.ModifyAsync(options => { options.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(metadata.ToOverwrites(_roleManager, advisors)); });
            }
        }

        public async Task SortChannels(ClanContext data)
        {
            foreach (DiscordChannelMetadata metadata in data.Channels.ToArray())
            {
                SocketGuildChannel channel = _bot.Guild.GetChannel(metadata.DiscordId);

                if (channel.Position != metadata.ChannelPosition) await channel.ModifyAsync(options => { options.Position = new Optional<int>(metadata.ChannelPosition); });
                if (channel.Name != metadata.Name) await channel.ModifyAsync(options => { options.Name = new Optional<string>(metadata.Name); });
            }
        }
    }
}
