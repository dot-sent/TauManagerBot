using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TauManager.BusinessLogic;

namespace TauManagerBot.Commands
{
    public class Campaign : MessageHandlerBase
    {
        public Campaign(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length < 1) return MessageResponse.NotHandled();
            if (args[0] != "!campaign") return MessageResponse.NotHandled();
            if (args.Length == 1) // Just campaign info
            {
                using(var scope = _serviceProvider.CreateScope())
                {
                    var campaignLogic = scope.ServiceProvider.GetRequiredService<ICampaignLogic>();
                    var nextCamp = campaignLogic.GetNextCampaignByDiscordLogin(message.FullDiscordAuthorLogin());
                    if (nextCamp == null) return MessageResponse.Handled("No future campaign(s) found for your syndicate or account not connected.");
                    return MessageResponse.HandledFormat("Syndicate [{0}] has its next campaign planned at {1} ({2} UTC), at {3}.\nIt will be T{4} {5} campaign, organized by {6}.",
                        nextCamp.Syndicate.Tag,
                        nextCamp.GCTDateString,
                        nextCamp.UTCDateString,
                        nextCamp.Station,
                        nextCamp.TiersString,
                        nextCamp.Difficulty,
                        nextCamp.Manager.Name
                    );
                }
            }
            return MessageResponse.NotHandled();
        }
    }
}