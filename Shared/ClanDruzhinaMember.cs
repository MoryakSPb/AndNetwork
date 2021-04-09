using System;
using System.Text.Json.Serialization;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Shared
{
    public class ClanDruzhinaMember
    {
        [JsonIgnore]
        public int DruzhinaId { get; set; }
        [JsonIgnore]
        public virtual ClanDruzhina Druzhina { get; set; }
        [JsonIgnore]
        public int MemberId { get; set; }
        public virtual ClanMember Member { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public ClanDruzhinaPositionEnum Position { get; set; }
    }
}
