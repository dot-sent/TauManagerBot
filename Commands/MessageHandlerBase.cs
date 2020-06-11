using System;
using Discord.WebSocket;

namespace TauManagerBot.Commands
{
    public abstract class MessageHandlerBase: IMessageHandler
    {
        protected IServiceProvider _serviceProvider { get; set; }
        public MessageHandlerBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public abstract MessageResponse HandleMessage(string[] args, SocketUserMessage message);
    }
}