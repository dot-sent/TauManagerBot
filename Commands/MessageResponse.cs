namespace TauManagerBot.Commands
{
    public class MessageResponse
    {
        public bool MessageHandled { get; set; }
        public string Response { get; set; }

        public static MessageResponse Handled(string response)
        {
            return new MessageResponse{
                MessageHandled = true,
                Response = response
            };
        }

        public static MessageResponse HandledFormat(string format, params object[] args)
        {
            return new MessageResponse{
                MessageHandled = true,
                Response = string.Format(format, args)
            };
        }

        public static MessageResponse NotHandled()
        {
            return new MessageResponse{
                MessageHandled = false
            };
        }
    }
}