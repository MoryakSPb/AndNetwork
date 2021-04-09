using System;
using System.Threading;
using System.Threading.Tasks;
using AndNetwork.Server.Discord.Commands;
using AndNetwork.Server.Discord.Utility;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server.Discord
{
    public class DiscordCommandService : IHostedService, IDisposable
    {
        private readonly DiscordBot _client;
        private readonly CommandService _commands;
        private readonly ILogger<DiscordCommandService> _logger;
        private readonly IServiceScope _scope;

        public DiscordCommandService(DiscordBot client, IServiceScopeFactory scopeFactory, ILogger<DiscordCommandService> logger)
        {
            _commands = new CommandService(new CommandServiceConfig
                                           {
                                               LogLevel = LogSeverity.Verbose,
                                               CaseSensitiveCommands = false,
                                               DefaultRunMode = RunMode.Async,
                                               SeparatorChar = ' ',
                                               ThrowOnError = false,
                                           });
            _client = client;
            _scope = scopeFactory.CreateScope();
            _logger = logger;
        }

        public void Dispose()
        {
            ((IDisposable)_commands)?.Dispose();
            _scope?.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken) => await InstallCommandsAsync().ConfigureAwait(false);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived -= HandleCommandAsync;
            return Task.CompletedTask;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModuleAsync<DiscordAdminCommands>(_scope.ServiceProvider);
            await _commands.AddModuleAsync<DiscordUserCommands>(_scope.ServiceProvider);
            await _commands.AddModuleAsync<DiscordAwardCommands>(_scope.ServiceProvider);
            await _commands.AddModuleAsync<DiscordElectionsCommands>(_scope.ServiceProvider);
            await _commands.AddModuleAsync<DiscordSendCommands>(_scope.ServiceProvider);
            await _commands.AddModuleAsync<DiscordRootCommands>(_scope.ServiceProvider);
        }

        private Task HandleCommandAsync(SocketMessage messageParam)
        {
            Task.Run(() => HandleCommand(messageParam));
            return Task.CompletedTask;
        }

        private async void HandleCommand(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message) return;
            int argPos = 0;
            if (message.Author.IsBot || !message.HasCharPrefix('/', ref argPos)) return;
            SocketCommandContext context = new(_client, message);
            IDisposable typing = context.Channel.EnterTypingState();

            try
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _scope.ServiceProvider).ConfigureAwait(true);
                if (!result.IsSuccess)
                {
                    _logger.LogInformation($"{context.User} send wrong command \"{message.Content}\": «{result.Error}/{result.ErrorReason}»");
                    await context.Message.ReplyAsync(result.Error?.GetLocalizedString() ?? "Неизвестная ошибка").ConfigureAwait(true);
                }
                else
                    _logger.LogInformation($"{message.Author} send \"{message.Content}\"");
            }
            finally
            {
                typing.Dispose();
            }
        }
    }
}
