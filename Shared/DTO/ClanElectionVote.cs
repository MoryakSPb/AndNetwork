using System.Collections.Generic;

namespace AndNetwork.Shared.DTO
{
    public class ClanElectionVote
    {
        public ClanElectionCode Code { get; set; }
        public List<ClanElectionVoteCandidate> Votes { get; set; }
        public int AgainstAll { get; set; }
    }
}
