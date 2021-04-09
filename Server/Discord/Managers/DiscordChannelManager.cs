using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Channels;
using Discord;
using Discord.WebSocket;

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

        public async Task ScanChannels(ClanContext data)
        {
            foreach (SocketCategoryChannel category in _bot.Guild.CategoryChannels)
                if (await data.ChannelCategories.AnyAsync(x => x.DiscordId == category.Id))
                    continue;
            /*await data.ChannelCategories.AddAsync(new DiscordChannelCategory
                                                      {
                                                          DiscordId = category.Id,
                                                          Name = category.Name,
                                                          Position = category.Position,
                                                      });*/

            await data.SaveChangesAsync();
            foreach (SocketTextChannel channel in _bot.Guild.TextChannels)
                if (await data.Channels.AnyAsync(x => x.DiscordId == channel.Id))
                    continue;
            /*await data.Channels.AddAsync(new DiscordChannelMetadata
                                             {
                                                 DiscordId = channel.Id,
                                                 Name = channel.Name,
                                                 CategoryPosition = channel.Category?.Position,
                                                 ChannelPosition = channel.Position,
                                                 Type = DiscordChannelTypeEnum.Text,
                                             });*/

            foreach (SocketVoiceChannel channel in _bot.Guild.VoiceChannels)
                if (await data.Channels.AnyAsync(x => x.DiscordId == channel.Id))
                    continue;
            /*await data.Channels.AddAsync(new DiscordChannelMetadata
                                             {
                                                 DiscordId = channel.Id,
                                                 Name = channel.Name,
                                                 CategoryPosition = channel.Category?.Position,
                                                 ChannelPosition = channel.Position,
                                                 Type = DiscordChannelTypeEnum.Voice,
                                             });*/

            await data.SaveChangesAsync();
        }

        public async Task SyncChannels(ClanContext data)
        {
            foreach (DiscordChannelMetadata metadata in data.Channels.ToArray())
            {
                SocketGuildChannel channel = _bot.Guild.GetChannel(metadata.DiscordId);

                await channel.ModifyAsync(options => { options.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(metadata.ToOverwrites(_roleManager)); });
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
