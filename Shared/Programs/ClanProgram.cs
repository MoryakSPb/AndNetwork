using System;
using System.Collections.Generic;

namespace AndNetwork.Shared.Programs
{
    public class ClanProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<ClanProgramTask> Tasks { get; set; }
        public virtual ClanMember Initiator { get; set; }
        public virtual IList<ClanMember> Members { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DisbandDate { get; set; }
    }
}
