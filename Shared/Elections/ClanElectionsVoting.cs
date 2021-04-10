using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Shared.Elections
{
    public class ClanElectionsVoting
    {
        [JsonIgnore]
        public int ElectionsId { get; set; }
        [JsonIgnore]
        public virtual ClanElections Elections { get; set; } = null!;
        public virtual ClanDepartmentEnum Department { get; set; }

        public int AgainstAll { get; set; } = 0;
        public virtual IList<ClanElectionsMember> Results { get; set; } = null!;
        [JsonIgnore]
        public int VotesCount => Results.Count(x => x.Votes is not null);
    }
}
