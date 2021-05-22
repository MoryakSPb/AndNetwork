using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands.Permissions;
using AndNetwork.Server.Discord.Managers;
using AndNetwork.Server.Discord.Utility;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server.Discord.Commands
{
    [Group("send")]
    public class DiscordSendCommands : DiscordCommandsBase
    {
        private readonly DiscordMessagesManager _messagesManager;

        public DiscordSendCommands(DiscordBot bot) : base(bot) => _messagesManager = Bot.MessagesManager;

        [Command("online")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task SendOnline(string message)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            IEnumerable<ClanMember> clanMembers = Bot.Guild.Users.Where(x => x.Status == UserStatus.Online || x.Status == UserStatus.AFK || x.Status == UserStatus.Idle).Join(data.Members, x => x.Id, x => x.DiscordId, (_, member) => member);
            foreach (ClanMember member in clanMembers) await _messagesManager.Send(member, message);
        }

        [Command("all")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task SendAll(string message)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            IEnumerable<ClanMember> clanMembers = Bot.GetGuild(266673838811512832ul).Users.Join(data.Members, x => x.Id, x => x.DiscordId, (_, member) => member).Where(x => x.Rank > ClanMemberRankEnum.None).ToArray();
            foreach (ClanMember member in clanMembers)
                try
                {
                    await _messagesManager.Send(member, message);
                }
                catch (Exception e)
                {
                    Bot.Logger.LogWarning(e, $"Ошибка при отправке сообщения участнику {member}");
                }
        }

        [Command("dm")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task Send(int id, string message)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FindAsync(id);
            if (member is null) await ReplyAsync("Игрок не найден");
            else
            {
                await ReplyAsync("Сообщение отправлено");
                await _messagesManager.Send(member, message);
            }
        }

        [Command("elect")]
        [MinRankPermission(ClanMemberRankEnum.FirstAdvisor)]
        public async Task Send(string message)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            IEnumerable<ClanMember> members = (await data.Elections.FindAsync(Bot.ElectionsService.CurrentElectionsId)).Voting.SelectMany(x => x.Results).Select(x => x.Member).Distinct();

            await _messagesManager.Send(await data.Members.FindAsync(1), message);

            foreach (ClanMember member in members)
            {
                await ReplyAsync("Сообщение отправлено");
                await _messagesManager.Send(member, message);
            }
        }

        [Command("department")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task SendDepartment(ClanDepartmentEnum department, string message)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            IEnumerable<ClanMember> clanMembers = data.Members.AsQueryable().Where(x => x.Department == department || x.Rank == ClanMemberRankEnum.FirstAdvisor);
            foreach (ClanMember member in clanMembers) await _messagesManager.Send(member, message);
        }

        [Command("noreaction")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task SendNoReaction(ulong channelId, ulong messageId, string text)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            SocketTextChannel channel = Bot.GetGuild(Bot.GuildId).GetTextChannel(channelId);
            if (channel is null)
            {
                await ReplyAsync("Канал не найден");
                return;
            }

            IMessage message = await channel.GetMessageAsync(messageId);
            if (message is null)
            {
                await ReplyAsync("Сообщение не найдено");
                return;
            }

            DiscordUserEqualityComparer comparer = new();
            (IUser user, ClanMember member)[] targets = channel.Users.Where(x => !x.IsBot).Except(message.Reactions.SelectMany(x => message.GetReactionUsersAsync(x.Key, int.MaxValue).ToEnumerable()).SelectMany(x => x).Where(x => !x.IsBot).Distinct(comparer), comparer).Join(data.Members, x => x.Id, x => x.DiscordId, (user, member) => (user, member)).ToArray();
            await ReplyAsync(string.Join(Environment.NewLine, targets.Select(x => x.member.ToString())));
            foreach ((IUser user, ClanMember member) in targets) await _messagesManager.Send(user, member, text);
        }

        [Command("reaction")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task SendReaction(ulong channelId, ulong messageId, string text)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            SocketTextChannel channel = Bot.GetGuild(Bot.GuildId).GetTextChannel(channelId);
            if (channel is null)
            {
                await ReplyAsync("Канал не найден");
                return;
            }

            IMessage message = await channel.GetMessageAsync(messageId);
            if (message is null)
            {
                await ReplyAsync("Сообщение не найдено");
                return;
            }

            IUser[] users = message.Reactions.SelectMany(x => message.GetReactionUsersAsync(x.Key, int.MaxValue).ToEnumerable()).SelectMany(x => x).ToArray();
            foreach (IUser user in users) await user.SendMessageAsync(text);
            await ReplyAsync("Сообщение доставлено:" + Environment.NewLine + string.Join(Environment.NewLine, users.Select(x => $"<@{x.Id}>")));
        }
    }
}
