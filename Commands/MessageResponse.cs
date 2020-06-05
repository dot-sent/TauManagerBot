namespace TauManagerBot.Commands
{
    public class MessageResponse
    {
        public ulong ChannelId { get; set; }
        public bool MessageHandled { get; set; }
        public string Response { get; set; }

        public static MessageResponse Handled(string response, ulong channelId = 0)
        {
            return new MessageResponse{
                MessageHandled = true,
                Response = response,
                ChannelId = channelId
            };
        }

        public static MessageResponse HandledFormat(ulong channelId, string format, params object[] args)
        {
            return new MessageResponse{
                MessageHandled = true,
                Response = string.Format(format, args),
                ChannelId = channelId
            };
        }

        public static MessageResponse HandledFormat(string format, params object[] args)
        {
            return HandledFormat(0, format, args);
        }

        public static MessageResponse NotHandled()
        {
            return new MessageResponse{
                MessageHandled = false,
            };
        }
    }
}