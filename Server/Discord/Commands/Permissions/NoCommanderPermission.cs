using System;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Commands.Permissions
{
    public class NoCommanderPermission : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            ClanContext data = (ClanContext)services.GetService(typeof(ClanContext));
            if (data is null) throw new ArgumentException("ClanContext is null", nameof(services));
            ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.DiscordId == context.User.Id);
            return member.Rank == ClanMemberRankEnum.Captain || member.Programs.Any(x => x.Initiator == member) ? PreconditionResult.FromError("Доступ запрещен") : PreconditionResult.FromSuccess();
        }
    }
}
