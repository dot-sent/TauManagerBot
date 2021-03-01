using System;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TauManagerBot.Commands
{
    public class Fuel : MessageHandlerBase
    {
        public Fuel(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length == 0 || args[0] != "!fuel") return MessageResponse.NotHandled();

            if (args.Length < 2)
            {
                return MessageResponse.Handled("Please specify a valid station or system name.");
            }
            var stationOrSystemName = string.Join(' ', args.Skip(1));
            var fuelService = _serviceProvider.GetRequiredService<IFuelTrackerService>();
            var result = fuelService.GetPrices(stationOrSystemName);

            if (result == null || result.Count() == 0)
            {
                var allSystemNames = fuelService.GetValidSystemNames();
                var allSystemShortcuts = fuelService.GetValidSystemShortcuts();
                return MessageResponse.HandledFormat("No results found for system or station name '{0}'. Valid system names are: {1} or shortcuts: {2}.",
                        stationOrSystemName,
                        String.Join(", ", allSystemNames),
                        String.Join(", ", allSystemShortcuts)
                    );
            }
            var resultBuilder = new StringBuilder(string.Format("Found {0} fuel price(s):\n", result.Count()));
            resultBuilder.AppendLine("```");
            resultBuilder.AppendLine("┌──────────┬─────────┬──────────────────────┬─────────┐");
            resultBuilder.AppendLine("│  Station │  Price  │       Recorded       │   Est.  │");
            resultBuilder.AppendLine("├──────────┼─────────┼──────────────────────┼─────────┤");
            foreach(var fuelInfo in result)
            {
                resultBuilder.AppendFormat("│ {0,8} │ {1,7:F2} │ {2:u} │ {3,7:F2} │",
                    fuelInfo.Station_Short_Name,
                    fuelInfo.Last_Price,
                    fuelInfo.Last_Reading,
                    fuelInfo.Estimation.HasValue ?
                        String.Format("{0,7:F2}", (double)fuelInfo.Estimation.Value) :
                        "   N/A");
                resultBuilder.Append("\n");
            }
            resultBuilder.AppendLine("└──────────┴─────────┴──────────────────────┴─────────┘");
            resultBuilder.AppendLine("```");
            resultBuilder.Append("*Credits: data - moritz' TauTracker; approximation logic - Sotheryn; Python implementation - SandwichMaker*");
            return MessageResponse.Handled(resultBuilder.ToString());
        }
    }
}