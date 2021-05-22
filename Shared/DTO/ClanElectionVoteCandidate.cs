namespace AndNetwork.Shared.DTO
{
    public class ClanElectionVoteCandidate
    {
        public ClanMember Member { get; set; } = null!;
        public int Votes { get; set; }
    }
}
