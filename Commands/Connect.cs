using System;
using Discord.WebSocket;

namespace TauManagerBot.Commands
{
    public class Connect : MessageHandlerBase
    {
        public Connect(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length != 1) return MessageResponse.NotHandled();
            if (args[0] != "!connect") return MessageResponse.NotHandled();
            return MessageResponse.HandledFormat("Please follow this link to connect your Discord and Manager profiles: https://dotsent.nl/Settings/ConnectDiscordAccount?login={0}%23{1}", message.Author.Username, message.Author.Discriminator);
        }
    }
}