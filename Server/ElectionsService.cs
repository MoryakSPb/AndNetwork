using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndNetwork.Server.Discord;
using AndNetwork.Shared;
using AndNetwork.Shared.Elections;
using AndNetwork.Shared.Enums;
using Discord.Rest;

namespace AndNetwork.Server
{
    public class ElectionsService
    {
        internal ClanElections CurrentElections { private get; set; }
        public int CurrentElectionsId => CurrentElections.Id;

        public bool TryRegCandidate(ClanContext data, ClanMember member)
        {
            CurrentElections = data.Elections.Find(CurrentElectionsId);
            if (CurrentElections.Stage != ClanElectionsStageEnum.Registration) return false;
            if (member.Rank < ClanMemberRankEnum.Assistant) return false;
            if (member.Rank == ClanMemberRankEnum.Captain) return false;
            if (member.Programs.Any(x => x.Initiator == member)) return false;
            if (CurrentElections.Voting.Any(x => x.Results.Any(y => y.MemberId == member.Id && y.Votes is not null))) return false;

            CurrentElections.Voting.Single(x => x.Department == member.Department).Results.Add(new ClanElectionsMember
                                                                                               {
                                                                                                   Department = member.Department,
                                                                                                   MemberId = member.Id,
                                                                                                   Votes = 0,
                                                                                                   ElectionsId = CurrentElections.Id,
                                                                                               });
            data.SaveChanges();
            return true;
        }

        public async Task NewElections(ClanContext data)
        {
            if (CurrentElections is not null) CurrentElections = await data.Elections.FindAsync(CurrentElectionsId);
            if (CurrentElections is not null) CurrentElections.Stage = ClanElectionsStageEnum.Ended;
            foreach (ClanMember advisor in data.Members.AsQueryable().Where(x => x.Rank == ClanMemberRankEnum.Advisor).ToArray()) advisor.Rank = advisor.Awards.GetRank();

            if (CurrentElections is not null)
                foreach (ClanElectionsVoting vote in CurrentElections.Voting)
                {
                    ClanElectionsMember[] winners = vote.Results.Where(x => x.Votes is not null).OrderByDescending(x => x.Votes.Value).ToArray();
                    Random random = new(CurrentElectionsId);
                    ClanElectionsMember winner = winners.Any() ? winners[random.Next(0, winners.Length)] : null;
                    if (winner is not null && winner.Votes >= vote.AgainstAll) winner.Member.Rank = ClanMemberRankEnum.Advisor;
                }

            ClanElections elections = new()
                                      {
                                          AdvisorsStartDate = DateTime.Today.AddDays(ClanRules.ADVISOR_TERM_DAYS),
                                          Stage = ClanElectionsStageEnum.Registration,
                                          Voting = new List<ClanElectionsVoting>(),
                                      };

            foreach (ClanDepartmentEnum department in Enum.GetValues<ClanDepartmentEnum>().Where(x => x > ClanDepartmentEnum.None))
                elections.Voting.Add(new ClanElectionsVoting
                                     {
                                         Department = department,
                                         Elections = elections,
                                         Results = new List<ClanElectionsMember>(),
                                     });

            CurrentElections = elections;
            await data.Elections.AddAsync(elections);
            await data.SaveChangesAsync();
        }

        public async Task StartVote(ClanContext data)
        {
            CurrentElections = await data.Elections.FindAsync(CurrentElectionsId);
            foreach (ClanMember member in data.Members.AsQueryable().Where(x => x.Department > ClanDepartmentEnum.None).ToArray())
            {
                Guid code = Guid.NewGuid();
                foreach (ClanElectionsVoting vote in CurrentElections.Voting)
                {
                    if (vote.Department == member.Department) continue;

                    vote.Results.Add(new ClanElectionsMember
                                     {
                                         Department = vote.Department,
                                         ElectionsId = CurrentElectionsId,
                                         MemberId = member.Id,
                                         Voted = false,
                                         Votes = null,
                                         VoterId = code,
                                     });
                }
            }

            CurrentElections.Stage = ClanElectionsStageEnum.Voting;
            await data.SaveChangesAsync();
        }

        public async Task EndVote(ClanContext data)
        {
            CurrentElections = await data.Elections.FindAsync(CurrentElectionsId);
            foreach (ClanElectionsVoting vote in CurrentElections.Voting)
            foreach (ClanElectionsMember electionsMember in vote.Results)
                electionsMember.VoterId = Guid.Empty;

            CurrentElections.Stage = ClanElectionsStageEnum.Announcement;
            await data.SaveChangesAsync();
        }

        public async Task UpdateMessages(DiscordBot bot, ClanContext data)
        {
            foreach (ClanDepartmentEnum department in Enum.GetValues<ClanDepartmentEnum>().Where(x => x > ClanDepartmentEnum.None))
            {
                RestUserMessage message = (RestUserMessage)await bot.Guild.GetTextChannel(bot.Configuration.ElectionMessagesChannel).GetMessageAsync(bot.Configuration.ElectionMessages[department]);
                await message.ModifyAsync(x => { x.Content = GetMessage(department, data); });
            }
        }


        private string GetMessage(ClanDepartmentEnum department, ClanContext data)
        {
            return CurrentElections.Stage switch
            {
                ClanElectionsStageEnum.Registration => GetRegistrationMessage(department, data),
                ClanElectionsStageEnum.Voting => GetVotingMessage(department, data),
                ClanElectionsStageEnum.Announcement => GetVotingMessage(department, data),
                _ => throw new ArgumentOutOfRangeException(nameof(department), department, null),
            };
        }

        private string GetRegistrationMessage(ClanDepartmentEnum department, ClanContext data)
        {
            CurrentElections = data.Elections.Find(CurrentElectionsId);
            ClanElectionsVoting voting = CurrentElections.Voting.First(x => x.Department == department);
            ClanElectionsMember[] candidates = voting.Results.Where(x => x.Votes is not null).ToArray();
            StringBuilder text = new(2000);
            text.AppendFormat("**{0}**", department.GetName());

            text.AppendLine();
            text.Append("Предварительное число избирателей: ");
            text.Append(data.Members.Count(x => x.Department > ClanDepartmentEnum.None && x.Department != department));
            text.AppendLine();
            text.AppendLine();

            if (candidates.Length == 0)
                text.AppendLine("На данный момент нет претендентов на пост советника");
            else
            {
                text.AppendLine("Кандидаты на пост советника:");
                foreach (ClanElectionsMember member in candidates) text.AppendLine($"<@{member.Member.DiscordId:D}>");
            }

            return text.ToString();
        }

        private string GetVotingMessage(ClanDepartmentEnum department, ClanContext data)
        {
            const string againstAllString = "Против всех";

            CurrentElections = data.Elections.Find(CurrentElectionsId);
            ClanElectionsVoting voting = CurrentElections.Voting.First(x => x.Department == department);
            List<MessageCandidate> candidates = voting.Results.Where(x => x.Votes is not null).Select(x => new MessageCandidate
                                                                                                           {
                                                                                                               Nickname = x.Member.Nickname,
                                                                                                               Votes = x.Votes.Value,
                                                                                                               Rank = x.Member.Rank.GetAsciiRankIcon(),
                                                                                                           }).ToList();
            candidates.Add(new MessageCandidate
                           {
                               Votes = voting.AgainstAll,
                               Rank = null,
                               Nickname = againstAllString,
                           });
            const int bar = 20;
            int total = candidates.Sum(x => x.Votes);
            ClanElectionsMember[] voters = voting.Results.Where(x => x.Votes is null).ToArray();

            StringBuilder text = new(2000);
            text.AppendFormat("**{0}**", department.GetName());
            text.AppendLine();
            text.Append("Явка: ");
            (int totalVoters, int currentVoters) = voters.Aggregate((0, 0), (results, member) => (results.Item1 + 1, results.Item2 + (member.Voted ? 1 : 0)));

            text.Append(currentVoters);
            text.Append('/');
            text.Append(totalVoters);
            text.Append('\t');
            text.Append('(');
            text.Append((currentVoters / (double)totalVoters).ToString("P0", CultureInfo.InvariantCulture));
            text.Append(')');
            text.AppendLine("```");

            const int rankMax = 5;
            int nicknameMax = Math.Max(againstAllString.Length, data.Members.Max(x => x.Nickname.Length));

            foreach (MessageCandidate candidate in candidates.OrderByDescending(x => x.Votes))
            {
                double percent = candidate.Votes / (double)total;
                if (double.IsNaN(percent)) percent = 0;

                if (candidate.Rank is not null)
                {
                    text.Append($"[{candidate.Rank}]");
                    text.Append(' ', rankMax - candidate.Rank.Length + 1);
                }
                else
                    text.Append(' ', rankMax + 3);

                text.Append(candidate.Nickname);
                text.Append(' ', nicknameMax - candidate.Nickname.Length + 1);

                int barPoints = (int)Math.Round(bar * percent, MidpointRounding.ToZero);
                text.Append('[');
                text.Append('|', barPoints);
                text.Append('-', bar - barPoints);
                text.Append(']');
                string voteStr = candidate.Votes.ToString("D");
                text.Append(' ', 4 - voteStr.Length);
                text.Append(voteStr);
                text.Append(' ');
                text.Append('(');
                string p = percent switch
                {
                    < 0.1d => percent.ToString("P3", CultureInfo.InvariantCulture),
                    < 1d => percent.ToString("P2", CultureInfo.InvariantCulture),
                    _ => percent.ToString("P1", CultureInfo.InvariantCulture),
                };
                text.Append(p);
                text.Append(')');
                text.AppendLine();
            }

            text.AppendLine("```");
            return text.ToString();
        }


        private record MessageCandidate
        {
            public string Nickname { get; init; }
            public string Rank { get; init; }
            public int Votes { get; init; }
        }
    }
}
