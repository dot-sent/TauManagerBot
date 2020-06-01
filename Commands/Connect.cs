using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TauManager.BusinessLogic;

namespace TauManagerBot.Commands
{
    public class Connect : MessageHandlerBase
    {
        public Connect(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length != 2) return MessageResponse.NotHandled();
            if (args[0] != "!connect") return MessageResponse.NotHandled();
            var playerName = args[1];
            using (var scope = _serviceProvider.CreateScope())
            {
                var playerLogic = scope.ServiceProvider.GetRequiredService<IPlayerLogic>();
                var fullName = message.Author.Username + "#" + message.Author.Discriminator;
                var result = playerLogic.RequestPlayerDiscordLink(playerName, fullName);
                if (string.IsNullOrEmpty(result))
                {
                    return MessageResponse.Handled("Subscription request failed. Possible causes: player name is not registered or the account is already connected. If the problem persists, please contact Dotsent.");
                }
                return MessageResponse.HandledFormat("Please follow this link to connect your Discord and Manager profiles: https://dotsent.nl/Settings/ConnectDiscordAccount?login={0}%23{1}&authCode={2}",
                    message.Author.Username,
                    message.Author.Discriminator,
                    result);
            }
        }
    }
}