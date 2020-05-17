using Discord.WebSocket;

namespace TauManagerBot.Commands
{
    public interface IMessageHandler
    {
        MessageResponse HandleMessage(string[] messageParts, SocketUserMessage message);
    }
}