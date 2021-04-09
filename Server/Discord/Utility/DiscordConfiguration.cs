using System.Collections.Generic;
using AndNetwork.Shared.Enums;
using Microsoft.Extensions.Configuration;

namespace AndNetwork.Server.Discord.Utility
{
    public class DiscordConfiguration
    {
        public Dictionary<ClanDepartmentEnum, ulong> ElectionMessages;
        public int BasicMap { get; set; } = 000000;
        public int GameMap { get; set; } = 100000;
        public int DepartmentMap { get; set; } = 200000;

        public int DruzhinaMap { get; set; } = 300000;
        public int ProgramMap { get; set; } = 400000;
        public int OfftopMap { get; set; } = 500000;
        public int PublicMap { get; set; } = 600000;

        public ulong ElectionMessagesChannel { get; set; }
        public string SiteUrl { get; set; }

        public DiscordConfiguration(IConfiguration configuration)
        {
            ElectionMessages = new Dictionary<ClanDepartmentEnum, ulong>
                               {
                                   {
                                       ClanDepartmentEnum.Infrastructure, ulong.Parse(configuration["Andromeda:Elections:MessagesChannel:Infrastructure"])
                                   },
                                   {
                                       ClanDepartmentEnum.Research, ulong.Parse(configuration["Andromeda:Elections:MessagesChannel:Research"])
                                   },
                                   {
                                       ClanDepartmentEnum.Military, ulong.Parse(configuration["Andromeda:Elections:MessagesChannel:Military"])
                                   },
                                   {
                                       ClanDepartmentEnum.Agitation, ulong.Parse(configuration["Andromeda:Elections:MessagesChannel:Agitation"])
                                   },
                               };
            SiteUrl = configuration["Andromeda:SiteUrl"];
            ElectionMessagesChannel = ulong.Parse(configuration["Andromeda:Elections:MessagesChannel:Channel"]);
        }
    }
}
