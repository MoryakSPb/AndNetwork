using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AndNetwork.Shared.Elections;
using AndNetwork.Shared.Enums;
using AndNetwork.Shared.Programs;

namespace AndNetwork.Shared
{
    public class ClanMember : IEquatable<ClanMember>, IComparable<ClanMember>, IComparable
    {
        public int Id { get; set; }
        public ulong SteamId { get; set; }
        public ulong DiscordId { get; set; }

        public long? VkId { get; set; }
        public long? TelegramId { get; set; }

        public string Nickname { get; set; }
        public string? RealName { get; set; }

        public DateTime JoinDate { get; set; }

        public ClanMemberRankEnum Rank { get; set; }
        public ClanDepartmentEnum Department { get; set; }

        [JsonIgnore]
        public virtual ClanDruzhina? Druzhina { get; set; }
        [JsonIgnore]
        public virtual IList<ClanProgram> Programs { get; set; }
        public virtual IList<ClanAward> Awards { get; set; }
        [JsonIgnore]
        public virtual IList<ClanElectionsMember> VoteMember { get; set; }
        [JsonIgnore]
        public virtual IList<ClanDruzhinaMember> AllDruzhinasMember { get; set; }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is ClanMember other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ClanMember)}");
        }

        public int CompareTo(ClanMember? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            int rankComparison = Rank.CompareTo(other.Rank);
            if (rankComparison != 0) return rankComparison;
            int departmentComparison = Department.CompareTo(other.Department);
            if (departmentComparison != 0) return departmentComparison;
            return JoinDate.CompareTo(other.JoinDate);
        }

        public bool Equals(ClanMember? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ClanMember)obj);
        }

        public override int GetHashCode() => Id;

        public static bool operator ==(ClanMember left, ClanMember right) => Equals(left, right);

        public static bool operator !=(ClanMember left, ClanMember right) => !Equals(left, right);

        public static bool operator <(ClanMember left, ClanMember right) => Comparer<ClanMember>.Default.Compare(left, right) < 0;

        public static bool operator >(ClanMember left, ClanMember right) => Comparer<ClanMember>.Default.Compare(left, right) > 0;

        public static bool operator <=(ClanMember left, ClanMember right) => Comparer<ClanMember>.Default.Compare(left, right) <= 0;

        public static bool operator >=(ClanMember left, ClanMember right) => Comparer<ClanMember>.Default.Compare(left, right) >= 0;

        public override string ToString()
        {
            string? rankIcon = Rank.GetRankIcon();
            string result = string.Empty;
            if (rankIcon is not null) result += $"[{rankIcon}] ";
            result += Nickname;
            if (RealName is not null) result += $" ({RealName})";
            return result;
        }
    }
}
