using System;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands.Permissions;
using AndNetwork.Server.Discord.Utility;
using AndNetwork.Shared;
using AndNetwork.Shared.Elections;
using AndNetwork.Shared.Enums;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Commands
{
    [Group("elections")]
    public class DiscordElectionsCommands : DiscordCommandsBase
    {
        private readonly DiscordConfiguration _configuration;
        private readonly ElectionsService _electionsService;

        public DiscordElectionsCommands(DiscordBot bot, ElectionsService electionsService, DiscordConfiguration configuration) : base(bot)
        {
            _electionsService = electionsService;
            _configuration = configuration;
        }


        [Command("vote")]
        [AnyDepartmentPermission]
        public async Task Vote()
        {
            if (!Context.IsPrivate)
            {
                await ReplyAsync("Команда доступна только в личных сообщениях");
                return;
            }

            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanElections elections = await data.Elections.FindAsync(_electionsService.CurrentElectionsId);
            ClanMember member = await data.Members.FirstAsync(x => x.DiscordId == Context.User.Id);
            if (elections.Stage != ClanElectionsStageEnum.Voting)
            {
                await ReplyAsync("Голосование не проходит на данный момент");
                return;
            }

            Guid[] ids = elections.Voting.SelectMany(x => x.Results).Where(x => x.MemberId == member.Id && !x.Voted && x.VoterId != Guid.Empty && x.Votes is null).Select(x => x.VoterId).Distinct().ToArray();
            if (ids.Any())
            {
                string answer = "*Пожалуйста, не передавайте эти ссылки кому-либо, в том числе офицерам и советникам клана. Подобные ссылки уникальны для каждого участника клана.*";
                answer += Environment.NewLine + Environment.NewLine;
                await ReplyAsync(answer + string.Join(Environment.NewLine, ids.Select(x => $"http://{_configuration.SiteUrl}/elections/{member.Id}/{x:D}")));
            }

            else
                await ReplyAsync("У вас нет активных голосований");
        }

        [Command(nameof(Registration))]
        [Alias("reg")]
        [MinRankPermission(ClanMemberRankEnum.Assistant)]
        [NoCommanderPermission]
        [AnyDepartmentPermission]
        public async Task Registration()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id);
            bool result = _electionsService.TryRegCandidate(data, member);
            await ReplyAsync(result ? "Теперь вы кандидат на должность советника" : "Регистрация не удалась").ConfigureAwait(true);
            if (result) await _electionsService.UpdateMessages(Bot, data);
        }

        [Command(nameof(Registration))]
        [Alias("reg")]
        [MinRankPermission(ClanMemberRankEnum.FirstAdvisor)]
        public async Task Registration(int id)
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.Id == id);
            bool result = _electionsService.TryRegCandidate(data, member);
            await ReplyAsync(result ? $"Участник «{member}» зарегистрирован как кандидат на выборах" : "Регистрация не удалась").ConfigureAwait(true);
            if (result) await _electionsService.UpdateMessages(Bot, data);
        }

        [Command("new")]
        [MinRankPermission(ClanMemberRankEnum.FirstAdvisor)]
        public async Task New()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            await _electionsService.NewElections(data);
            await ReplyAsync("Новые выборы начались");
            await _electionsService.UpdateMessages(Bot, data);
        }

        [Command("start")]
        [MinRankPermission(ClanMemberRankEnum.FirstAdvisor)]
        public async Task Start()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            await _electionsService.StartVote(data);
            await ReplyAsync("Голосование запущено");
            await _electionsService.UpdateMessages(Bot, data);
        }

        [Command("end")]
        [MinRankPermission(ClanMemberRankEnum.FirstAdvisor)]
        public async Task End()
        {
            using IDisposable _ = Bot.GetDatabaseConnection(out ClanContext data);
            await _electionsService.EndVote(data);
            await ReplyAsync("Голосование отановлено");
            await _electionsService.UpdateMessages(Bot, data);
        }
    }
}
