using System.Collections.Generic;
using Discord;

namespace AndNetwork.Server.Discord.Utility
{
    public class DiscordUserEqualityComparer : IEqualityComparer<IUser>, IEqualityComparer<IEntity<ulong>>
    {
        public bool Equals(IEntity<ulong> x, IEntity<ulong> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(IEntity<ulong> obj) => obj.Id.GetHashCode();

        public bool Equals(IUser x, IUser y) => Equals((IEntity<ulong>)x, y);

        public int GetHashCode(IUser obj) => GetHashCode((IEntity<ulong>)obj);
    }
}
