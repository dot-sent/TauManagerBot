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
            if (result == null || result.Count() == 0) return MessageResponse.Handled("Sorry, no results found. Please try again.");
            var resultBuilder = new StringBuilder(string.Format("Found {0} fuel price(s):\n", result.Count()));
            foreach(var fuelInfo in result)
            {
                resultBuilder.AppendFormat("{0}/{1}: {2,7:F2}, recorded at {3:d}\n",
                    fuelInfo.System_Name,
                    fuelInfo.Station_Name,
                    fuelInfo.Last_Price,
                    fuelInfo.Last_Reading);
            }
            return MessageResponse.Handled(resultBuilder.ToString());
        }
    }
}