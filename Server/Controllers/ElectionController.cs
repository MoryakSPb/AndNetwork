using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Server.Discord;
using AndNetwork.Shared;
using AndNetwork.Shared.DTO;
using AndNetwork.Shared.Elections;
using AndNetwork.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionController : ControllerBase
    {
        private readonly DiscordBot _bot;
        private readonly ClanContext _data;
        private readonly ILogger<ElectionController> _logger;

        public ElectionController(ClanContext data, DiscordBot bot, ILogger<ElectionController> logger)
        {
            _data = data;
            _bot = bot;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<ClanElectionVote> Get() => Ok();

        [HttpGet("{memberId}/{department}/{uuid}")]
        public async Task<ActionResult<ClanElectionVote>> Get(int memberId, int department, Guid uuid)
        {
            if (uuid == Guid.Empty) return NotFound();
            ClanElectionCode code = new()
                                    {
                                        Department = (ClanDepartmentEnum)department,
                                        Code = uuid,
                                        MemberId = memberId,
                                    };
            (bool result, ClanElectionsVoting vote) = await CheckCode(code);
            _logger.LogInformation($"#{code.MemberId} gets election data");
            return result
                ? new ClanElectionVote
                  {
                      Code = code,
                      Votes = vote.Results.Where(x => x.Votes is not null).Select(x => new ClanElectionVoteCandidate
                                                                                       {
                                                                                           Member = _data.Members.Find(x.MemberId),
                                                                                           Votes = x.Votes.Value,
                                                                                       }).ToList(),
                      AgainstAll = vote.AgainstAll,
                  }
                : NotFound();
        }

        [HttpPost("{memberId}/{department}/{uuid}")]
        public async Task<ActionResult> Post(int memberId, int department, Guid uuid, [FromBody] Dictionary<int, int> votes)
        {
            if (uuid == Guid.Empty) return NotFound();
            ClanElectionCode code = new()
                                    {
                                        Department = (ClanDepartmentEnum)department,
                                        Code = uuid,
                                        MemberId = memberId,
                                    };
            (bool result, ClanElectionsVoting vote) = await CheckCode(code);
            if (!result) return StatusCode(403);
            if (votes.Values.Any(x => x < 0) || votes.Values.Sum() != vote.VotesCount) return StatusCode(403);

            if (votes.Keys.Where(key => key != 0).Any(key => !vote.Results.Any(x => x.MemberId == key && x.Votes is not null))) return NotFound();

            foreach ((int id, int value) in votes)
                if (id == 0) vote.AgainstAll += value;
                else vote.Results.First(x => x.MemberId == id).Votes += value;

            vote.Results.First(x => x.MemberId == code.MemberId).Voted = true;
            await _data.SaveChangesAsync();
            await _bot.ElectionsService.UpdateMessages(_bot, _data);

            return Accepted();
        }

        private async Task<(bool, ClanElectionsVoting)> CheckCode(ClanElectionCode code)
        {
            ClanElectionsVoting vote;
            ClanElections elections = await _data.Elections.FindAsync(_bot.ElectionsService.CurrentElectionsId);
            if (elections is null || elections.Stage != ClanElectionsStageEnum.Voting)
            {
                vote = null;
                return default;
            }

            ClanMember member = await _data.Members.FindAsync(code.MemberId);
            vote = elections.Voting.FirstOrDefault(x => x.Department == code.Department);
            if (vote is null) return default;
            if (member is null) return default;
            ClanElectionsMember voteMember = vote.Results.FirstOrDefault(x => x.MemberId == code.MemberId && x.Department != member.Department && x.Votes is null && x.VoterId == code.Code && !x.Voted);
            if (voteMember is null) return default;
            _logger.LogInformation($"#{member} send election data");
            return (true, vote);
        }
    }
}
