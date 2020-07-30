using System;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TauManagerBot.Commands
{
    public class FuelDebug : MessageHandlerBase
    {
        public FuelDebug(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override MessageResponse HandleMessage(string[] args, SocketUserMessage message)
        {
            if (args.Length == 0 || args[0] != "!fueldbg") return MessageResponse.NotHandled();

            if (args.Length < 2)
            {
                return MessageResponse.Handled("Please specify a valid station or system name.");
            }
            var stationOrSystemName = string.Join(' ', args.Skip(1));

            var sotherynFuelService = _serviceProvider.GetRequiredService<ISotherynPriceTrackerService>();
            var approximations = sotherynFuelService.GetFuelPricesDebug(stationOrSystemName);

            if (approximations == null || approximations.Count() == 0) return MessageResponse.Handled("Sorry, no results found. Please try again.");
            var resultBuilder = new StringBuilder(string.Format("Found {0} fuel price(s):\n", approximations.Count()));
            foreach(var fuelInfo in approximations)
            {
                resultBuilder.AppendFormat("Station {0}: {1}\n",
                    fuelInfo.Key,
                    fuelInfo.Value);
            }
            return MessageResponse.Handled(resultBuilder.ToString());
        }
    }
}