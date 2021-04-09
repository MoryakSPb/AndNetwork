using System;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Shared.DTO
{
    public class ClanElectionCode
    {
        public int MemberId { get; set; }
        public Guid Code { get; set; }
        public ClanDepartmentEnum Department { get; set; }
    }
}
