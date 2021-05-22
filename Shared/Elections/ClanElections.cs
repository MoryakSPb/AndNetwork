using System;
using System.Collections.Generic;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Shared.Elections
{
    public class ClanElections
    {
        public int Id { get; set; }
        public DateTime AdvisorsStartDate { get; set; }
        public ClanElectionsStageEnum Stage { get; set; }

        public virtual IList<ClanElectionsVoting> Voting { get; set; } = null!;
    }
}
