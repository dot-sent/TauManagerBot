using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TauManager.BusinessLogic;
using TauManagerBot.Commands;

namespace TauManagerBot
{
    public class Worker : IHostedService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private DiscordSocketClient _client;
        private string _discordClientKey;
        private Dictionary<ulong, ulong> _discordGreetingChannels;
        private List<IMessageHandler> _handlers;
        private Timer _notificationCheckTimer;
        private IServiceProvider _serviceProvider;

        private INotificationQueueService _notificationQueueService;

        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            INotificationQueueService notificationQueueService,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _notificationQueueService = notificationQueueService;
            _handlers = new List<IMessageHandler>();
            _handlers.Add(new Admin(serviceProvider));
            _handlers.Add(new Stats(serviceProvider));
            _handlers.Add(new Connect(serviceProvider));
            _handlers.Add(new Disconnect(serviceProvider));
            _handlers.Add(new Subscribe(serviceProvider));
            _handlers.Add(new Help(serviceProvider));
            _handlers.Add(new Fuel(serviceProvider));
            _handlers.Add(new FuelDebug(serviceProvider));
            _handlers.Add(new Ration(serviceProvider));
            _handlers.Add(new Campaign(serviceProvider));

            _discordClientKey = configuration.GetValue<string>("DiscordClientKey");
            _discordGreetingChannels = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ulong>>(
                configuration.GetValue<string>("GreetingChannelIds")
            ).ToList().ToDictionary(kv => ulong.Parse(kv.Key), kv => kv.Value);
        }

        public void Dispose()
        {
            _notificationCheckTimer.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{time} Worker started", DateTimeOffset.Now);
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, _discordClientKey);
            await _client.StartAsync();
            _client.Ready += ClientReady;
            _client.MessageReceived += HandleCommandAsync;
            _client.UserJoined += HandleUserJoinedAsync;
            _notificationCheckTimer = new Timer(CheckNotificationQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private async Task HandleUserJoinedAsync(SocketGuildUser userInfo)
        {
            _logger.LogInformation("{time} User joined: '{1}' to guild `{2}`", DateTimeOffset.Now, userInfo.FullDiscordName(), userInfo.Guild.Name);
            if (_discordGreetingChannels.ContainsKey(userInfo.Guild.Id)) {
                _logger.LogInformation("{time} Attempting to send greeting message...", DateTimeOffset.Now);
                var channel = userInfo.Guild.GetTextChannel(_discordGreetingChannels[userInfo.Guild.Id]);
                _logger.LogInformation("{time} Channel identified...", DateTimeOffset.Now);
                await channel.SendMessageAsync(String.Format("Hello {0} and welcome to {1}! I'm your friendly local bot.\n\n" + 
                    "In order to receive member privileges, please confirm your identity via in-game chat first." + 
                    " Once you receive your member status, please refrain from using this channel for any" + 
                    " meaningful discussions, this channel is reserved for greeting new folks.\n\nCheers!",
                    userInfo.Username, userInfo.Guild.Name));
                }
            return;
        }

        private Task ClientReady()
        {
            _logger.LogInformation("{time} Client is Ready", DateTimeOffset.Now);
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            _logger.LogInformation("{time} Received message: '{1}'", DateTimeOffset.Now, message.Content);

            var messageObj = message as SocketUserMessage;
            if (message == null) return;
            if (_discordGreetingChannels.ContainsValue(message.Channel.Id)) return;

            int argPos = 0;

            if (!(messageObj.HasCharPrefix('!', ref argPos) || 
                messageObj.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var officerService = _serviceProvider.GetRequiredService<IRegisteredDiscordUsersService>();
            bool isOfficer = officerService.IsOfficer(messageObj.FullDiscordAuthorLogin());

            var args = messageObj.Content.Split(' ');
            foreach(IMessageHandler handler in _handlers)
            {
                var response = handler.HandleMessage(args, messageObj);
                if (response.MessageHandled)
                {
                    var dmChannel = await messageObj.Author.GetOrCreateDMChannelAsync();
                    if (response.ChannelId != 0)
                    {
                        var explicitChannel = _client.GetChannel(response.ChannelId);
                        if (explicitChannel == null)
                        {
                            await dmChannel.SendMessageAsync(string.Format("Can't find channel with id {0}, message not sent.", response.ChannelId));
                        } else {
                            var textChannel = explicitChannel as ISocketMessageChannel;
                            if (textChannel == null)
                            {
                                await dmChannel.SendMessageAsync(string.Format("Channel with id {0} is NOT ISocketMessageChannel, message not sent.", response.ChannelId));
                            } else {
                                await textChannel.SendMessageAsync(response.Response);
                            }
                        }
                    } else {
                        var channel = messageObj.Channel;
                        if (isOfficer)
                        {
                            await channel.SendMessageAsync(response.Response);
                        } else {
                            if (channel.Id != dmChannel.Id)
                            {
                                await dmChannel.SendMessageAsync("Hello there! This bot currently only responds via DM channels, please use this one in the future.");
                            }
                            await dmChannel.SendMessageAsync(response.Response);
                        }
                    }
                    return;
                }
            }
        }

        private async void CheckNotificationQueue(object state)
        {
            while (_notificationQueueService.Any())
            {
                var notification = _notificationQueueService.Dequeue();
                if (notification != null) 
                {
                    var userParts = notification.RecipientDiscordLogin.Split('#');
                    if (userParts.Length == 2)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var notificationLogic = scope.ServiceProvider.GetRequiredService<INotificationLogic>();
                            try {
                                var user = _client.GetUser(userParts[0], userParts[1]);
                                var channel = await user.GetOrCreateDMChannelAsync();
                                var result = await channel.SendMessageAsync(notification.Message);
                                notificationLogic.ReportNotificationStatus(notification.Id, true, string.Empty);
                            } catch (Exception e) {
                                notificationLogic.ReportNotificationStatus(notification.Id, false, DateTime.Now.ToLongTimeString() + ": " + e.ToString());
                            }
                        }
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
            _logger.LogInformation("{time} Worker stopped", DateTimeOffset.Now);
        }
    }
}
