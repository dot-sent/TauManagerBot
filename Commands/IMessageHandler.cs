using Discord.WebSocket;

namespace TauManagerBot.Commands
{
    public interface IMessageHandler
    {
        MessageResponse HandleMessage(string[] args, SocketUserMessage message);
    }
}