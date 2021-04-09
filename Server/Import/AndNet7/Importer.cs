using System;
using System.Collections.Generic;
using System.Linq;
using AndNetwork.Server.Import.AndNet7.Objects;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;

namespace AndNetwork.Server.Import.AndNet7
{
    public static class Importer
    {
        private static ClanAwardTypeEnum GetAwardType(this int idr) => idr switch
        {
            101 => ClanAwardTypeEnum.Hero,
            { } i when i >= 102 && i <= 106 => ClanAwardTypeEnum.Gold,
            { } i when i >= 107 && i <= 112 => ClanAwardTypeEnum.Silver,
            { } i when i >= 201 && i <= 208 => ClanAwardTypeEnum.Bronze,
            _ => ClanAwardTypeEnum.None,
        };

        private static ClanDepartmentEnum GetDepartment(this Player player) => player.Department switch
        {
            0 => ClanDepartmentEnum.None,
            1 => ClanDepartmentEnum.Infrastructure,
            2 => ClanDepartmentEnum.Research,
            3 => ClanDepartmentEnum.Military,
            4 => ClanDepartmentEnum.Agitation,
            byte.MaxValue => ClanDepartmentEnum.Reserve,
            { } when player.Post == 1 => ClanDepartmentEnum.BeginnersPool,
            _ => throw new ArgumentOutOfRangeException(nameof(player)),
        };

        public static IEnumerable<ClanMember> GetData(Save save)
        {
            foreach (Player player in save.Players)
            {
                ClanMember member = new()
                                    {
                                        Awards = GetMemberAwards(save, player.SteamId).ToArray(),
                                        Department = player.GetDepartment(),
                                        DiscordId = player.DiscordId,
                                        SteamId = player.SteamId,
                                        VkId = player.VkId,
                                        Nickname = player.Name,
                                        RealName = player.RealName,
                                        JoinDate = player.JoinDate,
                                    };
                member.Rank = member.Awards.GetRank();
                yield return member;
            }
        }

        public static IEnumerable<ClanAward> GetMemberAwards(Save save, ulong steamId)
        {
            return save.Awards.Where(x => x.PlayerSteamId == steamId).Select(x => new ClanAward
                                                                                  {
                                                                                      Date = x.Time,
                                                                                      Type = x.Idr.GetAwardType(),
                                                                                      Description = string.IsNullOrWhiteSpace(x.Description) ? $"[7/{x.Idr:D}]" : $"[7/{x.Idr:D}]: {x.Description}",
                                                                                  });
        }
    }
}
