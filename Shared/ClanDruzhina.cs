using System;
using System.Collections.Generic;
using System.Linq;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Shared
{
    public class ClanDruzhina
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ClanDepartmentEnum Department { get; set; }
        public IList<ClanDruzhinaMember> ActiveMembers => DisbandDate is not null && DisbandDate < CreationDate.Date ? new List<ClanDruzhinaMember>(0) : MembersHistory.Where(x => x.LeaveDate is null || x.LeaveDate <= DateTime.Today).ToList();
        public virtual IList<ClanDruzhinaMember> MembersHistory { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public DateTime? DisbandDate { get; set; }
    }
}
