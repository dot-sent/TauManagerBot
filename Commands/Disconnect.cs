using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TauManager.BusinessLogic;

namespace TauManagerBot.Commands
{
    public class Disconnect : MessageHandlerBase
    {
        public Disconnect(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length != 1) return MessageResponse.NotHandled();
            if (args[0] != "!disconnect") return MessageResponse.NotHandled();
            using (var scope = _serviceProvider.CreateScope())
            {
                var playerLogic = scope.ServiceProvider.GetRequiredService<IPlayerLogic>();
                var fullName = message.Author.Username + "#" + message.Author.Discriminator;
                var result = playerLogic.DisconnectDiscordAccountByDiscordLogin(fullName);
                if (!result)
                {
                    return MessageResponse.Handled("Failed to disconnect your account: maybe it's not connected? If the problem persists, please contact Dotsent.");
                }
                return MessageResponse.HandledFormat("Your Discord account has been successfully disconnected. Please use !connect <player_name> if you want to reconnect.");
            }
        }
    }
}