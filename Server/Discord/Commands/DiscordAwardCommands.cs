using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands.Permissions;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server.Discord.Commands
{
    [Group("award")]
    public class DiscordAwardCommands : DiscordCommandsBase
    {
        private readonly ILogger<DiscordAwardCommands> _logger;

        public DiscordAwardCommands(DiscordBot bot, ILogger<DiscordAwardCommands> logger) : base(bot) => _logger = logger;

        [Command("give")]
        [Summary("Give bronze award")]
        [MinRankPermission(ClanMemberRankEnum.Advisor)]
        public async Task GiveBronze(string description, params int[] membersId)
        {
            using IDisposable logScope = _logger.BeginScope(this);
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);

            StringBuilder text = new();
            text.AppendLine($"Бронзовую награду «{description}» получили:");
            IAsyncEnumerable<ClanMember> members = data.Members.AsAsyncEnumerable().Join(membersId.ToAsyncEnumerable().Distinct(), x => x.Id, x => x, (member, _) => member);
            foreach (ClanMember member in await members.ToArrayAsync())
            {
                member.Awards.Add(new ClanAward
                                  {
                                      Date = DateTime.Today,
                                      Description = string.IsNullOrWhiteSpace(description) ? null : description,
                                      Member = member,
                                      Type = ClanAwardTypeEnum.Bronze,
                                  });
                _logger.LogInformation($"{Context.User} adds bronze award «{description}» to {member}");
                text.AppendLine(member.ToString());
            }

            await data.SaveChangesAsync();
            await ReplyAsync(text.ToString());
        }

        [Command("rise")]
        public async Task AutoRise()
        {
            using IDisposable logScope = _logger.BeginScope(this);
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);

            foreach (ClanMember member in await data.Members.AsQueryable().Where(x => x.Department > ClanDepartmentEnum.None && x.Rank < ClanMemberRankEnum.Lieutenant).ToArrayAsync())
            {
                ClanMemberRankEnum rank = member.Awards.GetRank();
                if (member.Rank != rank)
                {
                    member.Rank = rank;
                    _logger.LogInformation($"Member «{member}» gets rank {member.Rank}");
                }
            }

            await data.SaveChangesAsync().ConfigureAwait(true);
            await ReplyAsync("Готово");
        }
    }
}
