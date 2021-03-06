using System;
using Discord.WebSocket;

namespace TauManagerBot.Commands
{
    public class Help : MessageHandlerBase
    {
        public Help(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length != 1) return MessageResponse.NotHandled();
            if (args[0] != "!help") return MessageResponse.NotHandled();
            return MessageResponse.Handled(@"
Hello, I'm friendly TauManager Bot! *(Don't mind my avatar, I told you I'm friendly, so I am)*

I accept the following commands:

* !help - this command, lists help on all commands
* !connect *player_name* - connect your Discord account with your Manager account, needed if you want to setup notifications
* !disconnect - disconnect your Discord account from any Manager accounts it might be connected to
* !stats *syndicate_tag* - lists current statistics by tier for a given syndicate tag (if found)
* !fuel *station_or_system_tag* - lists last recorded fuel price at the given station or system
* !ration - gives the last recorded price of Tier 1 Ration at CSH vendors
* !subscribe *number* - Deprecated. Please use the settings page at https://dotsent.nl/Settings/Discord instead

Still got questions? Ping Dotsent!");
        }
    }
}