using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TauManagerBot.Commands
{
    public class Ration : MessageHandlerBase
    {
        public Ration(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length != 1 || args[0] != "!ration") return MessageResponse.NotHandled();

            var rationService = _serviceProvider.GetRequiredService<IRationInfoService>();
            var result = rationService.GetCurrentPricePerTier();
            if (result == 0) return MessageResponse.Handled("Sorry, can't parse the ration price. Please try again later.");
            return MessageResponse.HandledFormat("Current Tier 1 Ration price on CSH: {0,7:F2} c", result);
        }
    }
}