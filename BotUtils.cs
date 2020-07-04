using Discord.WebSocket;

namespace TauManagerBot
{
    public static class BotUtils
    {
        public static string FullDiscordAuthorLogin(this SocketUserMessage message) =>
            message.Author.Username + "#" + message.Author.DiscriminatorValue.ToString();
    }
}