using System;
using System.Collections.Concurrent;
using AndNetwork.Shared;

namespace AndNetwork.Server.Auth
{
    public static class AuthTokens
    {
        internal static ConcurrentDictionary<Guid, ClanMember> Tokens { get; }
    }
}
