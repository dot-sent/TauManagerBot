using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TauManager;
using TauManager.BusinessLogic;

namespace TauManagerBot.Commands
{
    public class Subscribe : MessageHandlerBase
    {
        public Subscribe(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length != 2) return MessageResponse.NotHandled();
            if (args[0] != "!subscribe") return MessageResponse.NotHandled();
            int flags;
            if (!int.TryParse(args[1], out flags)) return MessageResponse.Handled("Invalid flag parameter.");
            var service = _serviceProvider.GetRequiredService<IRegisteredDiscordUsersService>();
            var fullName = message.Author.Username + "#" + message.Author.Discriminator;
            if (!service.IsUserRegistered(fullName))
            {
                return MessageResponse.Handled("Please use !connect command first.");
            }
            using (var scope = _serviceProvider.CreateScope())
            {
                var playerLogic = scope.ServiceProvider.GetRequiredService<IPlayerLogic>();
                if (!playerLogic.SetPlayerNotificationByDiscord(fullName, flags))
                {
                    return MessageResponse.Handled("Subscription request failed, please re-check your flags or contact Dotsent.");
                }
            }
            return MessageResponse.Handled("Subscription request processed.");
        }
    }
}