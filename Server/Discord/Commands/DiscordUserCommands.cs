using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands.Permissions;
using AndNetwork.Server.Discord.Utility;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using AndNetwork.Shared.Programs;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Commands
{
    [Group("member")]
    internal class DiscordUserCommands : DiscordCommandsBase
    {
        public DiscordUserCommands(DiscordBot bot) : base(bot) { }

        [Command("info")]
        public async Task Get()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id);
            await Get(member).ConfigureAwait(true);
        }

        [Command("info")]
        [MinRankPermission(ClanMemberRankEnum.Trainee)]
        public async Task Get(int id)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FindAsync(id);
            await Get(member).ConfigureAwait(true);
        }

        [Command("info")]
        [MinRankPermission(ClanMemberRankEnum.Trainee)]
        public async Task Get(string nickname)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.Nickname == nickname);
            await Get(member).ConfigureAwait(true);
        }

        [Command("award")]
        public async Task GetAward()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id);
            await GetAward(member).ConfigureAwait(true);
        }

        [Command("award")]
        [MinRankPermission(ClanMemberRankEnum.Trainee)]
        public async Task GetAward(int id)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FindAsync(id);
            await GetAward(member).ConfigureAwait(true);
        }

        [Command("award")]
        [MinRankPermission(ClanMemberRankEnum.Trainee)]
        public async Task GetAward(string nickname)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.Nickname == nickname);
            await GetAward(member).ConfigureAwait(true);
        }

        public async Task Get(ClanMember member)
        {
            if (member is null) await ReplyAsync("Участник не найден").ConfigureAwait(false);
            else
            {
                StringBuilder text = new();
                addField("Ник", member.Nickname);
                addField("Имя", member.RealName);
                addField("ID", member.Id.ToString());
                text.AppendLine();
                addField("Ранг", member.Rank.GetRankName());

                switch (member.Department)
                {
                    case > ClanDepartmentEnum.None:
                        addField("Отдел", member.Department.GetName());
                        break;
                    case < ClanDepartmentEnum.None:
                        addField("Общность игроков", member.Department.GetName());
                        break;
                }

                addField("Дружина", member.Druzhina?.Id.ToString());
                ClanProgram[] programs = member.Programs.Where(x => x.DisbandDate > DateTime.Today).ToArray();
                if (programs.Any()) addField("Программа", string.Join(", ", member.Programs.Where(x => x.DisbandDate > DateTime.Today).Select(x => x.Id.ToString())));
                text.AppendLine();
                addField("Steam", "http://steamcommunity.com/profiles/" + member.SteamId.ToString("D"));
                addField("Discord", $"<@{member.DiscordId:D}>");
                if (member.VkId is not null) addField("ВК", $"https://vk.com/id{member.VkId}");
                if (member.Rank > ClanMemberRankEnum.None && member.Awards.Any())
                {
                    text.AppendLine();
                    string awards = string.Join(Environment.NewLine, member.Awards.Aggregate(new Dictionary<ClanAwardTypeEnum, int>(Enum.GetValues<ClanAwardTypeEnum>().Reverse().Select(x => new KeyValuePair<ClanAwardTypeEnum, int>(x, 0))), (counts, award) =>
                    {
                        counts[award.Type]++;
                        return counts;
                    }).Where(x => x.Value > 0).Select(x => $"{x.Key.GetAwardSymbol()} × {x.Value:D}"));
                    addField("Награды", member.Awards.Sum(x => (int)x.Type) + Environment.NewLine + awards);
                }


                await ReplyAsync(text.ToString());

                void addField(string type, string value)
                {
                    if (type is null) return;
                    if (value is null) return;
                    text.Append('*', 2);
                    text.Append(type);
                    text.Append('*', 2);
                    text.Append(':');
                    text.Append(' ');
                    text.Append(value);
                    text.AppendLine();
                }
            }
        }

        public async Task GetAward(ClanMember member)
        {
            if (member is null) await ReplyAsync("Участник не найден").ConfigureAwait(false);
            else await ReplyAsync(string.Join(Environment.NewLine, member.Awards.GetStrings())).ConfigureAwait(true);
        }

        [Command("deserters")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task Deserters()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            IEnumerable<ulong> deserterIds = data.Members.AsQueryable().Where(x => x.Rank > ClanMemberRankEnum.None).Select(x => x.DiscordId).ToArray().Except(Bot.GetGuild(266673838811512832).Users.Select(x => x.Id));
            ClanMember[] deserters = deserterIds.Join(data.Members, x => x, x => x.DiscordId, (_, member) => member).ToArray();
            await ReplyAsync(string.Join(Environment.NewLine, deserters.Select(x => x.ToString())));
            foreach (ClanMember deserter in deserters)
            {
                deserter.Rank = ClanMemberRankEnum.Guest;
                deserter.Department = ClanDepartmentEnum.None;
            }

            await data.SaveChangesAsync();
        }
    }
}
