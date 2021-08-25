using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Managers;
using AndNetwork.Server.Discord.Utility;
using AndNetwork.Server.Import.AndNet7;
using AndNetwork.Shared;
using AndNetwork.Shared.Elections;
using AndNetwork.Shared.Enums;
using AndNetwork.Shared.Programs;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server.Discord
{
    public class DiscordBot : DiscordSocketClient, IHostedService
    {
        private readonly DiscordChannelManager _channelManager;
        private readonly DiscordRoleManager _roleManager;
        private readonly IServiceScopeFactory _scopeFactory;
        protected internal readonly DiscordConfiguration Configuration;
        protected internal readonly ElectionsService ElectionsService;

        public readonly ulong GuildId;
        protected internal readonly DiscordMessagesManager MessagesManager;
        protected readonly string Token;

        private bool _syncRoot;

        internal ILogger<DiscordBot> Logger { get; }

        public SocketGuild Guild { get; protected set; }

        public DiscordBot(ILogger<DiscordBot> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory, ElectionsService electionsService, DiscordConfiguration discordConfiguration) : base(new DiscordSocketConfig
                                                                                                                                                                                                            {
                                                                                                                                                                                                                LogLevel = LogSeverity.Info,
                                                                                                                                                                                                                DefaultRetryMode = RetryMode.AlwaysRetry,
                                                                                                                                                                                                                LargeThreshold = 250,
                                                                                                                                                                                                                RateLimitPrecision = RateLimitPrecision.Millisecond,
                                                                                                                                                                                                                UseSystemClock = true,
                                                                                                                                                                                                                AlwaysDownloadUsers = true,
                                                                                                                                                                                                            })
        {
            Log += OnLog;
            Logger = logger;
            Token = configuration["Token:Discord"];
            GuildId = ulong.Parse(configuration["Id:Discord"]);
            _scopeFactory = scopeFactory;
            _roleManager = new DiscordRoleManager(this);
            _channelManager = new DiscordChannelManager(this, _roleManager);
            MessagesManager = new DiscordMessagesManager(this);
            ElectionsService = electionsService;
            Configuration = discordConfiguration;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            UserJoined += OnUserJoined;
            await LoginAsync(TokenType.Bot, Token);
            await base.StartAsync();
            Status = UserStatus.Online;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Status = UserStatus.Offline;
            await base.StopAsync();
            await LogoutAsync();
        }

        private Task OnUserJoined(SocketGuildUser user)
        {
            Task.Run(async () =>
            {
                using IDisposable _ = GetDatabaseConnection(out ClanContext data);
                ClanMember member = await data.Members.FirstOrDefaultAsync(x => x.DiscordId == user.Id);
                Logger.LogInformation($"User {user} joined to server!");

                if (member is not null)
                {
                    StringBuilder log = new();
                    await _roleManager.UpdateRoles(member, log);
                    Logger.LogInformation(log.ToString());
                }
            });
            return Task.CompletedTask;
        }

        private async Task OnLog(LogMessage message)
        {
            await Task.Run(() =>
            {
                if (message.Exception is null) Logger.Log(message.Severity.ToLogLevel(), message.Message);
                else Logger.Log(message.Severity.ToLogLevel(), message.Exception, message.Message);
            });
        }

        public async Task Import(ClanContext data)
        {
            const string filePath = "save.json";
            if (!File.Exists(filePath)) return;
            Save save = JsonSerializer.Deserialize<Save>(await File.ReadAllBytesAsync(filePath));
            ClanMember[] result = Importer.GetData(save).ToArray();

            foreach (ClanMember member in result)
            {
                if (data.Members.Any(x => x.DiscordId == member.DiscordId)) continue;
                await data.Members.AddAsync(member);
            }

            await data.SaveChangesAsync();
        }

        public async Task SyncGuild(ClanContext data)
        {
            if (_syncRoot)
            {
                Logger.LogInformation("Invalid sync");
                return;
            }

            _syncRoot = true;
            try
            {
                Guild ??= base.GetGuild(GuildId);
                await _roleManager.InitRoles();

                //await _channelManager.ScanChannels(data);

                StringBuilder log = new();
                bool result = true;
                foreach (ClanMember member in data.Members.ToArray())
                {
                    UpdateRank(member);
                    bool userResult = await _roleManager.UpdateRoles(member, log);
                    if (!userResult) result = false;
                }

                if (result)
                {
                    Logger.LogInformation(log.ToString());
                }
                else
                {
                    Logger.LogWarning(log.ToString());
                }
                await _channelManager.SyncChannels(data);
                await _channelManager.SortChannels(data);

                ClanElections elections = ElectionsService.CurrentElections = data.Elections.FirstOrDefault(x => x.Stage != ClanElectionsStageEnum.Ended);
                if (elections is null) await ElectionsService.NewElections(data);

                await ElectionsService.UpdateMessages(this, data);

                await data.SaveChangesAsync();
                Logger.LogInformation("Sync done!");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error during sync");
            }
            finally
            {
                _syncRoot = false;
            }
        }

        internal IDisposable GetDatabaseConnection(out ClanContext data)
        {
            IServiceScope result = _scopeFactory.CreateScope();
            data = (ClanContext)result.ServiceProvider.GetService(typeof(ClanContext));
            return result;
        }

        private static void UpdateRank(ClanMember member)
        {
            if (member.Druzhina is null)
            {
                if (member.Rank == ClanMemberRankEnum.Captain || member.Rank == ClanMemberRankEnum.Lieutenant) member.Rank = member.Awards.GetRank();
            }
            else
            {
                ClanDruzhinaMember druzhinaMember = member.Druzhina.ActiveMembers.Single(x => x.MemberId == member.Id);
                if (druzhinaMember.Position >= ClanDruzhinaPositionEnum.Lieutenant && (member.Rank != ClanMemberRankEnum.Captain || member.Rank != ClanMemberRankEnum.Lieutenant))
                    member.Rank = druzhinaMember.Position switch
                    {
                        ClanDruzhinaPositionEnum.Lieutenant => ClanMemberRankEnum.Lieutenant,
                        ClanDruzhinaPositionEnum.Captain => ClanMemberRankEnum.Captain,
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                else if (member.Rank == ClanMemberRankEnum.Captain || member.Rank == ClanMemberRankEnum.Lieutenant) member.Rank = member.Awards.GetRank();
            }
        }
    }
}
