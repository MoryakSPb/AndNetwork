﻿using System;
using System.Text.Json.Serialization;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Shared
{
    public class ClanAward : IComparable<ClanAward>
    {
        public int Id { get; set; }
        public ClanAwardTypeEnum Type { get; set; }
        [JsonIgnore]
        public virtual ClanMember Member { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? Description { get; set; }

        public int CompareTo(ClanAward? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            int dateComparison = Date.CompareTo(other.Date);
            if (dateComparison != 0) return dateComparison;
            return Type.CompareTo(other.Type);
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Description) ? $"{Type:G} {Date:d}" : $"{Type:G} {Date:d} {Description}";
    }
}
