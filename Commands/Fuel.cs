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

            var sotherynFuelService = _serviceProvider.GetRequiredService<ISotherynPriceTrackerService>();
            var approximations = sotherynFuelService.GetFuelPrices(stationOrSystemName);

            if (result == null || result.Count() == 0) return MessageResponse.Handled("Sorry, no results found. Please try again.");
            var resultBuilder = new StringBuilder(string.Format("Found {0} fuel price(s):\n", result.Count()));
            foreach(var fuelInfo in result)
            {
                resultBuilder.AppendFormat("{0}/{1}: {2,7:F2}, recorded at {3:u}",
                    fuelInfo.System_Name,
                    fuelInfo.Station_Name,
                    fuelInfo.Last_Price,
                    fuelInfo.Last_Reading);
                if (approximations.ContainsKey(fuelInfo.Station_Short_Name))
                {
                    resultBuilder.AppendFormat("; approximated price at the moment: {0,7:F2}",
                        (double)approximations[fuelInfo.Station_Short_Name]);
                }
                resultBuilder.Append("\n");
            }
            if (approximations != null && approximations.Count > 0)
            {
                resultBuilder.Append("\nCredits for approximation logic: Sotheryn");
            }
            return MessageResponse.Handled(resultBuilder.ToString());
        }
    }
}