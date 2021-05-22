using System.Collections.Generic;

namespace AndNetwork.Shared.DTO
{
    public class ClanElectionVote
    {
        public ClanElectionCode Code { get; set; } = null!;
        public List<ClanElectionVoteCandidate> Votes { get; set; } = null!;
        public int AgainstAll { get; set; }
    }
}
