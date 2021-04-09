using System;

namespace AndNetwork.Server.Auth
{
    public record AuthToken(in Guid Token, in DateTime ExpireTime, in int MemberId);
}
