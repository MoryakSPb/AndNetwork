﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Channels;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using AndNetwork.Shared.Programs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace AndNetwork.Server.Discord.Managers
{
    public class DiscordRoleManager
    {
        protected internal readonly DiscordBot Bot;

        public Dictionary<ClanDepartmentEnum, IRole> DepartmentRoles { get; protected set; } = new();
        public IRole EveryoneRole => Bot.Guild.EveryoneRole;
        public IRole DefaultRole { get; protected set; }
        public IRole AdvisorRole { get; protected set; }
        public IRole FirstAdvisorRole { get; protected set; }

        internal DiscordRoleManager(DiscordBot bot) => Bot = bot;

        public async Task InitRoles(IReadOnlyCollection<IRole> roles = null)
        {
            roles ??= Bot.Guild.Roles.OfType<IRole>().ToArray();


            string firstAdvisorName = Enum.GetValues<ClanMemberRankEnum>().Max().GetRankName();
            FirstAdvisorRole = roles.FirstOrDefault(x => x.Name == firstAdvisorName) ?? await Bot.Guild.CreateRoleAsync(firstAdvisorName, GuildPermissions.None, Color.Red, true, true, RequestOptions.Default);
            DepartmentRoles.Clear();
            foreach (ClanDepartmentEnum department in Enum.GetValues<ClanDepartmentEnum>().Where(x => x != ClanDepartmentEnum.None))
            {
                string name = department.GetName();
                IRole role = roles.FirstOrDefault(x => x.Name == name) ?? await Bot.Guild.CreateRoleAsync(name, GuildPermissions.None, department.GetDiscordColor(), true, true, RequestOptions.Default);
                DepartmentRoles.Add(department, role);
            }

            const string defaultRoleName = "Участник клана";
            DefaultRole = roles.FirstOrDefault(x => x.Name == defaultRoleName) ?? await Bot.Guild.CreateRoleAsync(defaultRoleName, GuildPermissions.None, null, false, false, RequestOptions.Default);

            const string advisorRoleName = "Советник клана";
            AdvisorRole = roles.FirstOrDefault(x => x.Name == advisorRoleName) ?? await Bot.Guild.CreateRoleAsync(advisorRoleName, GuildPermissions.None, null, false, false, RequestOptions.Default);
        }

        public async Task UpdateRoles(ClanMember member, StringBuilder logText = null)
        {
            logText ??= new StringBuilder();
            SocketGuildUser user = Bot.Guild.Users.FirstOrDefault(x => x.Id == member.DiscordId);
            if (user is null)
            {
                logText.AppendLine($"{member} is not a member of the Discord server");
                return;
            }

            List<IRole> userRoles = user.Roles.Cast<IRole>().ToList();
            if (member.Rank > ClanMemberRankEnum.None)
            {
                List<IRole> validRoles = new()
                                         {
                                             DefaultRole,
                                         };
                if (user.Hierarchy == int.MaxValue) validRoles.Add(FirstAdvisorRole);
                if (member.Rank >= ClanMemberRankEnum.Advisor) validRoles.Add(AdvisorRole);
                if (member.Department != ClanDepartmentEnum.None) validRoles.Add(DepartmentRoles[member.Department]);

                List<IRole> invalidRoles = userRoles.Except(validRoles).ToList();
                validRoles = validRoles.Except(userRoles).ToList();


                invalidRoles.Remove(EveryoneRole);
                if (invalidRoles.Count > 0) await user.RemoveRolesAsync(invalidRoles);
                if (validRoles.Count > 0) await user.AddRolesAsync(validRoles);

                string nickname = member.Rank >= ClanMemberRankEnum.Neophyte ? member.ToString() : null;
                if (user.Nickname == nickname) return;
                if (user.Hierarchy == int.MaxValue) logText.AppendLine($"Bot cannot change nickname for user {user}. Change it manually to «{nickname}»");
                else await user.ModifyAsync(x => x.Nickname = nickname, RequestOptions.Default);
            }
            else if (!user.IsBot)
            {
                userRoles.Remove(EveryoneRole);
                if (userRoles.Count > 0) await user.RemoveRolesAsync(userRoles);
            }
        }

        public async Task UpdateDruzhina(ClanDruzhina druzhina, ClanContext data)
        {
            Dictionary<ClanDepartmentEnum, ClanMember> advisors = await data.Members.AsQueryable().Where(x => x.Rank == ClanMemberRankEnum.Advisor).ToDictionaryAsync(x => x.Department, x => x);
            foreach (DiscordChannelMetadata meta in data.Channels.AsQueryable().Where(x => x.DruzhinaId == druzhina.Id).ToArray()) await UpdateChannel(Bot.Guild.GetChannel(meta.DiscordId), meta, advisors);
        }

        public async Task UpdateProgram(ClanProgram program, ClanContext data)
        {
            foreach (DiscordChannelMetadata meta in data.Channels.AsQueryable().Where(x => x.ProgramId == program.Id).ToArray()) await UpdateChannel(Bot.Guild.GetChannel(meta.DiscordId), meta);
        }

        private async Task UpdateChannel(SocketGuildChannel channel, DiscordChannelMetadata meta, IReadOnlyDictionary<ClanDepartmentEnum, ClanMember> advisors = null)
        {
            await channel.ModifyAsync(options => { options.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(meta.ToOverwrites(this, advisors)); });
        }
    }
}
