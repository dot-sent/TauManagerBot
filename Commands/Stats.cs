using System;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TauManager;
using TauManager.BusinessLogic;

namespace TauManagerBot.Commands
{
    public class Stats : MessageHandlerBase
    {
        public Stats(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length == 0 || args[0] != "!stats") return MessageResponse.NotHandled();

            if (args.Length != 2)
            {
                return MessageResponse.Handled("Please specify a valid syndicate tag.");
            }
            var syndicateTag = args[1];
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TauDbContext>();
                var syndicate = dbContext.Syndicate.SingleOrDefault(s => s.Tag == syndicateTag);
                if (syndicate == null)
                {
                    return MessageResponse.HandledFormat("Can't find the syndicate with tag '{0}'", syndicateTag);
                } else {
                    var playerLogic = scope.ServiceProvider.GetRequiredService<IPlayerLogic>();
                    var syndicateMetrics = playerLogic.GetSyndicateMetrics(null, false, syndicate.Id);
                    var messageBuilder = new StringBuilder("Average stats for syndicate ");
                    messageBuilder.Append(syndicate.Tag);
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("---");
                    foreach(var tier in syndicateMetrics.PlayerStats.Keys.OrderByDescending(k => k))
                    {
                        var tierStats = syndicateMetrics.PlayerStats[tier];
                        messageBuilder.AppendFormat("Tier {0} - Player count: {1:d}, Total stat average {2:F2}, Total stat median {3:F2}",
                            tierStats.Tier,
                            tierStats.PlayerCount,
                            tierStats.StatTotal,
                            tierStats.StatTotalMedian);
                        messageBuilder.AppendLine();
                    }
                    return MessageResponse.Handled(messageBuilder.ToString());
                }
            }
        }
    }
}