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
                var syndicate = dbContext.Syndicate.SingleOrDefault(s => syndicateTag.ToLower() == s.Tag.ToLower());
                if (syndicate == null)
                {
                    return MessageResponse.HandledFormat("Can't find the syndicate with tag '{0}'", syndicateTag);
                } else {
                    var playerLogic = scope.ServiceProvider.GetRequiredService<IPlayerLogic>();
                    var syndicateMetrics = playerLogic.GetSyndicatePlayers(null, false, syndicate.Id);
                    var messageBuilder = new StringBuilder("Average stats for syndicate ");
                    messageBuilder.Append(syndicate.Tag);
                    messageBuilder.AppendLine();
                    messageBuilder.Append("```");
                    messageBuilder.AppendLine("Tier | Players |  Total avg (STR/AGI/STA)  | TS median");
                    messageBuilder.AppendLine("-----+---------+---------------------------+----------");
                    foreach(var tier in syndicateMetrics.PlayerStats.Keys.OrderByDescending(k => k))
                    {
                        var tierStats = syndicateMetrics.PlayerStats[tier];
                        messageBuilder.AppendFormat("  {0}  |    {1,2:d}   | {2,4:d} ({3,4:d} / {4,4:d} / {5,4:d}) |  {6,4:d}",
                            tierStats.Tier,
                            tierStats.PlayerCount,
                            (int)Math.Round(tierStats.StatTotal),
                            (int)Math.Round(tierStats.Strength),
                            (int)Math.Round(tierStats.Agility),
                            (int)Math.Round(tierStats.Stamina),
                            (int)Math.Round(tierStats.StatTotalMedian));
                        messageBuilder.AppendLine();
                    }
                    messageBuilder.Append("```");
                    return MessageResponse.Handled(messageBuilder.ToString());
                }
            }
        }
    }
}